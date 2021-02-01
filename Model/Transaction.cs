namespace Blockchain.Model
{
    public class Transaction
    {

        public Transaction() { }
        public Transaction(string sender, string receiveraddress, string type, string value, string description)
        {
            this.Sender = sender;
            this.ReceiverAddress = receiveraddress;
            this.Type = type;
            this.Value = value;
            this.Description = description;
        }
        public string Sender { get; set; }
        public string ReceiverAddress { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
