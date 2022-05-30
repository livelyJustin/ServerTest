namespace ServerCore
{
    class Program
    {
        static int num = 0;
        static object key = new object();
        static void Thread1()
        {
            lock (key)
            {
                for (int i = 0; i < 10000; i++)
                {
                    num++;
                    return;
                }
            }

        }
        static void Thread2()
        {
            lock (key)
            {
                for (int i = 0; i < 10000; i++)
                {
                    num--;
                    return;
                }

            }
        }

        static void Main(string[] args)
        {
            Task t = new Task(Thread1);
            Task t2 = new Task(Thread2);
            t.Start();
            t2.Start();

            Task.WaitAll(t, t2);

            Console.WriteLine(num);
        }
    }
}