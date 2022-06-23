using ServerCore;
using System.Net;

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

        public PlayerInforReq()
        {
            this.packetId = (ushort)PacketID.PlayerInforReq;
        }

        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;
            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            count += 2;
            // ID는 추출할 필요가 없음 이미 Switch 문에서 걸렸다면 해당 아이디 이기 때매
            //ushort packetId = BitConverter.ToUInt16(s.Array, s.Offset + count);//하드 코딩말고 나중에 자동화할 예정
            count += 2;
            this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count));

            count += 8;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;

            count += 2;
            // this로 변경
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), this.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count), count);

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
                        Console.WriteLine($"PlayerInforReq: {playerinfoReq.playerId}");
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
