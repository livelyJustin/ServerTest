using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using System.Net.Sockets;

namespace DummyClient
{
    // 패킷 헤더
    public abstract class Packet
    {
        // 크게 사용성이 없기에 제거해도 괜찮다. size, packetId 
        public ushort size;
        public ushort packetId;

        // send 생성을 하기 위한 클래스 생성
        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    // 실제 처럼 하기 위한 class
    class PlayerInforReq : Packet
    {
        public long playerId; // 8바이트

        public PlayerInforReq()
        {
            this.packetId = (ushort)PacketID.PlayerInforReq;
        }

        // wrtie는 직접 컨트롤하고 있기에 문제가 없지만 
        // read는 문제가 생길 수 있음 -> 서버는 항상 클라이언트 쪽에서 잘못된 정보를 보낼 수 있다고 가정하고 해야함
        public override void Read(ArraySegment<byte> s)
        {
            ushort count = 0;
            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            count += 2;
            // ID는 추출할 필요가 없음 이미 Switch 문에서 걸렸다면 해당 아이디 이기 때매
            //ushort packetId = BitConverter.ToUInt16(s.Array, s.Offset + count);//하드 코딩말고 나중에 자동화할 예정
            count += 2;
            
            // 클라에서 보낸 정보를 무조건 Read 하는 것이 아니라, 크기가 정확한지 다시 한 번 체크한다.
            // 코드의 안정성을 위해 패킷의 사이즈를 한 번 더 체크해야함 ReadOnlySpan을 받는 값으로 변경해주어 체크 

            // s.count는 Write에서 count로 보낸 값. 즉, 12이다. 그렇기에 위에서 size, packetId를 추출하며 사용한 바이트 값을 제외하고
            // 남은 값의 크기가 일치하는 지 확인하는 것이다. 범위를 초과할 경우 에러가 날 것이다. 
            this.playerId =  BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count));

            count += 8;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);

            bool success = true;
            ushort count = 0;

            count += 2;
            // this로 변경
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), this.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count), (ushort)4);

            // 성공 여부가 null 인지 아닌지로 알 수 있음
            if (success == false)
                return null;

            // 리턴 값 설정
            return SendBufferHelper.Close(count);
        }
    }


    public enum PacketID
    {
        PlayerInforReq = 1,
        PlayerInforOk = 2,
    }

    class ServerSession : Session
    {
        // writebytes가 유니티에서 안될경우 이렇게도 됨
        // unsafe는 포인터 조작가능 -> c++ 과 같은 방법으로 만들어서 사용도 됨. 속도 증가
        // array[offset] = value 개념
        //static unsafe void ToBytes(byte[] array, int offest, ulong value)
        //{
        //    fixed (byte* ptr = &array[offest])
        //        *(ulong*)ptr = value;
        //}

        public override void OnConnected(EndPoint end)
        {
            Console.WriteLine($"OnConnected : {end}");
            // 2바이트 + 2바이트 이니 4

            // 패킷id는 다른 곳에서 넣어줌
            PlayerInforReq packet = new PlayerInforReq() {  playerId = 1001 };

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
