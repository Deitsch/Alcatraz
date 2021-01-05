using RegistrationServer.Listener;
using RegistrationServer.Services;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
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
                ipAddresses[spreadDto.LobbyId].Add(NetworkUtils.GetIpWithPort());

                FtpService.UpdateAddresses(ipAddresses[spreadDto.LobbyId]);
            }
        }

        public void AddStartListener()
        {
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
