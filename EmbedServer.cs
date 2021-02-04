using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace Blockchain
{
    public class EmbedServer
    {
        private WebServer server;
        private string url;
        public EmbedServer(string port)
        {
            url = $"http://*:{port}/";

            server = CreateWebServer(url);
        }
        public void Stop()
        {
            server.Dispose();
            Console.WriteLine($"http server stopped");
        }
        public void Start()
        {
            // Once we've registered our modules and configured them, we call the RunAsync() method.
            server.RunAsync();
            Console.WriteLine($"http server available at {url}");
        }

        private WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m.WithController<Controller>())
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));

            // Listen for state changes.
            // server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }

        public sealed class Controller : WebApiController
        {
            //GET http://localhost:5449/ping
            [Route(HttpVerbs.Get, "/ping")]
            public string TableTennis()
            {
                var pong = new Model.Ping(); // vrnem objekt pong z default vrednostmi class-a Model.Ping
                return JsonConvert.SerializeObject(pong);
            }

            //GET http://localhost:5449/api/block
            [Route(HttpVerbs.Get, "/block")]
            public string GetAllBlocks() => JsonConvert.SerializeObject(DependencyManager.BlockMiner.Blockchain);

            //GET http://localhost:5449/api/block/blocknum/{blocknum?}
            [Route(HttpVerbs.Get, "/block/blocknum/{blocknum?}")]
            public string GetAllBlocks(int blocknum)
            {
                Model.Block block = null;
                if (blocknum < DependencyManager.BlockMiner.Blockchain.Count)
                    block = DependencyManager.BlockMiner.Blockchain[blocknum];
                return JsonConvert.SerializeObject(block);
            }

            //GET http://localhost:5449/api/block/latest
            [Route(HttpVerbs.Get, "/block/latest")]
            public string GetLatestBlocks()
            {
                var block = DependencyManager.BlockMiner.Blockchain.LastOrDefault();
                return JsonConvert.SerializeObject(block);
            }

            //Post http://localhost:5449/api/add
            //Body >> {"Sender":"amir","ReceiverAddress":"bob","Type":"vplaèilo","Value":"10", "description":"Opis transakcije"}
            [Route(HttpVerbs.Post, "/add")]
            public void AddTransaction()
            {
                var data = HttpContext.GetRequestDataAsync<Model.Transaction>();
                if (data != null && data.Result != null)
                    DependencyManager.TransactionPool.AddRaw(data.Result);
            }

            //GET http://localhost:5449/api/peer
            // vrne 1 node
            [Route(HttpVerbs.Get, "/peer")]
            public string GetPeer()
            {
                // ko me kdo poklièe, vrnem podatke o svojem nodu
                var peer = new Model.Peer
                {
                    PeerCount = 1,
                    HighestBlockNum = DependencyManager.BlockMiner.Blockchain.Count(),
                    TimeStampUtcLastSeen = DateTime.UtcNow,
                    PeerPublicKey = "123",
                    PeerHostName = "hp8730w",
                    PeerHostip = "192.168.1.64",
                    PeerFrendlyName = "Moj Blokchain node",
                    NodeClass = "Odin"
                };
                return JsonConvert.SerializeObject(peer);
            }

            //GET http://localhost:5449/api/listpeers
            // vrne seznam znanih nodov
            [Route(HttpVerbs.Get, "/listpeers")]
            public string ListPeers()
            {
                // Create a list of peers.
                List<Model.Peer> peers = new List<Model.Peer>();
                
                var peer = new Model.Peer
                {
                    PeerCount = 1,
                    HighestBlockNum = DependencyManager.BlockMiner.Blockchain.Count(),
                    TimeStampUtcLastSeen = DateTime.UtcNow,
                    PeerPublicKey = "123",
                    PeerHostName = "hp8730w",
                    PeerHostip = "192.168.1.64",
                    PeerFrendlyName = "Moj Blokchain node",
                    NodeClass = "Odin"
                };
                peers.Add(peer);
                peers.Add(new Model.Peer());
                peers.Add(peer);
                
                return JsonConvert.SerializeObject(peers);
            }


        }
    }
}