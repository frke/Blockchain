namespace Blockchain.Model
{
    public class Transaction
    {

        public Transaction() { }
        public Transaction(string sender, string receiver, string amount, string description)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Amount = amount;
            this.Description = description;
        }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
    }
}
