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
            //try
            //{
            //    SpreadConn spread = new SpreadConn(new SpreadConnection());
            //    spread.Connect(ConfigFile.SPREAD_ADDRESS, ConfigFile.SPREAD_PORT, Guid.NewGuid().ToString(), ConfigFile.SPREAD_PRIORITY, ConfigFile.SPREAD_GROUP_MEMBERSHIP);
            //    SpreadGroup spreadGroup = spread.JoinGroup(ConfigFile.SPREAD_GROUP_NAME);

            //    recThread rt = new recThread(spread.spreadConnection);
            //    Thread rtt = new Thread(new ThreadStart(rt.run));
            //    rtt.Start();
            //}
            //catch (SpreadException e)
            //{
            //    Console.Error.WriteLine("There was an error connecting to the daemon.");
            //    Console.WriteLine(e);
            //    Environment.Exit(1);
            //}
            //catch (Exception e)
            //{
            //    Console.Error.WriteLine("Can't find the daemon " + ConfigFile.SPREAD_ADDRESS);
            //    Console.WriteLine(e);
            //    Environment.Exit(1);
            //}

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
