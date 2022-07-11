using ServerCore;


class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInforReq p = packet as C_PlayerInforReq;
        Console.WriteLine($"PlayerInforReq: {p.playerId} playernanme: {p.name}");

        foreach (C_PlayerInforReq.Skill skill in p.skills)
        {
            Console.WriteLine($"skill_Id {skill.id}, skill_level {skill.level}, skill_duration {skill.duration} ");
        }
    }

}
