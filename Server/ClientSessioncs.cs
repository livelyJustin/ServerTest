using ServerCore;
using System.Net;

namespace Server
{
    // 패킷 헤더
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    // 실제 처럼 하기 위한 class
    class PlayerInforReq : Packet
    {
        public long playerId;
    }

    class PlayerInforOk : Packet
    {
        public int hp;
        public int attack;
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
                        // long은 8바이트 크기의 변수이기에 64비트로
                        long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count) ;
                        count += 8;
                        Console.WriteLine($"PlayerInforReq: {playerId}");
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
