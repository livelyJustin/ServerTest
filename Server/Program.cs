using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCore;


namespace Server
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint end)
        {
            Console.WriteLine($"OnConnected : {end}");

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Justin Server"); // 문자열을 바로 보낼 수 있는 타입으로 바꿔준거
            Send(sendBuff);

            Thread.Sleep(1000);

            Disconnect();
        }

        public override void OnDisconnected(EndPoint end)
        {
            Console.WriteLine($"OnDisconnected : {end}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvDa = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server: ] {recvDa}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");

        }
    }

    class Program
    {
        static Listener _listener = new Listener();

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


            //_listener.Init(endPoint, OnAcceptEventHandler)
            _listener.Init(endPoint, () => { return new GameSession(); });
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
