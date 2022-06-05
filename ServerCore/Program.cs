namespace ServerCore
{
    public class Program
    {
        static Mutex mutex = new Mutex();
        public static int num = 0;

        static void Thread_1()
        {
            for (int i = 0; i < 1000; i++)
            {
                mutex.WaitOne();
                num++;
                mutex.ReleaseMutex();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 1000; i++)
            {
                mutex.WaitOne();
                num--;
                mutex.ReleaseMutex();
            }
        }

        static void Main(string[] args)
        {
            Task t = new Task(Thread_1);
            Task t1 = new Task(Thread_2);

            t.Start();
            t1.Start();

            Task.WaitAll(t, t1);
            Console.WriteLine(num);
        }
    }
}