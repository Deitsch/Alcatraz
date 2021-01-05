using Grpc.Core;
using RegistrationServer.Listener;
using RegistrationServer.Spread.Enums;
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
    public abstract class PassiveReplicationOperation
    {
        private readonly OperationType operationType;
        protected readonly MessageListener listener;
        protected readonly ISpreadService spreadService;

        private readonly Dictionary<string, int> acknCount = new Dictionary<string, int>();
        private readonly Dictionary<string, SuccessType> receivedFromPrimary = new Dictionary<string, SuccessType>();
        protected readonly Dictionary<string, List<string>> ipAddresses = new Dictionary<string, List<string>>();

        protected PassiveReplicationOperation(OperationType operationType, MessageListener listener, ISpreadService spreadService)
        {
            this.operationType = operationType;
            this.listener = listener;
            this.spreadService = spreadService;
        }

        protected abstract void SpecificOperation(SpreadDto spreadDto);

        public void AddListener()
        {
            listener.Receive += (sender, e) => HandleMessage(e.Message);
        }

        public void Execute(SpreadDto spreadDto)
        {
            Console.WriteLine("Execute " + operationType);
            Console.WriteLine("Send to Primary");
            string jsonString = JsonSerializer.Serialize(spreadDto);
            spreadService.SendMulticast(MulticastType.ToPrimary, jsonString);

            GetOkFromPrimary(spreadDto);
        }

        private void HandleMessage(SpreadMessage message)
        {
            if (operationType != message.GetOperationType())
                return;

            switch ((MulticastType)message.Type)
            {
                case MulticastType.ToPrimary:
                    if (spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received on Primary");
                        ipAddresses.Add(message.LobbyId(), new List<string>());

                        Thread thread = new Thread(() => CollectAckn(message));
                        thread.Start();

                        Console.WriteLine("Send to Replica");
                        spreadService.SendMulticast(MulticastType.ToReplicas, message.Data);
                    }
                    break;

                case MulticastType.ToReplicas:
                    if (!spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received on Replica");

                        var spreadDto = message.ToSpreadDto();
                        spreadDto.IpWithPort = NetworkUtils.GetIpWithPort();
                        string jsonString = JsonSerializer.Serialize(spreadDto);

                        SpecificOperation(spreadDto);

                        Console.WriteLine("Send Ackn to Primary");
                        spreadService.SendMulticast(MulticastType.AcknToPrimary, jsonString);
                    }
                    break;

                case MulticastType.AcknToPrimary:
                    if (spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received ACKN from Replica on Primary");

                        ++acknCount[message.LobbyId()];
                        ipAddresses[message.LobbyId()].Add(message.IpWithPort());
                    }
                    break;

                case MulticastType.ToOriginalSenderSuccessfully:
                    if (spreadService.UserName == message.OriginalSender())
                    {
                        Console.WriteLine("Received 'successfully' from primary on original sender");

                        receivedFromPrimary[message.LobbyId()] = SuccessType.Successfully;
                    }
                    break;

                case MulticastType.ToOriginalSenderNotSuccessfully:
                    if (spreadService.UserName == message.OriginalSender())
                    {
                        Console.WriteLine("Received 'not successfully' from primary on original sender");

                        receivedFromPrimary[message.LobbyId()] = SuccessType.NotSuccessfully;
                    }
                    break;
            }
        }

        private void CollectAckn(SpreadMessage message)
        {
            bool allAcknReceived = false;
            string lobbyId = message.LobbyId();
            acknCount.Add(lobbyId, 0);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed.TotalMinutes < 1)
            {
                if (acknCount[lobbyId] >= spreadService.GroupMemberCounter - 1)
                {
                    allAcknReceived = true;
                    break;
                }
                Thread.Sleep(1000);
            }

            sw.Stop();
            acknCount.Remove(lobbyId);

            if (allAcknReceived)
            {
                SpecificOperation(message.ToSpreadDto());

                Console.WriteLine("Send 'successfully' to Original Sender");
                spreadService.SendMulticast(MulticastType.ToOriginalSenderSuccessfully, message.Data);
            }
            else
            {
                Console.WriteLine("Send 'not successfully' to Original Sender");
                spreadService.SendMulticast(MulticastType.ToOriginalSenderNotSuccessfully, message.Data);
            }

            ipAddresses.Remove(lobbyId);
        }

        private void GetOkFromPrimary(SpreadDto spreadDto)
        {
            bool success = false;
            string lobbyId = spreadDto.LobbyId;
            receivedFromPrimary.Add(lobbyId, SuccessType.Unknown);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (sw.Elapsed.TotalMinutes < 1)
            {
                if (receivedFromPrimary[lobbyId] == SuccessType.Successfully)
                {
                    success = true;
                    break;
                }
                else if (receivedFromPrimary[lobbyId] == SuccessType.NotSuccessfully)
                {
                    break;
                }
                Thread.Sleep(1000);
            }

            if (success)
            {
                receivedFromPrimary.Remove(lobbyId);
                return;
            }

            throw new RpcException(new Status(StatusCode.Cancelled,
                    "Not all servers acknowledged request"));
        }
    }
}
