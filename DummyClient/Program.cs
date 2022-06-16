using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint end)
        {
            Console.WriteLine($"OnConnected : {end}");

            for (int i = 0; i < 5; i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"서버야.. 자니..? {i}");
                Send(sendBuff);
            }
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint end)
        {
            Console.WriteLine($"OnDisconnected : {end}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvDa = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client: ] {recvDa}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");

        }
    }
    public class Program
    {
        static void Main(string[] args)
        {
            // DNS 
            string host = Dns.GetHostName(); // 내 로컬 컴퓨터의 호스트 이름
            IPHostEntry iPHost = Dns.GetHostEntry(host); // 
            IPAddress ipAddr = iPHost.AddressList[0]; // 원하던 아이피 주소, 아이피가 여러개이기에 배열
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 2222); // 식당 주소, 뒷문 후문인지는 똑같음

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });


            while (true)
            {
                try
                {
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
