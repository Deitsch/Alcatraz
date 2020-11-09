using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new LobbyService.LobbyServiceClient(channel);
            var reply = await client.JoinLobbyAsync(
                              new JoinLobbyRequest { Name = "SWEGGRPC" });
            Console.WriteLine("Greeting: " + reply.Name);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
