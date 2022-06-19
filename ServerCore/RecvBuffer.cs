using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        int _readerPos;
        int _writePos;
        // 유효 사이즈, 얼마나 데이터가 쌓여있는지 체크하기 위함
        public int DataSize { get { return _writePos - _readerPos; } }

        // 버퍼의 남은 사이즈
        public int FreeSize { get { return _buffer.Count - _writePos; } }


        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readerPos, DataSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        // 위치 값 초기화를 위함
        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은 데이터가 없다면 두개가 같은 위치이기에 0으로 처리
                _readerPos = _writePos = 0;
            }
            else
            {
                // 남은게 있다면 가장 앞으로 복사해서 가져다 놓는다.
                Array.Copy(_buffer.Array, _buffer.Offset + _readerPos, _buffer.Array, _buffer.Offset, dataSize);
                _readerPos = 0;
                _writePos = dataSize;
            }
        }

        // 읽기, 쓰기가 실제로 완료되면 호출하여 이동 시키는 함수
        public bool OnRead(int numOfBytes)
        {
            if(numOfBytes > DataSize)
                return false;

            _readerPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
