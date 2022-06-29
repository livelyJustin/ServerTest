using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient
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
        // read는  생길 수 있음 -> 서버는 항상 클라이언트 쪽에서 잘못된 정보를 보낼 수 있다고 가정하고 해야함
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
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.PlayerInforReq);

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

    class ServerSession : Session
    {

        public override void OnConnected(EndPoint end)
        {
            Console.WriteLine($"OnConnected : {end}");
            // 2바이트 + 2바이트 이니 4

            PlayerInforReq packet = new PlayerInforReq() { playerId = 1001, name = "저스틴" };
            packet.skills.Add(new PlayerInforReq.SkillInfo() { id = 111, level = 10, duration = 3f });
            packet.skills.Add(new PlayerInforReq.SkillInfo() { id = 222, level = 20, duration = 4f });
            packet.skills.Add(new PlayerInforReq.SkillInfo() { id = 333, level = 30, duration = 5f });
            packet.skills.Add(new PlayerInforReq.SkillInfo() { id = 444, level = 40, duration = 6f });

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
