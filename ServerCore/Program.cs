using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);

            for (int i = 0; i < 3; i++)
            {
                Task t = new Task(() => { while (true) { } },TaskCreationOptions.LongRunning);
                t.Start();
            }

            ThreadPool.QueueUserWorkItem(MainThread);

            while(true)
            {

            }
        }

        static void MainThread(object state)
        {
            Console.WriteLine("메인 쓰레드 하이용");
        }
    }
}