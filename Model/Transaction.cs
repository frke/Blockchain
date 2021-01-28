namespace Blockchain.Model
{
    public class Transaction
    {

        public Transaction() { }
        public Transaction(string sender, string receiver, string type, string value, string description)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Type = type;
            this.Value = value;
            this.Description = description;
        }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
