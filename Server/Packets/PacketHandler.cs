using System;
using ServerCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class PacketHandler
    {
        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            PlayerInforReq p = packet as PlayerInforReq;
            Console.WriteLine($"PlayerInforReq: {p.playerId} playernanme: {p.name}");

            foreach (PlayerInforReq.Skill skill in p.skills)
            {
                Console.WriteLine($"skill_Id {skill.id}, skill_level {skill.level}, skill_duration {skill.duration} ");
            }
        }

    }
}
