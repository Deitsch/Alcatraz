using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RegistrationServer.Spread;
using spread;

namespace RegistrationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("Enter your port: ");
            var port = Console.ReadLine();
            var webHost = CreateHostBuilder(args, port).Build();
            webHost.Start();

            SpreadConn spread = new SpreadConn();
            spread.Connect(ConfigFile.SPREAD_ADDRESS, ConfigFile.SPREAD_PORT, Guid.NewGuid().ToString(), ConfigFile.SPREAD_PRIORITY, ConfigFile.SPREAD_GROUP_MEMBERSHIP);
            spread.Run();

            webHost.WaitForShutdown();

        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args, string port) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                     webBuilder.UseUrls($"http://localhost:{port}");
                });

    }
}
