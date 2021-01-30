using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Model
{
    class Ping
    {
        public string Vprasanje { get; set; } = "Ping";
        public string Odgovor { get; set; } = "Pong";
        public DateTime TimeStamp { get; set; } = DateTime.Now; // lokalni čas
        public DateTime TimeStampUtc { get; set; } = DateTime.UtcNow; // utc čas
        public long UnixTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds(); // unix čas od 1.1.1970 0:0:0
    }
}

