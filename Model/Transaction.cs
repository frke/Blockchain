namespace Blockchain.Model
{
    public class Transaction
    {

        public Transaction() { }
        public Transaction(string from, string to, string amount, string description)
        {
            this.From = from;
            this.To = to;
            this.Amount = amount;
            this.Description = description;
        }
        public string From { get; set; }
        public string To { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
    }
}
