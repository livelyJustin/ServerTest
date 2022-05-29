namespace ServerCore
{
    class Program
    {
        static int num = 0;

        static void Thread1()
        {

            for (int i = 0; i < 10000; i++)
                Interlocked.Increment(ref num);

        }
        static void Thread2()
        {
            for (int i = 0; i < 10000; i++)
                Interlocked.Decrement(ref num);
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