using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace RegistrationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //dsi.bplaced.net dsi123456
            //dsi_registry.bplaced.net supersec

            string hostname = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostname);

            string ip = "";
            foreach (IPAddress address in host.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = address.ToString();
                    break;
                }
            }
            var port = new Random().Next(5000, 5500);

            var webHost = CreateHostBuilder(args, ip, port.ToString()).Build();
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
