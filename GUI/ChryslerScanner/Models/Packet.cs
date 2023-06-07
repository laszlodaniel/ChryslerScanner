namespace ChryslerScanner.Models
{
    public class Packet
    {
        public byte Sync { get; set; }
        public int Length { get; set; }
        public bool Direction { get; set; }
        public byte Bus { get; set; }
        public byte Command { get; set; }
        public byte Mode { get; set; }
        public byte[] Payload { get; set; }
        public byte Checksum { get; set; }
    }
}