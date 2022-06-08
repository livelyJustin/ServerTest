namespace ServerCore
{
    public class Program
    {
        static volatile  int count = 0;
        static Lock _lock = new Lock();
        static void Main(string[] args)
        {
            Task t = new Task(delegate
            {
                for (int i = 0; i < 10; i++)
                {
                    _lock.WriteLock();
                    count++;
                    _lock.WriteUnlock();
                }
            });
            Task t2 = new Task(delegate
            {
                for (int i = 0; i < 10; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });

            t.Start();
            t2.Start();

            Task.WaitAll(t, t2);
            Console.WriteLine("카운트는? " + count);
        }
    }
}