using ServerCore;
using System.Net;
using System.Text;

namespace Server
{
	class ClientSession : PacketSession
    {
        public int SessionId { get; set; } 
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint end)
        {
            Console.WriteLine($"OnConnected : {end}");
            Program.Room.Enter(this);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint end)
        {
            SessionManager.instance.Remove(this);
            Console.WriteLine($"OnDisconnected : {end}");
            if(Room != null)
            {
                Room.Leave(this);
                Room = null;
            }
        }


        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
}
