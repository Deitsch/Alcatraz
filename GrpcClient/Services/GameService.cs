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

        public override Task<StartGameResonse> StartGame(StartGameRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StartGameResonse());
        }
    }
}
