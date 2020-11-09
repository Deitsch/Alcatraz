using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcClient.Proto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrpcClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new Lobby.LobbyClient(channel);
            var reply = await client.JoinLobbyAsync(new JoinLobbyRequest { Name = "OIDA" });
            foreach (var player in reply.Players)
            {
                Console.WriteLine("InLobby: " + player.Name);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
