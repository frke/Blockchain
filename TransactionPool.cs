using Blockchain.Model;
using System.Collections.Generic;
using System.Linq;

namespace Blockchain
{
    public class TransactionPool
    {
        private List<Transaction> rawTransactionList;

        private object lockObj;

        public TransactionPool()
        {
            lockObj = new object();
            rawTransactionList = new List<Transaction>();
        }

        public void AddRaw(Transaction transaction)
        {
            lock (lockObj)
            {
                string[] data = new string[] { transaction.Sender, transaction.Type, transaction.Value, transaction.Description };

                // ReceiverAddress == podpis
                transaction.ReceiverAddress = Crypto.Poskus(data);

                //Crypto.VerifyData(transaction.Sender+transaction.Type+ transaction.Value+ transaction.Description, transaction.ReceiverAddress)
                rawTransactionList.Add(transaction);
            }
        }
        //public void AddRaw(string sender, string receiveraddress, string type, string value, string description)
        //{
        //    var transaction = new Transaction(sender, receiveraddress, type, value, description);
        //    lock (lockObj)
        //    {
        //        rawTransactionList.Add(transaction);
        //    }
        //}

        public List<Transaction> TakeAll()
        {
            lock (lockObj)
            {
                var all = rawTransactionList.ToList();
                rawTransactionList.Clear();
                return all;
            }
        }
    }
}
