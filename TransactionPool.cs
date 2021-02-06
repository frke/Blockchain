using Blockchain.Model;
using System.Collections.Generic;
using System.Linq;

namespace Blockchain
{
    public class TransactionPool
    {
        private List<Transaction> rawTransactionList;

        private readonly object lockObj;

        public TransactionPool()
        {
            lockObj = new object();
            rawTransactionList = new List<Transaction>();
        }

        public void AddRaw(Transaction transaction)
        {
            lock (lockObj)
            {
                string[] data = new string[] { transaction.Tx, transaction.Sender, transaction.Type, transaction.Value, transaction.Description };

                // Transaction.Tx je podpis in hkrati identifikcijska številka transakcije
                // data se podpiše s privatnim ključem
                // vrne tudi polje verifid, ki se izračuna takoj po podpisu.
                bool verified;
                transaction.Tx = Crypto.Podpis(data, out verified);
                // če imam data in transaction.Tx, lahko preverim, če je podpis pristen - glej polje transaction.Verified

                transaction.Verified = verified;

                //Crypto.VerifyData(transaction.Sender + transaction.Receiver transaction.Type + transaction.Value + transaction.Description)
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
