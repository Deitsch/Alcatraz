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
            List<string> addresses = new List<string>();
            var client = new WebClient();
            client.Credentials = new NetworkCredential("dsi_registry", "supersec");

            Stream myStream = client.OpenRead("ftp://dsi.bplaced.net/dsi.txt");
            StreamReader sr = new StreamReader(myStream);
            while (sr.Peek() >= 0)
            {
                addresses.Add(sr.ReadLine());
            }
            myStream.Close();

            Console.WriteLine("Registered Server: " + String.Join(",", addresses));

            var ip = GetLocalIPv4(NetworkInterfaceType.Ethernet);

            var port = 5001; //new Random().Next(5000, 5049);

            //using (var client = new WebClient())
            //{
            //    client.Credentials = new NetworkCredential("dsi_registry", "supersec");
            //    client.UploadFile("ftp://dsi.bplaced.net/path.txt", WebRequestMethods.Ftp.UploadFile, localFile);
            //}

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

        public static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

    }
}
