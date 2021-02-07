using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Model
{
    class Ping
    {
        public string Vprasanje { get; } = "Ping";
        public string Odgovor { get; } = "Pong";
        public DateTime Timestamp { get; set; } = DateTime.Now; // lokalni čas
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow; // utc čas
        public long UnixTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds(); // unix čas od 1.1.1970 0:0:0
        public int VersionMajor { get; } = 0; // vsak node ima svojo verzijo Major - glavna verzija, Major med seboj niso kompatibilne
        public int VersionMinor { get; } = 0; // vsak node ima svojo verzijo - podverzija, Minor verzije so med seboj kompatibilne brez omejitev
        public string Version { get; } = "0.0.RC1";// Major+.+Minor+RC{number} RC m= release candidate, RC je lahko poljubno, zaradi preglednosti ne več kot 5
        public string SourceCodeSignature { get; set; } = "Hash od git commit sporočila te verzije";
        public string BuildSignature { get; set; } = "00000"; // to je 64 byte hash sha256 od glavnega exe fajla, enak za iste Major.Minor verzije 

    }
}

