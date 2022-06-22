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
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    // 실제 처럼 하기 위한 class
    class PlayerInforReq : Packet
    {
        public long playerId; // 8바이트
    }

    class PlayerInforOk : Packet
    {
        public int hp;
        public int attack;
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

            PlayerInforReq packet = new PlayerInforReq() {  packetId = (ushort)PacketID.PlayerInforReq, playerId = 1001 };

            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> openSeg = SendBufferHelper.Open(4096);

                bool success = true;
                // count를 int 타입으로 넣으면 TryWriteBytes에서 4바이트가 들어감
                ushort count = 0;

                 // count를 - 하는 이유: openSeg.Count가 유효범위를 나타내기에 사용한 만큼(count)은 빼주어야함
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), packet.packetId);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset + count, openSeg.Count - count), packet.playerId);
                count += 8;

                // buffer[]에 넣는 부분은 좋지 않은 구조 ushort를 보내준건데 byte[]로 반환한 것이니까 
                // 효율적이지 못함
                //byte[] size = BitConverter.GetBytes(packet.size);
                // 예시 1. 실패하는 경우: openSeg.Count보다 packet.size가 더 큰 경우 // bool에 &연산은 뭐지? 
                // packet.size는 모든 작업이 처리 된 후 마지막에 알 수 있기에 비워 두었던 첫 번째 공간에 담아준다. 
                success &= BitConverter.TryWriteBytes(new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count), count);


                //byte[] packetId = BitConverter.GetBytes(packet.packetId);
                //byte[] playerId = BitConverter.GetBytes(packet.playerId);

                // 자동화 하기 위해서는 기존 처럼 offset + ?, 이런 구조가 아니라 개선이 필요
                //Array.Copy(size, 0, openSeg.Array, openSeg.Offset + count, size.Length);
                //count += 2;
                //Array.Copy(packetId, 0, openSeg.Array, openSeg.Offset + count, packetId.Length);
                //count += 2;
                //Array.Copy(playerId, 0, openSeg.Array, openSeg.Offset + count, playerId.Length);
                //count += 8;

                ArraySegment<byte> sendBuffer = SendBufferHelper.Close(count);
                if(success)
                    Send(sendBuffer);
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
