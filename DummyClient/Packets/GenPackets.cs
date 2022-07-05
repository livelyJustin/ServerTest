using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
    PlayerInforReq = 1,
	test = 2,
	
}


class PlayerInforReq
{
    public byte testByte;
	public long playerId;
	public string name;
	public class Skill
	{
	    public int id;
		public short level;
		public float duration;
		public class Attribute
		{
		    public int att;
		
		    public void Read(ReadOnlySpan<byte> readSpan, ref ushort count)
		    {
		        this.att = BitConverter.ToInt32(readSpan.Slice(count, readSpan.Length - count));
				count += sizeof(int);
		    }
		
		    public bool Write(Span<byte> span, ref ushort count)
		    {
		        bool success = true;
		        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.att);
				count += sizeof(int);
		        return success;
		    }
		}
		public List<Attribute> attributes = new List<Attribute>();
	
	    public void Read(ReadOnlySpan<byte> readSpan, ref ushort count)
	    {
	        this.id = BitConverter.ToInt32(readSpan.Slice(count, readSpan.Length - count));
			count += sizeof(int);
			this.level = BitConverter.ToInt16(readSpan.Slice(count, readSpan.Length - count));
			count += sizeof(short);
			this.duration = BitConverter.ToSingle(readSpan.Slice(count, readSpan.Length - count));
			count += sizeof(float);
			this.attributes.Clear();
			ushort attributeLeng = BitConverter.ToUInt16(readSpan.Slice(count, readSpan.Length - count));
			count += sizeof(ushort);
			for (int i = 0; i < attributeLeng; i++)
			{
			    Attribute attribute = new Attribute();
			    attribute.Read(readSpan, ref count);
			    attributes.Add(attribute);
			}
			
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
			success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.attributes.Count);
			count += sizeof(ushort);
			foreach (Attribute attribute in attributes)
			    success &= attribute.Write(span, ref count);
			
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
        this.testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
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
        segment.Array[segment.Offset + count] = (byte)this.testByte;
		count += sizeof(byte);
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

class test
{
    public int testint;
   
    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> readSpan = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.testint = BitConverter.ToInt32(readSpan.Slice(count, readSpan.Length - count));
		count += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 0;

        Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketID.test);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.testint);
		count += sizeof(int);
        success &= BitConverter.TryWriteBytes(span, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

