﻿using Blockchain.Model;
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

                // Receiver == podpis
                transaction.Receiver = Crypto.Poskus(data);

                //Crypto.VerifyData(transaction.Sender+transaction.Type+ transaction.Value+ transaction.Description, transaction.Receiver)
                rawTransactionList.Add(transaction);
            }
        }
        //public void AddRaw(string sender, string receiver, string type, string value, string description)
        //{
        //    var transaction = new Transaction(sender, receiver, type, value, description);
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
