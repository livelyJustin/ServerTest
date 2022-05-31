namespace ServerCore
{
    class PlaySection
    {
        static object key1 = new object();

        public static void Test()
        {
            lock (key1)
            {
                UserSection.UserAction();
            }
        }

        public static void PlayAction()
        {
            lock (key1)
            {
            }
        }

    }

    class UserSection
    {
        static object key2 = new object();

        public static void Test()
        {
            lock (key2)
            {
                PlaySection.PlayAction();
            }
        }
        public static void UserAction()
        {
            lock (key2)
            {
            }
        }
    }

    class Program
    {
        static void Thread1()
        {
            for (int i = 0; i < 10000; i++)
            {
                PlaySection.Test();
            }
        }
        static void Thread2()
        {
            for (int i = 0; i < 10000; i++)
            {
                UserSection.Test();
            }
        }

        static void Main(string[] args)
        {
            Task t = new Task(Thread1);
            Task t2 = new Task(Thread2);
            t.Start();
            Thread.Sleep(100);
            t2.Start();

            Task.WaitAll(t, t2);
        }
    }
}