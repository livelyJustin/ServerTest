using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


class C_Chat : IPacket
{
    public string chat;

    public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }
   
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> readSpan = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

         ushort chatLen = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(readSpan.Slice(count, chatLen)); 
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.C_Chat);
        count += sizeof(ushort);
        ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, openSeg.Array, openSeg.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
        success &= BitConverter.TryWriteBytes(span, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

class S_Chat : IPacket
{
    public int playerId;
	public string chat;

    public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }
   
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> readSpan = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(readSpan.Slice(count, readSpan.Length - count));
		count += sizeof(int);
		 ushort chatLen = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(readSpan.Slice(count, chatLen)); 
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.S_Chat);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, openSeg.Array, openSeg.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;
        success &= BitConverter.TryWriteBytes(span, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

