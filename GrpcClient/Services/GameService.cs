using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcClient.Game.Proto;
using Microsoft.Extensions.Logging;

namespace GrpcClient.Services
{
    public class GameService : Game.Proto.Game.GameBase
    {

        private readonly ILogger<GameService> _logger;
        private List<NetworkPlayer> _networkPlayers;
        private readonly Alcatraz.Alcatraz _alcatraz;
        private int _index;
        private NetworkPlayer me;


        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
            //_alcatraz = new Alcatraz.Alcatraz();
        }

        public override Task<InitGameResponse> InitGame(InitGameRequest request, ServerCallContext context)
        {
            Console.WriteLine("Init Game, wait for your move");
            _networkPlayers = request.GameInfo.Players.ToList();
            _index = request.GameInfo.Index;
            me = _networkPlayers[_index];
            //_alcatraz.init(request.GameInfo.Players.Count, _index);
            return Task.FromResult(new InitGameResponse());
        }

        public override Task<SetCurrentPlayerResponse> SetCurrentPlayer(SetCurrentPlayerRequest request, ServerCallContext context)
        {
            Console.WriteLine($"It's your turn {me.Name}!");
            Console.ReadKey();
            var makeMove = new MakeMoveRequest
            {
                MoveInfo =
                {
                    Id = Guid.NewGuid().ToString(),
                    PlayerName = me.Name,
                    Prisoner = new Prisoner {OldPoint = null, NewPoint = new Point {X = 1, Y = 2},}
                }
            };
            var allGood = false;
            while (!allGood)
            {
                allGood = true;
                for (var index = 0; index < _networkPlayers.Count; index++)
                {
                    var player = _networkPlayers[index];
                    try
                    {
                        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                        var channel = GrpcChannel.ForAddress($"http://{player.Ip}:{player.Port}");
                        using (channel)
                        {
                            var gameClient = new Game.Proto.Game.GameClient(channel);
                            gameClient.MakeMove(makeMove);
                            //gameClient.InitGame(new InitGameRequest { GameInfo = gameInfo });
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        allGood = false;
                        Thread.Sleep(1000);
                        break;
                    }
                }
            }
            Console.WriteLine("You made a move!");
            var nextPlayer = _networkPlayers[(_index + 1) % _networkPlayers.Count];
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var c = GrpcChannel.ForAddress($"http://{nextPlayer.Ip}:{nextPlayer.Port}");
            var gClient = new Game.Proto.Game.GameClient(c);
            gClient.SetCurrentPlayer(new SetCurrentPlayerRequest());
            return Task.FromResult(new SetCurrentPlayerResponse());
        }

        private string lastMove;
        public override Task<MakeMoveResponse> MakeMove(MakeMoveRequest request, ServerCallContext context)
        {
            var alreadyExecuted = lastMove == request.MoveInfo.Id;
            //request.MoveInfo.PlayerName
            //request.MoveInfo.Prisoner.p
            return Task.FromResult(new MakeMoveResponse());
        }
    }
}
