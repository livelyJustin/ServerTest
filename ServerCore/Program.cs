namespace ServerCore
{
    public class Program
    {
        static ThreadLocal<String> _threadLocal = new ThreadLocal<String>(
            () => { return $"My Name is:  {Thread.CurrentThread.ManagedThreadId}"; }  );

        static void WhoAmI()
        {
            bool repeat = _threadLocal.IsValueCreated;

            if (repeat)
                Console.WriteLine("중복임다" + _threadLocal.Value);
            else
                Console.WriteLine(_threadLocal.Value);
        }



        static void Main(string[] args)
        {
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            _threadLocal.Dispose();
        }
    }
}


//bool repeat = _threadLocal.IsValueCreated;

//if (repeat)
//    Console.WriteLine("중복임다" + _threadLocal.Value);
//else
//    Console.WriteLine(_threadLocal.Value);