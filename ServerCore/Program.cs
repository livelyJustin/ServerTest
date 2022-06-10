using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Program
    {
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
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 혹시 모르니 try catch
            try
            {
                // 문지기 교육
                listenSocket.Bind(endPoint);

                // 영업 시작
                // backlog = 최대 대기수(매개변수) 초과 시 fail 뜸
                listenSocket.Listen(10);

                // 영업을 손님 받을 때 까지 해야하니 무한루프
                while (true)
                {
                    Console.WriteLine("Listening .... ");

                    // 손님의 문의가 왔을 때 입장
                    // -> 만약 손님이 입장을 안한다면? 실제는 이렇게 하진 않음 클라이언트가 입장안하면 아래는 안하고, 입장 해야만 할거임 
                    Socket clientSocket = listenSocket.Accept(); // 리턴 값 소켓(손님의 친구)

                    // 받는다
                    byte[] recvBuff = new byte[1024]; // 좀 크게 생성
                    int recvBytes = clientSocket.Receive(recvBuff); // 받은 정보는 recv에 넣고 몇 바이트인지는 recvBytes 저장
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);  // 변환하고자 하는 값, 시작 인덱스, 문자열 몇개짜리인지 설정
                                                                                        // 문자열을 받는다는 가정이기에 이렇게 간단
                                                                                        // 문자열로 소통하기에 endcording; UTF-8로 통일
                    Console.WriteLine($"[From Client] {recvData}");
                    // 보낸다

                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to Justin Server"); // 문자열을 바로 보낼 수 있는 타입으로 바꿔준거
                    clientSocket.Send(sendBuff);

                    // 끝낸다.
                    clientSocket.Shutdown(SocketShutdown.Both); // 조금 더 우아하게 쫓아내는 방법? 나중에 알려줌
                    clientSocket.Close();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}