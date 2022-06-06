namespace ServerCore
{
    public class Program
    {
        static object key = new object();
        static SpinLock spin = new SpinLock();
        static bool key2;


        class Reward
        {
            
        }
        static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        static Reward GetRewardID(int id)
        {
            rwLock.EnterReadLock();
            rwLock.ExitReadLock();

            lock (key)
            {

            }
            return null;
        }
        static void AddReward(Reward reward)
        {
            rwLock.EnterWriteLock();
            rwLock.ExitWriteLock();
            lock (key)
            {

            }
        }

        static void Main(string[] args)
        {
            lock (key)
            {

            }
            try
            {
                spin.Enter(ref key2);
            }
            finally
            {
                if (key2)
                    spin.Exit();
            }
        }
    }
}