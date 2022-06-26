using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient
{
    // 패킷 헤더
    public abstract class Packet
    {
        // 크게 사용성이 없기에 제거해도 괜찮다. size, packetId 
        public ushort size;
        public ushort packetId;

        // send 생성을 하기 위한 클래스 생성
        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    // 실제 처럼 하기 위한 class
    class PlayerInforReq : Packet
    {
        public long playerId; // 8바이트
        public string name; // string 뿐만 아니라 아이콘과 같은 byte 배열도 넘기는 방법을 이해한 것이다.(두 단계로 보낸방법)

        public PlayerInforReq()
        {
            this.packetId = (ushort)PacketID.PlayerInforReq;
        }

        // wrtie는 직접 컨트롤하고 있기에 문제가 없지만 
        // read는 문제가 생길 수 있음 -> 서버는 항상 클라이언트 쪽에서 잘못된 정보를 보낼 수 있다고 가정하고 해야함
        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> readSpan = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt64(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(long);

            // string
            ushort nameLen = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(readSpan.Slice(count, nameLen)); // byte[] -> string

        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;




            // 하드 코딩 식이 아닌 더 깔끔한 표현을 위해 ushort의 사이즈를 올려준다.
            // this로 변경
            
            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), this.packetId);


            // slice() => 시작 offest과 길이를 넘겨주어 해당 부분을 잘라서 넘겨준다. -> 가독성 증가
            // slice는 원본이 변경 되는게 아닌 그 일부를 복사해 넘겨준다.
            
            Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.packetId);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
            
            count += sizeof(long);

            // string을 보냐기 위한 두 단계

            //this.name.Length; => 길이는 4이나 8바이트가 나옴 
            // utf-16으로 넘겨주는법 이렇게해야 4바이트가 넘어감

            //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            //success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);

            //count += sizeof(ushort);
            //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, openSeg.Array, count, nameLen);

            // Array.Copy 시에는 getbytes로 내부적으로는 new를 하고 뱉어주고 있기에 상태이기에 위와 통일감 또는 효율적이게 작용하기 위해 사용함

            // 이렇게하면 openSeg 배열의 offset+count 위치에 this.name 인코딩 값이 들어감
            // 그거에 대한 총 길이가 ushort 타입으로 캐스팅하여 담아둠

            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, openSeg.Array, openSeg.Offset + count + sizeof(ushort));
            
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);

            count += nameLen;
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span, count); // 원본을 넣으면 된다.


            // 성공 여부가 null 인지 아닌지로 알 수 있음
            if (success == false)
                return null;

            // 리턴 값 설정
            return SendBufferHelper.Close(count);
        }
    }


    public enum PacketID
    {
        PlayerInforReq = 1,
        PlayerInforOk = 2,
    }

    class ServerSession : Session
    {
        // writebytes가 유니티에서 안될경우 이렇게도 됨
        // unsafe는 포인터 조작가능 -> c++ 과 같은 방법으로 만들어서 사용도 됨. 속도 증가
        // array[offset] = value 개념
        //static unsafe void ToBytes(byte[] array, int offest, ulong value)
        //{
        //    fixed (byte* ptr = &array[offest])
        //        *(ulong*)ptr = value;
        //}

        public override void OnConnected(EndPoint end)
        {
            Console.WriteLine($"OnConnected : {end}");
            // 2바이트 + 2바이트 이니 4

            // 패킷id는 다른 곳에서 넣어줌
            PlayerInforReq packet = new PlayerInforReq() { playerId = 1001, name = "저스틴" };

            {
                // write() 함수에서 버퍼 크기 할당, 작업까지 다 해줌 
                // 다른 곳에서도 패킷을 보낼 때는 packet 클래스를 인스턴스하여 사용해도 됨
                ArraySegment<byte> sendSeg = packet.Write();
                if (sendSeg != null)
                    Send(sendSeg);
            }

            Thread.Sleep(1000);

            Disconnect();
        }


        public override void OnDisconnected(EndPoint end)
        {
            Console.WriteLine($"OnDisconnected : {end}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvDa = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvDa}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
}
