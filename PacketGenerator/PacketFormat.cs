using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    // 공통적으로 사용되는 요소들과, 유동적으로 변하는 요소들을 구분하여 처리할 에정
    // 예를들어 class의 기존 PlayerInfoReq 였지만, 다른 곳에서는 class 이름이 다른 곳에서는 다른 이름일 수 있기에 {0} 이라는 걸로 대체하여
    // 나중에이 {0}에 이름을 넘겨줄 수 있도록 한다.
    class PacketFormat
    {
        //{0} 패킷이름: 항상 PlaerInforReq은 아니기에 유동적임
        //{1} 멤버변수: playerId, name은 다다르기에
        //{2} 멤버변수 read
        //{3} 멤버변수 write
        public static string packetFormat =
@"
class {0}
{{
    {1}
    public long playerId; // 8바이트
    public string name; // string 뿐만 아니라 아이콘과 같은 byte 배열도 넘기는 방법을 이해한 것이다.(두 단계로 보낸방법)

    public List<SkillInfo> skills = new List<SkillInfo>();


    public struct SkillInfo
    {{
        public int id;
        public short level;
        public float duration;

        public bool Write(Span<byte> span, ref ushort count)
        {{
            bool success = true;

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), id);
            count += sizeof(int);

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), level);
            count += sizeof(short);

            success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), duration);
            count += sizeof(float);
            return success;
        }}

        public void Read(ReadOnlySpan<byte> readSpan, ref ushort count)
        {{
            id = BitConverter.ToInt32(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(int);

            level = BitConverter.ToInt16(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(short);

            duration = BitConverter.ToSingle(readSpan.Slice(count, readSpan.Length - count));
            count += sizeof(float);
        }}
    }}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> readSpan = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;
        Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.{0});
        {3}
        success &= BitConverter.TryWriteBytes(span, count); // 원본을 넣으면 된다.
        if (success == false)
            return null;
        // 리턴 값 설정
        return SendBufferHelper.Close(count);
    }}
}}
";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat = 
@"public {0} {1}";

        // {0} 변수 이름(기존은id)
        // {1} To ~ 변수 형식 (기존 ToInt32)
        // {2} sizeof ~ 변수 형식 (기존 int or something)
        public static string readFormat =
@" this.{0} = BitConverter.{1}(readSpan.Slice(count, readSpan.Length - count));
count += sizeof({2});";

        // {0} 변수 이름 여기서 toint, ushort, getstring은 고정
        public static string readStringFormat =
@" ushort {0}Len = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
count += sizeof(ushort);

this.{0} = Encoding.Unicode.GetString(readSpan.Slice(count, {0}Len)); 
count += {0}Len;";

        // {0} 변수 이름 (기존 playerId)
        // {1} 변수 형식 (기존 long)
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

        // {0} 변수 이름 
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, openSeg.Array, openSeg.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";
    }
}
