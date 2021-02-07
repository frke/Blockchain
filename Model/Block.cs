using System;
using System.Collections.Generic;

namespace Blockchain.Model
{
    public class Block
    {
        public long BlockNum { get; set; }
        public DateTime Timestamp { get; set; }
        public string Hash { get; set; }
        public string PrevHash { get; set; }
        public long Nounce { get; set; }
        public string SignedHash { get; set; } = "Ta blok še ni podpisan";
        public List<Transaction> TransactionList { get; set; }
    }
}
