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
        public static List<NetworkPlayer> NetworkPlayers;
        private readonly Alcatraz.Alcatraz _alcatraz;
        public static int Index;
        private NetworkPlayer me;
        public static PlayerState PlayerState = PlayerState.Unknown;
        public static bool ItsMyTurn;
        private string lastMoveToken;
        private string initGameToken;
        private string setCurrentPlayerToken;

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
            //_alcatraz = new Alcatraz.Alcatraz();
        }

        public override Task<InitGameResponse> InitGame(InitGameRequest request, ServerCallContext context)
        {
            if (initGameToken != request.Id)
            {
                Console.WriteLine("Init Game, wait for your move");
                NetworkPlayers = request.GameInfo.Players.ToList();
                Index = request.GameInfo.Index;
                me = NetworkPlayers[Index];
                PlayerState = PlayerState.InGame;
                //_alcatraz.init(request.GameInfo.Players.Count, _index);
            }
            else
            {
                Console.WriteLine("Received init more than once, so doing nothing");
            }
           
            return Task.FromResult(new InitGameResponse());
        }

        public override Task<SetCurrentPlayerResponse> SetCurrentPlayer(SetCurrentPlayerRequest request, ServerCallContext context)
        {
            if (initGameToken != request.Id)
            {
                Console.WriteLine($"It's your turn {me.Name}!");
                ItsMyTurn = true;
            }
            else
            {
                Console.WriteLine("Received init more than once, so doing nothing");
            }
            return Task.FromResult(new SetCurrentPlayerResponse());
        }

        public override Task<MakeMoveResponse> MakeMove(MakeMoveRequest request, ServerCallContext context)
        {
            //check idempotency token
            if (lastMoveToken != request.MoveInfo.Id)
            {
                //execute move
                Console.WriteLine($"{request.MoveInfo.PlayerName} did this move x:{request.MoveInfo.Prisoner.NewPoint.X} y:{request.MoveInfo.Prisoner.NewPoint.Y}");
                lastMoveToken = request.MoveInfo.Id;
            }
            else
            {
                //already done, other player has not received
                Console.WriteLine("Received a move more than once, so doing nothing");
            }
            
            return Task.FromResult(new MakeMoveResponse());
        }
    }
}
