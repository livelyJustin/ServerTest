using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper
    {
        // 전역 변수가 사용은 편하지만 멀티 쓰레드 환경으로 고유한 공간을 만들어줌
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            // 한 번도 사용하지 않았을 때
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        // 한 쪽 방향으로 usedSize가 우측으로 점점 이동(RecvBuffer의 WritePos)
        // [] [] [] [] [] [] [] [] [] []
        byte[] _buffer;
        int _usedSize = 0;

        // 총 길이에서 usedsize(사용한 사이즈)를 빼주면 남은 공간(유효 공간)을 알 수 있음
        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunckSize)
        {
            _buffer = new byte[chunckSize];
        }

        // 얼마만큼 사용할 것인지 매개변수로 받아옴
        public ArraySegment<byte> Open(int reserveSize)
        {
            // 원하는 사이즈가 남은 사이즈 보다 클 경우는 해당 버퍼는 고갈이기에 null이다
            if(reserveSize > FreeSize)
                return null;

            // 여유공간이 있다면 사용 요청한 공간을 넘겨준다.
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            // 사용한 만큼으로 다시 돌려준다
            _usedSize = usedSize;
            return segment;
        }

        // sendbuffer는 클린이라는 개념이 없음 => send를 여러명에게 보내기에 다른 세션에서 참조
        // 할 수도 있기에 재사용 구조를 만들기는 어려움(1회용)


    }
}
