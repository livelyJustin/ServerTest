using System;
using System.Threading;
using System.Threading.Tasks;


namespace ServerCore
{
    class Program
    {
        int _answer;
        bool _complete;

        void T1()
        {
            _answer = 123; 
            Thread.MemoryBarrier(); // _answer = 123이 들어간걸 메모리에 알려줘(Store)
            _complete = true;
            Thread.MemoryBarrier(); // _complete = true가 들어간걸 메모리에 알려줘(Store)
        }
        void RNG()
        {
            Thread.MemoryBarrier(); // _complet의 최신 값을 알려줘(Load)

            if (_complete)
            {
                Thread.MemoryBarrier(); // _answer의 최신 값을 알려줘(Load)
                Console.WriteLine(_answer);
            }
        }

        static void Main(string[] args)
        {
           
        }
    }
}