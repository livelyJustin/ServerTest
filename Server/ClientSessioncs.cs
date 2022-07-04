using ServerCore;
using System.Net;
using System.Text;

namespace Server
{
	

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
            count += (ushort)sizeof(ushort);

            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);//하드 코딩말고 나중에 자동화할 예정
            count += (ushort)sizeof(ushort);
            // packetId를 PacketID Class 타입으로 캐스트 한 걸 검사
            // 자동화 고려
            switch ((PacketID)packetId)
            {
                case PacketID.PlayerInforReq:
                    {
                        PlayerInforReq playerinfoReq = new PlayerInforReq();
                        playerinfoReq.Read(buffer);
                        Console.WriteLine($"PlayerInforReq: {playerinfoReq.playerId} playernanme: {playerinfoReq.name}");

                        foreach (PlayerInforReq.Skill skill in playerinfoReq.skills)
                        {
                            Console.WriteLine($"skill_Id {skill.id}, skill_level {skill.level}, skill_duration {skill.duration} ");
                        }

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
