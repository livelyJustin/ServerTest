
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
