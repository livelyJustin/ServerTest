using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPackets;
        static ushort packetId; // emum을 처리하기 위해서는 몇개의 packet을 처리했는지 체크하기 위한 변수
        static string packetEnums;
        static string serverRegister;
        static string clientRegister;


        static void Main(string[] args)
        {
            string pdlPath = "../../PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                // 주석 무시
                IgnoreComments = true,
                // 스페이스바(공백) 무시
                IgnoreWhitespace = true,
            };

            if (args.Length >= 1)
                pdlPath = args[0];

            // XML 을 생성하고 파싱 한 뒤에는 다시 닫아줘야 한다.
            // 이때 using을 사용한다면 해당 영역에서 벗어날 때 Dispose를 진행
            using (XmlReader x = XmlReader.Create(pdlPath, settings))
            {
                // 버전 정보같은 헤더 영역은 건너 뛴다.
                x.MoveToContent();

                // 스트림 방식으로 읽어드림
                while (x.Read())
                {
                    if (x.Depth == 1 && x.NodeType == XmlNodeType.Element)
                        ParsePacket(x);
                    //Console.WriteLine(x.Name + " " + x["name"]);
                }

                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText);
                string clientManagerText = string.Format(PacketFormat.managerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);
                string serverManagerText = string.Format(PacketFormat.managerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);
            }
        }

        public static void ParsePacket(XmlReader x)
        {
            if (x.NodeType == XmlNodeType.EndElement)
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

            //ParseMembers(x);


            // 멤버 호출이 끝날 때 파일에 담기
            Tuple<string, string, string> t = ParseMembers(x);
            genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";
            if (packetName.StartsWith("S_") || packetName.StartsWith("s_"))
                clientRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            else if (packetName.StartsWith("C_") || packetName.StartsWith("c_"))
                serverRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;

        }
        // Packet의 내부 애들 동작시키기
        public static Tuple<string, string, string> ParseMembers(XmlReader x)
        {
            string packetName = x["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            // 내부 멤버들은 뎁스가 하나 추가되니 +1
            int depth = x.Depth + 1;
            while (x.Read())
            {
                // 순서대로 불러오기에 /packet이 오면 depth가 1이라서 
                // 알아서 나가질 것
                if (x.Depth != depth)
                    break;

                string memberName = x["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                // 코드 가독성 증가를 위한 enter
                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                // 앞에 정의해준 long string list 같은 것들
                string memberType = x.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseListPacket(x);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseListPacket(XmlReader x)
        {
            string listName = x["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(x);

            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                t.Item1, t.Item2, t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean"; // ToInt16과 같은 것
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1); // substring은 인자 부터 값 시작
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}