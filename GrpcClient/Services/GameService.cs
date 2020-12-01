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
        public static bool ItsMyTurn = false;

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
            //_alcatraz = new Alcatraz.Alcatraz();
        }

        public override Task<InitGameResponse> InitGame(InitGameRequest request, ServerCallContext context)
        {
            Console.WriteLine("Init Game, wait for your move");
            NetworkPlayers = request.GameInfo.Players.ToList();
            Index = request.GameInfo.Index;
            me = NetworkPlayers[Index];
            PlayerState = PlayerState.InGame;
            //_alcatraz.init(request.GameInfo.Players.Count, _index);
            return Task.FromResult(new InitGameResponse());
        }

        public override Task<SetCurrentPlayerResponse> SetCurrentPlayer(SetCurrentPlayerRequest request, ServerCallContext context)
        {
            Console.WriteLine($"It's your turn {me.Name}!");
            ItsMyTurn = true;
            return Task.FromResult(new SetCurrentPlayerResponse());
        }

        private string lastMove;
        public override Task<MakeMoveResponse> MakeMove(MakeMoveRequest request, ServerCallContext context)
        {
            Console.WriteLine($"{request.MoveInfo.PlayerName} did this move x:{request.MoveInfo.Prisoner.NewPoint.X} y:{request.MoveInfo.Prisoner.NewPoint.Y}");
            var alreadyExecuted = lastMove == request.MoveInfo.Id;
            //request.MoveInfo.PlayerName
            //request.MoveInfo.Prisoner.p
            return Task.FromResult(new MakeMoveResponse());
        }
    }
}
