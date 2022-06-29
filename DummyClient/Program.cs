using ServerCore;
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

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new ServerSession(); });

            while (true)
            {
                try
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Somthing Wrong... {e}");
                }

                Thread.Sleep(1000);
            }

        }
    }
}
