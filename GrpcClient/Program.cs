using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrpcClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            var response = client.SayHello(
                new HelloRequest { Name = "SCHEISS" }
                );

            Console.WriteLine(response.Message);

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
