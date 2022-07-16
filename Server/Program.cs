using ServerCore;
using System.Net;
using System.Text;


namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void Main(string[] args)
        {
            // DNS (Domain Name System) 
            // 하드 코딩으로 IP를 등록해두면 나중에 교체 등의 문제가 생길 수 있어 도메인으로 등록

            // DNS 
            string host = Dns.GetHostName(); // 내 로컬 컴퓨터의 호스트 이름
            IPHostEntry iPHost = Dns.GetHostEntry(host); // 
            IPAddress ipAddr = iPHost.AddressList[0]; // 원하던 아이피 주소, 아이피가 여러개이기에 배열
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 2222); // 2222는 포트 번호 (클라이언트가 접속하는 번호와 같아야함)

            // 문지기가 들고 있는 핸드폰
            //Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            _listener.Init(endPoint, () => { return SessionManager.instance.Generate(); });
            Console.WriteLine("Listening .... ");

            // 영업을 손님 받을 때 까지 해야하니 무한루프
            while (true)
            {

                // 손님의 문의가 왔을 때 입장
                // -> 만약 손님이 입장을 안한다면? 실제는 이렇게 하진 않음 클라이언트가 입장안하면 아래는 안하고, 입장 해야만 할거임 
                //Socket clientSocket = _listener.Accept(); // 리턴 값 소켓
            }

        }
    }

}
