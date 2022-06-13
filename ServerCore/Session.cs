using System.Net.Sockets;
using System.Text;


namespace ServerCore
{
    internal class Session
    {
        Socket _socket;
        int _disconnected = 0;

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            // 자동 콜백
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // 유저 정보를 넣을 수 있음(식별자)
            //            recvArgs.UserToken = this;
            // receive는 버퍼를 만들어줘야함
            // 버퍼의 크기는? 시작은? 상황에 따라 다를 수 있음)
            // 버퍼를 크게 만들어서 세션끼리 나누어 사용할 수 있어 위치 필요 
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv(recvArgs);
        }

        // 샌드는 리시브와 다르게 조금 더 복잡할 수 있음
        public void Send(byte[] sendbuff)
        {
            _socket.Send(sendbuff);
        }

        // 동시에 Disconnect를 하거나
        // 두번 연속으로 진입하면 오류가 나서 한 번만 실행하도록 해야한다.

        public void Disconnect()
        {
            if ((Interlocked.Exchange(ref _disconnected, 1)) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            // 비동기
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
            {
                OnRecvCompleted(null, args);
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 몇 바이트가 오는가? 상대가 연결을 끄면 0이 올 수 있음
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                // todo

                try
                {
                    // 위에서 설정한 값을 불러올 수 있음
                    string recvDa = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client: ] {recvDa}");
                    RegisterRecv(args);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Fail {e}");
                }
            }
            else
            {
                // todo

            }
        }
        #endregion
    }
}
