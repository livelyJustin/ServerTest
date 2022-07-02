using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient
{

    class PlayerInforReq
    {
        public long playerId;
        public string name;
        public struct Skill
        {
            public int id;
            public short level;
            public float duration;

            public void Read(ReadOnlySpan<byte> readSpan, ref ushort count)
            {
                this.id = BitConverter.ToInt32(readSpan.Slice(count, readSpan.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(readSpan.Slice(count, readSpan.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(readSpan.Slice(count, readSpan.Length - count));
                count += sizeof(float);
            }

            public bool Write(Span<byte> span, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.duration);
                count += sizeof(float);
                return success;
            }
        }
        public List<Skill> skills = new List<Skill>();

        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> readSpan = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(long);
            ushort nameLen = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(readSpan.Slice(count, nameLen));
            count += nameLen;
            this.skills.Clear();
            ushort skillLeng = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < skillLeng; i++)
            {
                Skill skill = new Skill();
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
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, openSeg.Array, openSeg.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;
            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.skills.Count);
            count += sizeof(ushort);
            foreach (Skill skill in skills)
                success &= skill.Write(span, ref count);

            success &= BitConverter.TryWriteBytes(span, count);
            if (success == false)
                return null;
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

            PlayerInforReq packet = new PlayerInforReq() { playerId = 1001, name = "저스틴" };
            // skill list에 SkillInfo구조체로 add를 넣어주어 스킬리스트 목록을 늘려준다.
            packet.skills.Add(new PlayerInforReq.Skill() { id = 111, level = 10, duration = 3f });
            packet.skills.Add(new PlayerInforReq.Skill() { id = 222, level = 20, duration = 4f });
            packet.skills.Add(new PlayerInforReq.Skill() { id = 333, level = 30, duration = 5f });
            packet.skills.Add(new PlayerInforReq.Skill() { id = 444, level = 40, duration = 6f });
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
