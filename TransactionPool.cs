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
                rawTransactionList.Add(transaction);
            }
        }
        public void AddRaw(string from, string to, string amount, string description)
        {
            var transaction = new Transaction(from, to, amount, description);
            lock (lockObj)
            {
                rawTransactionList.Add(transaction);
            }
        }

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
