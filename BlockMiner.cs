using Blockchain.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blockchain
{
    public class BlockMiner
    {
        // MINING_PERIOD je fisken čas, vsakih x milisekund se naredi nov blok,
        private static readonly int MINING_PERIOD = 20000;

        // Hashi se ne računajo, če je število transakcij v bloku manj ali enako kot STEVILO_TRANSAKCIJ_V_BLOKU_MIN
        private static readonly int STEVILO_TRANSAKCIJ_V_BLOKU_MIN = 1;

        // zahtevnost računanja: število ničel pove po približno koliko iteracijah se bo našel ustrezen hash. 4 ničle pomenijo nekaj 10000 ponovitev
        private static readonly string KOLIKO_NICEL_NA_ZACETKU_HASH = "0000";

        // kliče vsakič, preden začne računati hash od bloka
        private TransactionPool TransactionPool { get => DependencyManager.TransactionPool; }

        public List<Block> Blockchain { get; private set; }
        private CancellationTokenSource cancellationToken;

        /// <summary>
        /// >Startam na začetku izvajanja programa
        /// </summary>
        public BlockMiner()
        {
            Blockchain = new List<Block>();
        }

        /// <summary>
        /// Začetek izvajanja programa. DoGenerateBlock() se sam nikoli ne konča - vsebuje while(true), zato je uporabljen cancellationToken ?????
        /// </summary>
        public void Start()
        {
            cancellationToken = new CancellationTokenSource();
            Task.Run(() => DoGenerateBlock(), cancellationToken.Token);
            Console.WriteLine("Mining has started");
        }

        /// <summary>
        /// Ustavi izvajanje programa.
        /// </summary>
        public void Stop()
        {
            cancellationToken.Cancel();
            Console.WriteLine("Mining has stopped");
        }

        /// <summary>
        /// Sproži generiranje bloka in potem počaka toliko časa, kot je nastavljeno v spremenljivki MINIG_PERIOD
        /// </summary>
        private void DoGenerateBlock()
        {
            while (true)
            {
                var startTime = DateTime.Now.Millisecond;
                GenerateBlock();
                //SaveBlock();
                var endTime = DateTime.Now.Millisecond;
                var remainTime = MINING_PERIOD - (endTime - startTime);
                Thread.Sleep(remainTime < 0 ? 0 : remainTime);
            }
        }

        /// <summary>
        /// Shrani blok v bazo
        /// </summary>
        private void SaveBlock(Block block)
        {
            // Če je prvi blok == 0 prepišem file, sicer dodajam na konec
            if (block?.BlockNum == 0)
            {
                File.WriteAllText(@"MojBlockchain.json", JsonConvert.SerializeObject(block,Formatting.Indented) + Environment.NewLine);
            }
            else
            {
                // to se bo izvedlo, tudi če je block null
                File.AppendAllText(@"MojBlockchain.json", JsonConvert.SerializeObject(block, Formatting.Indented) + Environment.NewLine);
            }
        }

        /// <summary>
        /// Ta metoda naredi nov blok (vključno z dodjanjem transakcij, računanjem hash in dodanjanje na konec verige blokov
        /// </summary>
        private void GenerateBlock()
        {
            var lastBlock = Blockchain.LastOrDefault();
            var block = new Block()
            {
                TimeStamp = DateTime.UtcNow,
                Nounce = 0,
                TransactionList = TransactionPool.TakeAll(),
                BlockNum = (lastBlock?.BlockNum + 1 ?? 0),
                PrevHash = lastBlock?.Hash ?? string.Empty
            };

            // hash začnem računati, samo če je v bloku večje število transakcij, kot je nastavljeno v spremenljivki
            // to je zato, ker je rudarjenje (računanje hash) računsko zahtevna operacija.
            if (block.TransactionList.Count() >= STEVILO_TRANSAKCIJ_V_BLOKU_MIN)
            {
                MineBlock(block);
                Blockchain.Add(block);
                SaveBlock(block);
            }
        }

        /// <summary>
        /// Ta metoda rudari en blok toliko časa, da najde pravi hash, ki se začne z xxxx ničlami
        /// Ko hash najde, določi block.Hash in block.Nounce
        /// </summary>
        /// <param name="block"></param>
        private void MineBlock(Block block)
        {
            // izračuna merkleRootHash za vse transakcije v bloku
            string merkleRootHash = FindMerkleRootHash(block.TransactionList);
            long nounce = -1;
            string hash;
            do
            {
                nounce++;
                var rowData = block.BlockNum + block.PrevHash + block.TimeStamp.ToString() + nounce + merkleRootHash;
                hash = CalculateHash(CalculateHash(rowData));
            }

            // kakšna je zahtevnost je določeno v spremenljivki
            while (!hash.StartsWith(KOLIKO_NICEL_NA_ZACETKU_HASH));
            block.Hash = hash;
            block.Nounce = nounce;
        }

        /// <summary>
        /// Izračuna merkle root iz podanih transakcij.
        /// Za iste transakcije je merkleroo hasa vedno enak
        /// </summary>
        /// <param name="transactionList"></param>
        /// <returns></returns>
        private string FindMerkleRootHash(IList<Transaction> transactionList)
        {
            // najprej dobim listo hash, 2-krat kličem sha256
            var transactionStrList = transactionList.Select(
                tran => CalculateHash(
                    CalculateHash(tran.Tx + tran.Sender + tran.ReceiverAddress + tran.Type + tran.Value + tran.Description)
                    )
                ).ToList();
            // iz liste katere nato izračuna merkle root hash
            string BuildMerkeRootHash = BuildMerkleRootHash(transactionStrList);
            return BuildMerkeRootHash;
        }

        /// <summary>
        /// Naredi merklovo drevo iz vseh hashev transackij
        /// glej python primer: https://gist.github.com/shirriff/c9fb5d98e6da79d9a772#file-merkle-py
        /// </summary>
        /// <param name="merkelLeaves"></param>
        /// <returns></returns>
        private string BuildMerkleRootHash(IList<string> merkelLeaves)
        {
            if (merkelLeaves == null || !merkelLeaves.Any())
                return string.Empty;

            // če imam samo še en hash, potem je ta tudi merkletre root hash
            if (merkelLeaves.Count() == 1)
                return merkelLeaves.First();

            // če je spisek liho število, potem zadnji hash podojim, tako da dobim sodo število
            if (merkelLeaves.Count() % 2 > 0)
                merkelLeaves.Add(merkelLeaves.Last());

            var merkleBranches = new List<string>();

            // grem skozi listo in obdelam dva po dva hasha
            for (int i = 0; i < merkelLeaves.Count(); i += 2)
            {
                // združim dva string in iz njih izračunam hash z dvojnim klicem sha256
                var leafPair = string.Concat(merkelLeaves[i], merkelLeaves[i + 1]);
                merkleBranches.Add(CalculateHash(CalculateHash(leafPair)));
            }
            // rekurzivno kličem funkcijo, v kateri sem. Pri vsakem klicu se število listov, leaves, branhes zmanjša na polovico
            return BuildMerkleRootHash(merkleBranches);
        }

        /// <summary>
        /// Izračunam hash
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static string CalculateHash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    // ToString("x2") vrne hexadecimalno predstavitev byta, npr 13 vrne "0d"
                    // npr ToString("x") bi vrnil samo d
                    // x2 pomeni, da vrne 2 znaka, na začetku doda 0, če je potrebno
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}