namespace ServerCore
{
    // 우선 재귀적을 허용할 것인가 
    // 스핀란 정책 (5000번)

    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // WriteThread[15] 한 번에 한 쓰레드만 획득가능. 그 쓰레드가 누군지 확인 가능
        // ReadCount[16] ReadLock을 여러 쓰레드가 획득할 수 있기에 카운트하기 위함
        int flag;
        int write_Count;

        public void WriteLock()
        {
            int lockThreadId = (flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                write_Count++;
                return;
            }

            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {

                    if (Interlocked.CompareExchange(ref flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        write_Count = 1;
                        return;
                    }
                }
                Thread.Yield();
            }
        }

        // 시도해서 성공하며 리턴
        // 아무도 획득한게 없는 상황에는 flag에 쓰레드 이름 넣기 단, 이러면 멀티 쓰레드의 오류가 생김
        // 상호배제 해줘어야함
        // 상호배제 상황을 만들어주고, flag가 비어있을 경우 쓰레드 아이디를 넣어줌. 
        // interlock에 들어가기전에 flag 값을 EMPTY_FLAG와 비교하여 비어있을 경우에만 return 이 되게한다

        public void WriteUnlock()
        {
            int write_lcok = --write_Count;
            if (write_lcok == 0)
                Interlocked.Exchange(ref flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            int lockThreadId = (flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref flag);
                return;
            }
            // 아무도 WriteLock을 획득하지 않았을 경우
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref flag, expected + 1, expected) == expected)
                        return;
                }
                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref flag);
        }
    }
}
