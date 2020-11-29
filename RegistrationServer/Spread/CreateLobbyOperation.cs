using RegistrationServer.Listener;
using RegistrationServer.Proto;
using RegistrationServer.utils;
using spread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RegistrationServer.Spread
{
    public class CreateLobbyOperation : Operation
    {
        SpreadConnection spreadConnection;
        int ackCount = 0;

        public CreateLobbyOperation(MessageListener listener, SpreadConnection spreadConnection) : base(listener)
        {
            this.spreadConnection = spreadConnection;
        }

        public override async Task<LobbyInfo> Execute(LobbyInfo lobbyInfo)
        {

            if (spreadConnection.IsPrimary)
            {
                spreadConnection.MulticastLobbyInfoToReplicas(lobbyInfo);
                while (true)
                {
                    Thread.Sleep(2000);
                    if (spreadConnection.GroupMemberCounter == ackCount)
                    {
                        
                    }
                    //Define Timeout
                }
            }
            else
            {
                spreadConnection.MulticastLobbyInfoToPrimary(lobbyInfo);
            }
        }

        public override void HandleMessage(SpreadMessage message)
        {
            if (message.Type == (short)SpreadMulticastType.CreateLobbyAckn)
            {
                ackCount++;
            }
            if (message.Type == (short)SpreadMulticastType.ReceiveLobbyFromPrimary)
            {
                var spreadDto = new LobbySpreadDto(message.Data);
                if (spreadDto.OriginalSender == spreadConnection.userName)

            }
        }
    }
}
