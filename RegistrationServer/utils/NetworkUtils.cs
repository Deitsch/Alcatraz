using RegistrationServer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RegistrationServer.utils
{
    public static class NetworkUtils
    {
        private const int FIRST_PORT = 5001;
        private const int LAST_PORT = 5099;

        private static string ip;
        private static string port;

        public static string GetIpWithPort()
        {
            return ip + ":" + port;
        }

        public static string GetIpAddress()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostname);

            foreach (IPAddress address in host.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = address.ToString();
                    break;
                }
            }

            return ip;
        }

        public static string SelectNextPort()
        {
            List<string> addresses = FtpService.ReadAddresses();
            List<int> ports = addresses
                .Select(address => GetPortFromAddress(address))
                .Select(int.Parse)
                .ToList();

            if(ports.Any())
            {
                int newPort = ports.Max() + 1;
                if(newPort > LAST_PORT) 
                    newPort = FIRST_PORT;

                while(ports.Contains(newPort))
                {
                    ++newPort;
                    if (newPort > LAST_PORT)
                        return "";
                }
                
                port = newPort.ToString();
            }
            else
            {
                port = FIRST_PORT.ToString();
            }
            return port;
        }

        private static string GetPortFromAddress(string address)
        {
            return address[(address.IndexOf(':') + 1)..];
        }
    }
}
