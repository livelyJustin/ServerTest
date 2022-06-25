using ServerCore;
using System.Net;
using System.Text;

namespace Server
{
    // 패킷 헤더
    public abstract class Packet
    {
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
        public string name;

        public PlayerInforReq()
        {
            this.packetId = (ushort)PacketID.PlayerInforReq;
        }

        // wrtie는 직접 컨트롤하고 있기에 문제가 없지만 
        // read는 문제가 생길 수 있음 -> 서버는 항상 클라이언트 쪽에서 잘못된 정보를 보낼 수 있다고 가정하고 해야함
        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;
            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);

            ReadOnlySpan<byte> readSpan = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            // ID는 추출할 필요가 없음 이미 Switch 문에서 걸렸다면 해당 아이디 이기 때매
            //ushort packetId = BitConverter.ToUInt16(s.Array, s.Offset + count);//하드 코딩말고 나중에 자동화할 예정
            count += sizeof(ushort);

            // 클라에서 보낸 정보를 무조건 Read 하는 것이 아니라, 크기가 정확한지 다시 한 번 체크한다.
            // 코드의 안정성을 위해 패킷의 사이즈를 한 번 더 체크해야함 ReadOnlySpan을 받는 값으로 변경해주어 체크 

            // s.count는 Write에서 count로 보낸 값. 즉, 12이다. 그렇기에 위에서 size, packetId를 추출하며 사용한 바이트 값을 제외하고
            // 남은 값의 크기가 일치하는 지 확인하는 것이다. 범위를 초과할 경우 에러가 날 것이다. 
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

            Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);



            // 하드 코딩 식이 아닌 더 깔끔한 표현을 위해 ushort의 사이즈를 올려준다.
            count += sizeof(ushort);
            // this로 변경
            //success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), this.packetId);
            // slice() => 시작 offest과 길이를 넘겨주어 해당 부분을 잘라서 넘겨준다. -> 가독성 증가
            // slice는 결과가 변경 되는게 아닌 그 일부를 복사해 넘겨준다고 보면된다.
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.packetId);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
            count += sizeof(long);

            // string을 보냐기 위한 두 단계

            //this.name.Length; => 길이는 4이나 8바이트가 나옴 
            // utf-16으로 넘겨주는법 이렇게해야 4바이트가 넘어감
            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            count += sizeof(ushort);
            //Encoding.Unicode.GetBytes(this.name);   // string을 바이트 배열로 넘겨줄 수 있다.
            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, openSeg.Array, count, nameLen);
            count += nameLen;

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

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint end)
        {
            //Console.WriteLine($"OnConnected : {end}");

            //Packet packet = new Packet() { size = 100, packetId = 10 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            //byte[] buffer1 = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);

            //Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);

            //ArraySegment<byte> sendBuffer = SendBufferHelper.Close(buffer1.Length + buffer2.Length);
            //Send(sendBuffer);
            Thread.Sleep(5000);


            Disconnect();
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);//하드 코딩말고 나중에 자동화할 예정
            count += 2;

            // packetId를 PacketID Class 타입으로 캐스트 한 걸 검사
            // 자동화 고려
            switch ((PacketID)packetId)
            {
                case PacketID.PlayerInforReq:
                    {
                        PlayerInforReq playerinfoReq = new PlayerInforReq();
                        playerinfoReq.Read(buffer);
                        Console.WriteLine($"PlayerInforReq: {playerinfoReq.playerId} playernanme: {playerinfoReq.name}");
                    }
                    break;
            }
            Console.WriteLine($"RecvPacktID: {packetId} size:{size}");
        }

        public override void OnDisconnected(EndPoint end)
        {
            Console.WriteLine($"OnDisconnected : {end}");
        }


        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");

        }
    }
}
