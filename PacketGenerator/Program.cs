using System.Xml;

namespace PacketGenerator
{
    class Program
    {

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                // 주석 무시
                IgnoreComments = true,
                // 스페이스바(공백) 무시
                IgnoreWhitespace = true,
            };

            // XML 을 생성하고 파싱 한 뒤에는 다시 닫아줘야 한다.
            // 이때 using을 사용한다면 해당 영역에서 벗어날 때 Dispose를 진행
            using (XmlReader x = XmlReader.Create("PDL.xml", settings)) 
            {
                // 버전 정보같은 헤더 영역은 건너 뛴다.
                x.MoveToContent();

                // 스트림 방식으로 읽어드림
                while(x.Read())
                {
                    if (x.Depth == 1 && x.NodeType == XmlNodeType.Element)
                        ParsePacket(x);
                    //Console.WriteLine(x.Name + " " + x["name"]);
                }
            }

        }

        public static void ParsePacket(XmlReader x)
        {
            if (x.NodeType == XmlNodeType.Element)
                return;

            if (x.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invaild Packet name");
                return;
            }

            string packetName = x["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            ParseMembers(x);

        }
        // Packet의 내부 애들 동작시키기
        public static void ParseMembers(XmlReader x)
        {
            string packetName = x["name"];

            // 내부 멤버들은 뎁스가 하나 추가되니 +1
            int depth = x.Depth + 1;
            while(x.Read())
            {
                // 순서대로 불러오기에 /packet이 오면 depth가 1이라서 
                // 알아서 나가질 것
                if (x.Depth != depth)
                    break;

                string memberName = x["name"];  
                if(string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }

                // 앞에 정의해준 long string list 같은 것들
                string memberType = x.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                    case "string":
                    case "list":
                        break;
                    default:
                        break;



                }


            }
        }
    }
}