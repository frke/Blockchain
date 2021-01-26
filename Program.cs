using System;

namespace Blockchain
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DependencyManager.Fill(new TransactionPool(), new BlockMiner(), new EmbedServer("5449"));
            DependencyManager.BlockMiner.Start();
            DependencyManager.EmbedServer.Start();
            Console.WriteLine("Pritisni katerokoli tipko za izhod");
            Console.ReadKey();
            DependencyManager.BlockMiner.Stop();
            DependencyManager.EmbedServer.Stop();    
        }
    }
    /// <summary>
    /// ima properties: TransactionPool, BlockMine, EmbedServer
    /// </summary>
    public static class DependencyManager
    {
        public static TransactionPool TransactionPool { get; private set; }
        public static BlockMiner BlockMiner { get; private set; }
        public static EmbedServer EmbedServer { get; private set; }

        public static void Fill(TransactionPool transactionPool, BlockMiner blockMiner, EmbedServer embedServer)
        {
            TransactionPool = transactionPool;
            BlockMiner = blockMiner;
            EmbedServer = embedServer;
        }
    }
}