namespace Blockchain.Model
{
    public class Transaction
    {

        public Transaction() { }
        public Transaction(string tx, string sender, string receiveraddress, string type, string value, string description)
        {
            this.Tx = tx;
            this.Sender = sender;
            this.ReceiverAddress = receiveraddress;
            this.Type = type;
            this.Value = value;
            this.Description = description;
        }
        public string Tx { get; set; }
        public string Sender { get; set; }
        public string ReceiverAddress { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        // Verified ni del transakcije, ampak samo informacija, da je transakcija true=1 ali false=0, privezto je false
        // Verfied je izračunana iz vsebine
        // Verified tudi ni del blockchaina
        // napolni jo blockminer
        public bool Verified { get; set; } = false;
    }
}
