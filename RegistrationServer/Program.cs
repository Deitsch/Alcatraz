using RegistrationServer.Spread;
using spread;
using System;

namespace RegistrationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowMenu();
            HandleUserInput();
        }

        private static void ShowMenu()
        {
            Console.WriteLine("Menue");
            Console.WriteLine("1. Write");
            Console.WriteLine("2. Read");
        }

        private static void HandleUserInput()
        {
            string message = Console.ReadLine();
            int selectedOption = Convert.ToInt32(message);

            SpreadConn spread = new SpreadConn(new SpreadConnection());

            switch (selectedOption.toRegistrationServerOperation())
            {
                case RegistrationServerOperation.Write:
                    {
                        spread.Connect(ConfigFile.SPREAD_ADDRESS, ConfigFile.SPREAD_PORT, Guid.NewGuid().ToString(), ConfigFile.SPREAD_PRIORITY, ConfigFile.SPREAD_GROUP_MEMBERSHIP);
                        SpreadGroup spreadGroup = spread.JoinGroup(ConfigFile.SPREAD_GROUP_NAME);
                        string input = Console.ReadLine();
                        spread.SendMessage(input, spreadGroup);
                        break;
                    }
                case RegistrationServerOperation.Read:
                    {
                        spread.Connect(ConfigFile.SPREAD_ADDRESS, ConfigFile.SPREAD_PORT, Guid.NewGuid().ToString(), ConfigFile.SPREAD_PRIORITY, ConfigFile.SPREAD_GROUP_MEMBERSHIP);
                        SpreadGroup spreadGroup = spread.JoinGroup(ConfigFile.SPREAD_GROUP_NAME);
                        while (true)
                        {
                            string response = spread.ReceiveMessage();
                            Console.WriteLine(response);
                        }
                    }
                case RegistrationServerOperation.Join:
                    {
                        break;
                    }
                case RegistrationServerOperation.Leave:
                    {
                        break;
                    }
            }

            Console.ReadLine();
        }
    }
}
