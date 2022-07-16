using System.Net;
using System.Net.Sockets;


namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;
        // 패킷이 왔다는 건 시작은 size, 두 번 째는 id, 나머지는 내용일 것

        // sealed 다른 Class가 PacketSession을 상속받고, 이 함수를 사용하고자 할 때는 할 수가 없음 봉인임
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            // 내가 원하는 패킷이 다 올 때까지 기다렸다가 처리 할 예정
            int processLegnth = 0;

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인( 2 바이트는 받아오는 Size 크기 ) 
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 체크
                // ushort만큼 긁어줄 예정 
                ushort datasize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                // 패킷이 다 안왔으면 대기
                if (buffer.Count < datasize)
                    break;

                // 위 조건들을 통과했으면 이제 패킷 조립가능 ArraySegment struct 구조이기에 new를 써도 heap에 할당되지 않기에 괜찮다
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, datasize)); // buffer.Slice()도 가능

                processLegnth += datasize;

                // 위에 작업들을 모두 통과했다면 첫 번 째 패킷은 끝난거기에 다음 패킷으로 넘겨버림
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + datasize, buffer.Count - datasize);
            }

            return processLegnth;
        }
        // sealed한 class 대신에 이걸 받아와 쓰도록 구현
        public abstract void OnRecvPacket(ArraySegment<byte> buffer);


    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();

        // 실제로 바로 sendAsyn를 하는게 아나리 담아두었다가 꺼냈다가 할 큐
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        //bool _pending = false; // 실행 중일 때는 바로 실행하지 않고, 큐에다가 저장해놓기 위함
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        // 재사용을 할 수 없다는게 단점 -> 멤버 변수로 선언하여 사용
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint end);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint end);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            // 자동 콜백
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            //_recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv();
        }

        // 샌드는 리시브와 다르게 조금 더 복잡할 수 있음
        public void Send(ArraySegment<byte> sendBuff)
        {
            // 멀티 쓰레드환경이기에 동시에 상호배제, 임계구역을 만들기 위한 lock
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }

        // 동시에 Disconnect를 하거나
        // 두번 연속으로 진입하면 오류가 나서 한 번만 실행하도록 해야한다.

        public void Disconnect()
        {
            if ((Interlocked.Exchange(ref _disconnected, 1)) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            if (_disconnected == 1)
                return;

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            try
            {
                // _sendArgs.BufferList여기에 한 번에 처리할 것이 담겨서 아래에서 성공한다면 해당 일감들을 처리한다.
                bool pending = _socket.SendAsync(_sendArgs);

                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

        // 이벤트를 통한 동작, pending == false일 때 동작 총 2개의 방법으로 동작하기 lock 처리해주어야함
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        // queue에 남아있는 항목이 있다면 register에서 다시 뽑아서 사용
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                        //else
                        //    _pending = false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Fail {e}");
                    }
                }
                else
                    Disconnect();
            }
        }
        // 여기까지 들어왔다면 성공적으로 처리 된 것이니
        // 다시 들어올 수 있게 pending을 false로 해준다.


        void RegisterRecv()
        {
            if (_disconnected == 1)
                return;

            _recvBuffer.Clean();
            // 초기에 설정한 버퍼로 사용하는 것이 아닌 유동적으로 설정
            // write가 되어있는 영역 부터 작업 실행
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                // 비동기
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Fail {e}");
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 몇 바이트가 오는가? 상대가 연결을 끄면 0이 올 수 있음
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 콘텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리 했는지 받는다.
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }
                    //OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));

                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();

                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Fail {e}");
                }
            }
            else
            {
                // todo
                Disconnect();
            }
        }
        #endregion
    }
}
