using RegistrationServer.Listener;
using RegistrationServer.Lobby.Proto;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

namespace RegistrationServer.Spread
{
    public class CreateLobbyOperation : Operation
    {
        private readonly ISpreadService spreadService;
        private readonly LobbyRepository lobbyRepository;

        private readonly Dictionary<string, int> acknCount = new Dictionary<string, int>();
        private readonly Dictionary<string, SuccessType> receivedFromPrimary = new Dictionary<string, SuccessType>();

        public CreateLobbyOperation(MessageListener listener, ISpreadService spreadService, LobbyRepository lobbyRepository) : base(listener)
        {
            this.spreadService = spreadService;
            this.spreadService = spreadService;
            this.lobbyRepository = lobbyRepository;
        }

        public void Execute(LobbyInfo lobbyInfo)
        {
            LobbyDto lobbyDto = new LobbyDto
            {
                LobbyInfo = lobbyInfo,
                OriginalSender = spreadService.UserName
            };
            spreadService.SendMulticast(MulticastType.CreateLobbyToPrimary, JsonSerializer.Serialize(lobbyDto));

            try
            {
                GetOkFromPrimary(lobbyInfo);
                return;
            }
            catch (SpreadException e)
            {
                throw e;
            }
        }

        public override void HandleMessage(SpreadMessage message)
        {
            switch(message.Type)
            {
                case (short)MulticastType.CreateLobbyToPrimary:
                    if(spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received on Primary");

                        Thread thread = new Thread(() => CollectAckn(message));
                        thread.Start();

                        spreadService.SendMulticast(MulticastType.CreateLobbyToReplicas, message.Data);
                    }
                    break;

                case (short)MulticastType.CreateLobbyToReplicas:
                    if (!spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received on Replica");

                        lobbyRepository.SaveLobby(message.GetLobby());
                        spreadService.SendMulticast(MulticastType.CreateLobbyAcknToPrimary, message.Data);
                    }
                    break;

                case (short)MulticastType.CreateLobbyAcknToPrimary:
                    if(spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received ACKN from Replica on Primary");

                        ++acknCount[message.GetLobby().Id];
                    }
                    break;

                case (short)MulticastType.CreateLobbyToOriginalSenderSuccessfully:
                    if (spreadService.UserName == message.GetOriginalSender())
                    {
                        Console.WriteLine("Received 'successfully' from primary on original sender");

                        receivedFromPrimary[message.GetLobby().Id] = SuccessType.Successfully;
                    }
                    break;

                case (short)MulticastType.CreateLobbyToOriginalSenderNotSuccessfully:
                    if (spreadService.UserName == message.GetOriginalSender())
                    {
                        Console.WriteLine("Received 'not successfully' from primary on original sender");

                        receivedFromPrimary[message.GetLobby().Id] = SuccessType.NotSuccessfully;
                    }
                    break;
            }
        }

        private void CollectAckn(SpreadMessage message)
        {
            bool allAcknReceived = false;
            string lobbyId = message.GetLobby().Id;
            acknCount.Add(lobbyId, 0);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed.TotalMinutes < 1)
            {
                if (acknCount[lobbyId] >= spreadService.GroupMemberCounter)
                {
                    allAcknReceived = true;
                    break;
                }
                Thread.Sleep(1000);
            }

            sw.Stop();
            acknCount.Remove(lobbyId);

            if(allAcknReceived)
            {
                lobbyRepository.SaveLobby(message.GetLobby());
                spreadService.SendMulticast(MulticastType.CreateLobbyToOriginalSenderSuccessfully, message.Data);
            }
            else
            {
                spreadService.SendMulticast(MulticastType.CreateLobbyToOriginalSenderNotSuccessfully, message.Data);
            }
        }

        private void GetOkFromPrimary(LobbyInfo lobbyInfo)
        {
            bool success = false;
            receivedFromPrimary.Add(lobbyInfo.Id, SuccessType.Unknown);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed.TotalMinutes < 1)
            {
                if (receivedFromPrimary[lobbyInfo.Id] == SuccessType.Successfully)
                {
                    success = true;
                }
                else if (receivedFromPrimary[lobbyInfo.Id] == SuccessType.NotSuccessfully)
                {
                    break;
                }
                Thread.Sleep(1000);
            }

            if(success)
            {
                receivedFromPrimary.Remove(lobbyInfo.Id);
                return;
            }

            throw new SpreadException("Not all servers acknowledged request");
        }
    }
}
