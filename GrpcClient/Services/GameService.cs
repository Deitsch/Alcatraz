using System;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcClient.Game.Proto;
using Microsoft.Extensions.Logging;

namespace GrpcClient.Services
{
    public class GameService : Game.Proto.Game.GameBase
    {
        private readonly ILogger<GameService> _logger;


        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
        }

        public override Task<InitGameResponse> InitGame(InitGameRequest request, ServerCallContext context)
        {
            Console.WriteLine("Init Game");
            //var alcatraz = new Alcatraz.Alcatraz();
            //alcatraz.showWindow();
            return Task.FromResult(new InitGameResponse());
        }

        public override Task<StartGameResponse> StartGame(StartGameRequest request, ServerCallContext context)
        {
            Console.WriteLine("Start Game");
            //var alcatraz = new Alcatraz.Alcatraz();
            //alcatraz.showWindow();
            return Task.FromResult(new StartGameResponse());
        }
    }
}
