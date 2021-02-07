using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Model
{
    class Peer
    {
        public long PeerCount{ get; set; } 
        public DateTime TimestampUtcLastSeen { get; set; } 
        public long HighestBlockNum { get; set; }
        public string PeerPublicKey { get; set; }
        public string PeerHostName { get; set; }
        public string PeerPort { get; set; } = "5449";
        public string PeerHostip { get; set; }
        public string PeerFrendlyName { get; set; }

        // internal set = only possible to set in this dll asembly. 
        // Če bo drug program uporabljal to dll knjižnico ga bo lahko samo prebral
        public string NodeClass { get; internal set; } = "Standard";
    }
}
