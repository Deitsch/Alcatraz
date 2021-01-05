using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RegistrationServer.Services;
using RegistrationServer.utils;

namespace RegistrationServer
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            //dsi.bplaced.net dsi123456
            //dsi_registry.bplaced.net supersec

            string port = NetworkUtils.SelectNextPort();
            string ip = NetworkUtils.GetIpAddress();

            if(string.IsNullOrWhiteSpace(ip))
            {
                Console.WriteLine("Ip not valid!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            if (string.IsNullOrWhiteSpace(port))
            {
                Console.WriteLine("All available ports are already in use!");
                Console.ReadLine();
                Environment.Exit(0);
            }

            var webHost = CreateHostBuilder(args, ip, port).Build();
            webHost.Start();
            webHost.WaitForShutdown();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args, string ip, string port) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                     webBuilder.UseUrls($"http://{ip}:{port}");
                });

    }
}
