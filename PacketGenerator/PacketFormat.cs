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
        // {0} Register 부분
        public static string managerFormat =
@"using System;
using System.Collections.Generic;
using ServerCore;

class PacketManager
{{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager instance
    {{
        get
        {{
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }}
    }}
    #endregion Singleton

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv =
        new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler 
        = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += (ushort)sizeof(ushort);

        ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += (ushort)sizeof(ushort);

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(packetId, out action))
            action.Invoke(session, buffer);
    }}
    
    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T pkt = new T();
        pkt.Read(buffer);

        Action<PacketSession, IPacket> action = null ;
        // 
        if(_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }}
}}
";
        // {0} 패킷아이디
        public static string managerRegisterFormat =
@"      _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
       _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);";
        // {0} 패킷 이름과 번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{{
    {0}
}}

interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}
";
        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        //{0} 패킷이름: 항상 PlaerInforReq은 아니기에 유동적임
        //{1} 멤버변수: playerId, name은 다다르기에
        //{2} 멤버변수 read
        //{3} 멤버변수 write
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}
   
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
        count += sizeof(ushort);
        {3}
        success &= BitConverter.TryWriteBytes(span, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }}
}}
";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat = 
@"public {0} {1};";
        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버변수: playerId, name은 다다르기에
        // {3} 멤버변수 read
        // {4} 멤버변수 write
        public static string memberListFormat =
@"public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> readSpan, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> span, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}
}}
public List<{0}> {1}s = new List<{0}>();";

        // {0} 변수 이름(기존은id)
        // {1} To ~ 변수 형식 (기존 ToInt32)
        // {2} sizeof ~ 변수 형식 (기존 int or something)
        public static string readFormat =
@"this.{0} = BitConverter.{1}(readSpan.Slice(count, readSpan.Length - count));
count += sizeof({2});";

        // {0} 변수 이름
        // {1} 변수 형식 -> sByte도 사용할 수 있기에
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";

        // {0} 변수 이름 여기서 toint, ushort, getstring은 고정
        public static string readStringFormat =
@" ushort {0}Len = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(readSpan.Slice(count, {0}Len)); 
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Leng = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Leng; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(readSpan, ref count);
    {1}s.Add({1});
}}
";
        // {0} 변수 이름 (기존 playerId)
        // {1} 변수 형식 (기존 long)
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

        // {0} 변수 이름
        // {1} 변수 형식 -> sByte도 사용할 수 있기에
        public static string writeByteFormat =
@"openSeg.Array[openSeg.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        // {0} 변수 이름 
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, openSeg.Array, openSeg.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";
        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
    @"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in {1}s)
    success &= {1}.Write(span, ref count);
";
    }

}
