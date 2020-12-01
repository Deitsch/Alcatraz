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
        private readonly MessageListener listener;
        private readonly ISpreadService spreadService;

        private readonly Dictionary<string, int> acknCount = new Dictionary<string, int>();
        private readonly Dictionary<string, SuccessType> receivedFromPrimary = new Dictionary<string, SuccessType>();

        protected PassiveReplicationOperation(OperationType operationType, MessageListener listener, ISpreadService spreadService)
        {
            this.operationType = operationType;
            this.listener = listener;
            this.spreadService = spreadService;
        }

        protected abstract void SpecificOperation(SpreadMessage message);

        public void AddListener()
        {
            listener.Receive += (sender, e) => HandleMessage(e.Message);
        }

        public void Execute(SpreadDto spreadDto, OperationType operationType)
        {
            Console.WriteLine("Send to Primary");
            string jsonString = JsonSerializer.Serialize(spreadDto);
            spreadService.SendMulticast(MulticastType.ToPrimary, jsonString);

            GetOkFromPrimary(spreadDto);
        }

        private void HandleMessage(SpreadMessage message)
        {
            if (operationType != message.GetOperationType())
                return;

            switch (message.Type)
            {
                case (short)MulticastType.ToPrimary:
                    if (spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received on Primary");

                        Thread thread = new Thread(() => CollectAckn(message));
                        thread.Start();

                        Console.WriteLine("Send to Replica");
                        spreadService.SendMulticast(MulticastType.ToReplicas, message.Data);
                    }
                    break;

                case (short)MulticastType.ToReplicas:
                    if (!spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received on Replica");

                        SpecificOperation(message);

                        Console.WriteLine("Send Ackn to Primary");
                        spreadService.SendMulticast(MulticastType.AcknToPrimary, message.Data);
                    }
                    break;

                case (short)MulticastType.AcknToPrimary:
                    if (spreadService.IsPrimary)
                    {
                        Console.WriteLine("Received ACKN from Replica on Primary");

                        ++acknCount[message.GetLobbyId()];
                    }
                    break;

                case (short)MulticastType.ToOriginalSenderSuccessfully:
                    if (spreadService.UserName == message.GetOriginalSender())
                    {
                        Console.WriteLine("Received 'successfully' from primary on original sender");

                        receivedFromPrimary[message.GetLobbyId()] = SuccessType.Successfully;
                    }
                    break;

                case (short)MulticastType.ToOriginalSenderNotSuccessfully:
                    if (spreadService.UserName == message.GetOriginalSender())
                    {
                        Console.WriteLine("Received 'not successfully' from primary on original sender");

                        receivedFromPrimary[message.GetLobbyId()] = SuccessType.NotSuccessfully;
                    }
                    break;
            }
        }

        private void CollectAckn(SpreadMessage message)
        {
            bool allAcknReceived = false;
            string lobbyId = message.GetLobbyId();
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
                SpecificOperation(message);

                Console.WriteLine("Send 'successfully' to Original Sender");
                spreadService.SendMulticast(MulticastType.ToOriginalSenderSuccessfully, message.Data);
            }
            else
            {
                Console.WriteLine("Send 'not successfully' to Original Sender");
                spreadService.SendMulticast(MulticastType.ToOriginalSenderNotSuccessfully, message.Data);
            }
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

            throw new SpreadException("Not all servers acknowledged request");
        }
    }
}
