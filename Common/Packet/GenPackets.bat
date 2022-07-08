START ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packets"
XCOPY /Y GenPackets.cs "../../Server/Packets"
XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packets"
XCOPY /Y ServerPacketManager.cs "../../Server/Packets"
