using ServerCore;
using System.Net;
using System.Text;

namespace Server
{

    class PlayerInforReq  
    {
        public long playerId; // 8바이트
        public string name; // string 뿐만 아니라 아이콘과 같은 byte 배열도 넘기는 방법을 이해한 것이다.(두 단계로 보낸방법)

        public List<SkillInfo> skills = new List<SkillInfo>();


        public struct SkillInfo 
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> span, ref ushort count)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), id);
                count += sizeof(int);

                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), level);
                count += sizeof(short);

                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), duration);
                count += sizeof(float);
                return success;
            }

            public void Read(ReadOnlySpan<byte> readSpan, ref ushort count)
            {
                id = BitConverter.ToInt32(readSpan.Slice(count, readSpan.Length - count));
                count += sizeof(int);

                level = BitConverter.ToInt16(readSpan.Slice(count, readSpan.Length - count));
                count += sizeof(short);

                duration = BitConverter.ToSingle(readSpan.Slice(count, readSpan.Length - count));
                count += sizeof(float);
            }
        }


        // wrtie는 직접 컨트롤하고 있기에 문제가 없지만 
        // read는 문제가 생길 수 있음 -> 서버는 항상 클라이언트 쪽에서 잘못된 정보를 보낼 수 있다고 가정하고 해야함
        public void Read(ArraySegment<byte> segment)
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
            count += nameLen;

            // skill
            skills.Clear();
            ushort skillLeng = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLeng; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(readSpan, ref count);
                skills.Add(skill);
            }

        }

        public ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;

            Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.packetId);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);

            count += sizeof(long);

            // string
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, openSeg.Array, openSeg.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // skill

            // skill의 count를 ushort 타입으로 넣어줘 list가 얼마나 들어갈지 말해주기
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);


            // 하나씩 돌며 데이터 넣기
            foreach (SkillInfo skill in skills)
                success &= skill.Write(span, ref count);

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

                        foreach (PlayerInforReq.SkillInfo skill in playerinfoReq.skills)
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
