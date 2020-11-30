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

        public override Task<SayHiResponse> Hi(SayHiRequest request, ServerCallContext context)
        {
            Console.WriteLine("hi from grpcclient");

            return Task.FromResult(new SayHiResponse());
        }

        public override Task<StartGameResponse> StartGame(StartGameRequest request, ServerCallContext context)
        {
            Console.WriteLine("sheesh sccr lul");
            //var alcatraz = new Alcatraz.Alcatraz();
            //alcatraz.showWindow();
            return Task.FromResult(new StartGameResponse());
        }
    }
}
