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
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(
                              new HelloRequest { Name = "FUCKING SCHEISS GRPC" });
            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
