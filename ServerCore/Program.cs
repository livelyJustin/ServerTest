namespace ServerCore
{
    class SpinLock
    {
        volatile public bool _lock = false; // 1이면 탈출
        public void Acquire()
        {
            while (_lock)
            {
                // 잠김이 풀리기를 기다리는중
            }
            _lock = true;
        }
        public void Release()
        {
            _lock = false;
        }
    }

    class Program
    {
        static int num = 0;
        static SpinLock spinLock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                spinLock.Acquire();
                num++;
                spinLock.Release();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                spinLock.Acquire();
                num--;
                spinLock.Release();
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