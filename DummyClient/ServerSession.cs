using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

namespace DummyClient
{
	

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint end)
        {
            Console.WriteLine($"OnConnected : {end}");

            PlayerInforReq packet = new PlayerInforReq() { playerId = 1001, name = "저스틴" };
			// skill list에 SkillInfo구조체로 add를 넣어주어 스킬리스트 목록을 늘려준다.
			var skill = new PlayerInforReq.Skill() { id = 101, level = 1, duration = 5f };
			skill.attributes.Add(new PlayerInforReq.Skill.Attribute(){ att = 1000 });
            packet.skills.Add(skill);
            packet.skills.Add(new PlayerInforReq.Skill() { id = 222, level = 20, duration = 4f });
            packet.skills.Add(new PlayerInforReq.Skill() { id = 333, level = 30, duration = 5f });
            packet.skills.Add(new PlayerInforReq.Skill() { id = 444, level = 40, duration = 6f });
            {
                // write() 함수에서 버퍼 크기 할당, 작업까지 다 해줌 
                // 다른 곳에서도 패킷을 보낼 때는 packet 클래스를 인스턴스하여 사용해도 됨
                ArraySegment<byte> sendSeg = packet.Write();
                if (sendSeg != null)
                    Send(sendSeg);
            }
            Thread.Sleep(1000);

            Disconnect();
        }

        public override void OnDisconnected(EndPoint end)
        {
            Console.WriteLine($"OnDisconnected : {end}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvDa = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvDa}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
}
