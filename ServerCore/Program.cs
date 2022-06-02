namespace ServerCore
{

    public class SpinLock
    {
        volatile public int _lock = 0;
        public void Acquire()
        {
            while(true)
            {
                int origin =  Interlocked.CompareExchange(ref _lock, 1, 0);
                if (origin == 0)
                    break;
            }
        }
        public void Release()
        {
            _lock = 0;
        }
    }

    public  class Program
    {
        static SpinLock spin = new SpinLock();
        public static int num = 0;

        static void Thread_1()
        {
            for (int i = 0; i < 100; i++)
            {
                spin.Acquire();
                num++;
                spin.Release();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 100; i++)
            {
                spin.Acquire();
                num--;
                spin.Release();
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