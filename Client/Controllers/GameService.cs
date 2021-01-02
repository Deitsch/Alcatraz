using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Client.Game.Proto;
using Client.Services;
using Grpc.Net.Client;

namespace Client.Controllers
{
    public class GameService : Client.Game.Proto.Game.GameBase
    {

        private readonly ILogger<GameService> _logger;
        public static List<NetworkPlayer> NetworkPlayers;
        public static int Index;
        public static PlayerState PlayerState = PlayerState.Unknown;
        private string lastMoveToken;
        private string initGameToken;

        public static MoveListener moveListener { get; private set; }
        public static Alcatraz.Alcatraz game;

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
        }

        public override Task<InitGameResponse> InitGame(InitGameRequest request, ServerCallContext context)
        {
            if (initGameToken != request.Id)
            {
                
                NetworkPlayers = request.GameInfo.Players.ToList();
                Index = request.GameInfo.Index;
                Console.WriteLine($" Welcome {request.GameInfo.Players[Index].Name} - index : {Index}");
                PlayerState = PlayerState.InGame;
            
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                game = new Alcatraz.Alcatraz();
                moveListener = new MoveListener(this);

                game.init(request.GameInfo.Players.Count, Index);
                
                for (int i = 0; i < request.GameInfo.Players.Count; i++)
                {
                    game.getPlayer(i).Name = request.GameInfo.Players[i].Name;
                }
                
                game.addMoveListener(moveListener);

                Thread t = new Thread(new ThreadStart(startGame));
                t.Name = "GUI Thread";
                t.Start();
            }
            else
            {
                Console.WriteLine("Received init more than once");
            }
           
            return Task.FromResult(new InitGameResponse());
        }

        private void startGame() {
            _logger.LogInformation(Thread.CurrentThread.Name + " started");
            game.showWindow();
            game.start();
            Application.Run();
        }

        //public override Task<SetCurrentPlayerResponse> SetCurrentPlayer(SetCurrentPlayerRequest request, ServerCallContext context)
        //{
        //    if (initGameToken != request.Id)
        //    {
        //        Console.WriteLine($"It's your turn {me.Name}!");

        //    }
        //    else
        //    {
        //        Console.WriteLine($"Received current player token {initGameToken} more than once -> skip");
        //    }
        //    return Task.FromResult(new SetCurrentPlayerResponse());
        //}

        public override Task<MakeMoveResponse> RemoteMove(MakeMoveRequest request, ServerCallContext context)
        {
            MoveInformation moveInfo = request.MoveInfo;
            //check idempotency token
            if (lastMoveToken != request.MoveInfo.Id)
            {
                //execute move
                Alcatraz.Player player = game.getPlayer(moveInfo.Player);
                Alcatraz.Prisoner prisoner = game.getPrisoner(moveInfo.Prisoner.Id);
                Console.WriteLine("Player: " + player.Name + " id: " + player.Id + " is moving " + prisoner + " to " + (moveInfo.RowOrCol == Alcatraz.Alcatraz.ROW ? "row" : "col") + " " + (moveInfo.RowOrCol == Alcatraz.Alcatraz.ROW ? moveInfo.Row : moveInfo.Col));
                game.doMove(player, prisoner, moveInfo.RowOrCol, moveInfo.Row, moveInfo.Col);
                lastMoveToken = moveInfo.Id;
            }
            else
            {
                //already done, other player has not received
                Console.WriteLine($"Received move {lastMoveToken} more than once -> skip");
            }
            
            return Task.FromResult(new MakeMoveResponse());
        }

        public void DoMoveAndSetNextPlayer(Alcatraz.Player player, Alcatraz.Prisoner prisoner, int rowOrCol, int row, int col)
        {

            var makeMove = new MakeMoveRequest
            {
                MoveInfo = new MoveInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Player = Index,
                    Prisoner = new AlcatrazFigure { Id = prisoner.Id },
                    RowOrCol = rowOrCol,
                    Row = row,
                    Col = col
                }
            };
            MakeReliableMove(makeMove);
            Console.WriteLine("Move sent to other Players!");

            //var nextPlayer = GameService.NetworkPlayers[(GameService.Index + 1) % GameService.NetworkPlayers.Count];
            //SetNextPlayerReliable(nextPlayer);
        }

        private static void MakeReliableMove(MakeMoveRequest makeMove)
        {
            var allGood = false;
            while (!allGood)
            {
                allGood = true;
                for (var index = 0; index < GameService.NetworkPlayers.Count; index++)
                {
                    if(index != Index)
                    {
                        var player = GameService.NetworkPlayers[index];
                        try
                        {
                            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                            using var channel = GrpcChannel.ForAddress($"http://{player.Ip}:{player.Port}");
                            var gameClient = new Game.Proto.Game.GameClient(channel);
                            gameClient.RemoteMove(makeMove);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Player {player.Ip}:{player.Port} did not respond -> retry in 1000 ms");
                            allGood = false;
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                }
            }
        }

        //private static void SetNextPlayerReliable(NetworkPlayer nextPlayer)
        //{
        //    var allGood = false;
        //    while (!allGood)
        //    {
        //        allGood = true;
        //        try
        //        {
        //            using var c = GrpcChannel.ForAddress($"http://{nextPlayer.Ip}:{nextPlayer.Port}");
        //            var gClient = new Game.Proto.Game.GameClient(c);
        //            gClient.SetCurrentPlayer(new SetCurrentPlayerRequest());
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine($"Player {nextPlayer.Ip}:{nextPlayer.Port} did not respond -> retry in 1000 ms");
        //            allGood = false;
        //            Thread.Sleep(1000);
        //        }
        //    }
        //}
    }
}
