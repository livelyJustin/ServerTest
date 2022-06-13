using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            // DNS 
            string host = Dns.GetHostName(); // 내 로컬 컴퓨터의 호스트 이름
            IPHostEntry iPHost = Dns.GetHostEntry(host); // 
            IPAddress ipAddr = iPHost.AddressList[0]; // 원하던 아이피 주소, 아이피가 여러개이기에 배열
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 2222); // 식당 주소, 뒷문 후문인지는 똑같음

            // 연락할 휴대폰 생성

            while (true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 입장 요청 endpoint 로 입장 문의를하는것
                    socket.Connect(endPoint);  // 주의 실제로 이러면 계속 대기함
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()} ");
                    // 어떤 녀석한테 연결 됐나 확인

                    // 보낸다음 받을 예정
                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"서버야.. 자니..? {i}"); // string을 bytes 타입으로 변경
                        int sendBytes = socket.Send(sendBuff); // 주의 실제로 이러면 계속 대기함
                    }


                    // 받는다 서버가 나한테 얼마나 보낼지 모르늬 크게 만든다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff); // 주의 실제로 이러면 계속 대기함
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server ] {recvData}");

                    //
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Somthing Wrong...");
                }

                Thread.Sleep(1000);
            }

        }
    }
}
