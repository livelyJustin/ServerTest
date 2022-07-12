using Server;
using ServerCore;
using System;


class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if(clientSession.Room == null)
            return;

        clientSession.Room.BroadCast(clientSession, chatPacket.chat);

        Console.WriteLine($"PlayerInforReq: {p.playerId} playernanme: {p.name}");

        foreach (C_PlayerInforReq.Skill skill in p.skills)
        {
            Console.WriteLine($"skill_Id {skill.id}, skill_level {skill.level}, skill_duration {skill.duration} ");
        }
    }

}
