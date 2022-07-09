using System;
using System.Collections.Generic;
using ServerCore;

class PacketManager
{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion Singleton

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv =
        new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler 
        = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
      _onRecv.Add((ushort)PacketID.C_PlayerInforReq, MakePacket<C_PlayerInforReq>);
       _handler.Add((ushort)PacketID.C_PlayerInforReq, PacketHandler.C_PlayerInforReqHandler);

    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += (ushort)sizeof(ushort);

        ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += (ushort)sizeof(ushort);

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(packetId, out action))
            action.Invoke(session, buffer);
    }
    
    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);

        Action<PacketSession, IPacket> action = null ;
        // 
        if(_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}