using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        // 기존에 메인이라는 직원(쓰레드)이 있었고
        static void Main(string[] args)
        {
            // thread라는 새로운 직원을 뽑아 어떤 것을 하라는 Start 명령을 내린 것이다.
            Thread thread = new Thread(MainThread);
            thread.IsBackground = true;
            thread.Name = "FirstThread";
            thread.Start();
            Console.WriteLine("기다리는 중메인 하이용");

            thread.Join();
            Console.WriteLine("그냥 메인 하이용");
        }

        static void MainThread()
        {
            int t = 0;
            while(t < 3)
            {

                Console.WriteLine("메인 쓰레드 하이용");
            t++;
            }
        }
    }

}
