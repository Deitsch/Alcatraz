using System;
using System.Threading.Tasks;
using Client.Game.Proto;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Client.Services
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
            Console.WriteLine("hi from client");

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
