using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Spread;
using RegistrationServer.Spread.Interface;
using spread;

namespace RegistrationServer
{
    public class GreeterService 
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly ISpreadConn _spread;

        public GreeterService(ILogger<GreeterService> logger, ISpreadConn spread)
        {
            _logger = logger;
            _spread = spread;
        }

        public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
