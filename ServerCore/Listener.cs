using System.Net.Sockets;
using System.Net;


namespace ServerCore
{
    // 코드가 커질 경우를 위해 옮겨 주기
    public class Listener
    {
        Socket _listenSocket;
        //Action<Socket> _onAcceptHandler;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 문지기 교육
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog = 최대 대기수(매개변수) 초과 시 fail 뜸
            _listenSocket.Listen(10);

            // 최초 한 번은 args를 생성하고 이벤트 등록
            // 그 후로는 계속 재사용 가능
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptComplete);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            // 비동기 처리이긴하나 성패의 여유와 관계 없이 리턴해버림
            // 완료 처리가 없이 되어버리니 문제가 생김
            //_listenSocket.AcceptAsync();

            // 요청을 하면 알아서 콜백을 해줌
            // 하지만 정말 만약에 성공인데 실행과 동시에 클라이언트가 
            // 접속 요청을 한다면 pending 값은 false 이기에 직접 실행 해줘야함
            // 만약 pending이 true 라면 어떻게든 나에게 뭐가올것
            bool pending = _listenSocket.AcceptAsync(args);
            if(pending == false)
            {
                OnAcceptComplete(null, args);
            }
        }


        // 결국 accpet가 되면 어떻게든 일로옴
        // pending 처리가 되던, args가 알아서 하던
        void OnAcceptComplete(Object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session _session = _sessionFactory.Invoke();
                _session.Start(args.AcceptSocket);

                _session.OnConnected(args.AcceptSocket.RemoteEndPoint);

                //_onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
                Console.WriteLine(args.SocketError.ToString()); ;

            // 뭐든 일이 끝났으니 다음 턴을 위해 다시 넣어주기
            RegisterAccept(args);
        }

        public Socket Accept()
        {
            // 블로킹 계열의 함수를 사용하는게 문제
            // 블로킹을 하게 되면 무한 대기를 할 수 있음
            // 리시브, 샌드에서도 계속 블록한다면 문제 될 수 있음
            // 비동기(논블로킹) 으로 해야함

            return _listenSocket.Accept();
        }
    }
}
