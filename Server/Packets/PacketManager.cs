using ServerCore;

namespace Server
{
    internal class PacketManager
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
            // 패킷 만들어 준 것을 등록
            _onRecv.Add((ushort)PacketID.PlayerInforReq, MakePacket<PlayerInforReq>);
            _handler.Add((ushort)PacketID.PlayerInforReq, PacketHandler.PlayerInfoReqHandler);
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

        // 제네릭 타입으로 선언해줘야 패킷 클래스를 넘길 수 있기 떄문
        // 제네릭 T는 아무타입이나 다 가능한게 아닌, IPacket을 상속 받고, new() 가능해야한다.

        // 패킷을 만들고, 핸들러까지 불러준다.
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
}
