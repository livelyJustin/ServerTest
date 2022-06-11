using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // 서버 생성
            string host = Dns.GetHostName();
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = iPHost.AddressList[0];
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 2222);

            // 소켓 생성
            Socket server = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 받아주기
            server.Bind(iPEndPoint);

            // 대기
                server.Listen();
            while(true)
            {
                Console.WriteLine("접속 허가");
                // 승인
                Socket clientSocket = server.Accept();

                // 받기
                byte[] recvBusff = new byte[1024];
                int recvData = clientSocket.Receive(recvBusff);
                string recvDa = Encoding.UTF8.GetString(recvBusff, 0, recvData);

                Console.WriteLine($"손님왔다잉 왔다잉 {recvDa}");
                // 보내기
                byte[] sendBusff = Encoding.UTF8.GetBytes("너 임마 잘 왔어 서비스 많이 줄게");
                clientSocket.Send(sendBusff);

                // 끝내기


            }

        }
    }

}
