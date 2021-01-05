using RegistrationServer.Listener;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
using System;
using System.Threading;

namespace RegistrationServer.Spread.Operations
{
    public class UpdateIpAddressesOperation : PassiveReplicationOperation
    {
        public UpdateIpAddressesOperation(MessageListener listener, ISpreadService spreadService)
            : base(OperationType.UpdateIpAddresses, listener, spreadService) {}

        protected override void SpecificOperation(SpreadDto spreadDto)
        {
            if (spreadService.IsPrimary)
            {
                ipAddresses[spreadDto.LobbyId].Add(GetIpWithPort());

                foreach(string ip in ipAddresses[spreadDto.LobbyId])
                {
                    Console.WriteLine(ip);
                }

                // ToDo: write ipAddresses to file
            }
        }

        public void AddStartListener()
        {
            Console.WriteLine("Append Start Listener");
            listener.Receive += (sender, e) => HandleStartMessage(e.Message);
        }

        private void HandleStartMessage(SpreadMessage message)
        {
            if ((MulticastType)message.Type == MulticastType.StartUpdateIpAddressesOperation && spreadService.IsPrimary)
            {
                Thread thread = new Thread(() => Execute(message.ToSpreadDto()));
                thread.Start();
            }
        }
    }
}
