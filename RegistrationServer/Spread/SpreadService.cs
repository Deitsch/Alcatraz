using RegistrationServer.Listener;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RegistrationServer.Spread
{
    public class SpreadService : ISpreadService
	{
        readonly List<string> groupMembers = new List<string>();

		public int GroupMemberCounter { get => groupMembers.Count; }

        public bool IsPrimary
        {
			get => primaryName == UserName;
        }

		public string UserName
		{
			get => connection.UserName;
		}

		public string Port { get; private set; }

		private bool PrimaryLeft
		{
			get => !groupMembers.Contains(primaryName);
		}

        private string primaryName;

        private readonly ISpreadConnectionWrapper connection;
        private readonly MessageListener messageListener;
        private readonly LobbyRepository lobbyRepository;

        public SpreadService(ISpreadConnectionWrapper connection, MessageListener messageListener, LobbyRepository lobbyRepository)
		{ 
            this.connection = connection;
            this.messageListener = messageListener;
            this.lobbyRepository = lobbyRepository;
        }

        public void Run()
        {
			Port = "8080"; // ToDo: choose next free port
			messageListener.Receive += (sender, e) => HandleMessage(e.Message);

			while (true)
            {
				messageListener.Listen();
            }
        }

        private void HandleMessage(SpreadMessage message)
		{
			if (message.IsMembership)
			{
				DisplayMembershipMessage(message);
				MembershipInfo info = message.MembershipInfo;
				UpdateActualMembers(info.Members);

				if (info.IsCausedByJoin)
				{
					if (IsPrimary)
                    {
						SendMulticast(MulticastType.NewPrimary, UserName);
						SendMulticast(MulticastType.UpdateDb, GetSerializedLobbies());
					}

					if (info.Members.Length == 1)
                    {
						primaryName = UserName;
						// ToDo: write ip address to file
					}


				}
				else if (info.IsCausedByLeave || info.IsCausedByDisconnect)
				{
					if (PrimaryLeft && IAmNewPrimary())
						SendMulticast(MulticastType.NewPrimary, UserName);
				}
			}
			else
			{
				switch ((MulticastType)message.Type)
				{
					case MulticastType.NewPrimary:
						primaryName = message.Data.DecodeToString();
						Console.WriteLine("New Primary was set: " + primaryName);

						if(IsPrimary)
							UpdateIpAddresses();

						break;

					case MulticastType.UpdateDb:
						if(!IsPrimary)
                        {
							var jsonString = message.Data.DecodeToString();
							var lobbieDtos = JsonSerializer.Deserialize<List<LobbyInfoDto>>(jsonString);
							var lobbies = lobbieDtos.Select(lobby => lobby.ToLobbyInfo()).ToList();
							lobbyRepository.UpdateAll(lobbies);
						}
						break;
				}
			}
		}

        private void UpdateIpAddresses()
        {
			var spreadDto = new SpreadDto
			{
				Type = OperationType.UpdateIpAddresses,
				OriginalSender = UserName,
				LobbyId = DateTime.Now.ToString()
			};

			var jsonString = JsonSerializer.Serialize(spreadDto);
			SendMulticast(MulticastType.StartUpdateIpAddressesOperation, jsonString);
		}

        private string GetSerializedLobbies()
        {
			var lobbies = lobbyRepository.FindAll().Select(lobby => lobby.ToDto()).ToList();
			return JsonSerializer.Serialize(lobbies);
		}

		private void UpdateActualMembers(SpreadGroup[] actualMembers)
		{
			groupMembers.Clear();
			groupMembers.AddRange(actualMembers.Select(m => m.ToString().Trim('#').Substring(0, 8)));
		}

		private bool IAmNewPrimary()
		{
			return UserName == groupMembers.Max();
		}

        public void SendMulticast(MulticastType multicastType, string data)
		{
			SendMulticast(multicastType, data.EncodeToByteArray());
		}

		public void SendMulticast(MulticastType multicastType,byte[] data)
		{
			var msg = new SpreadMessage
			{
				Data = data,
				Type = (short)multicastType
			};
			msg.AddGroup(connection.SpreadGroup);
			msg.IsSafe = true;
			connection.SpreadConnection.Multicast(msg);
		}

		private void DisplayMembershipMessage(SpreadMessage msg)
		{
			SpreadGroup[] groups = msg.Groups;

			Console.WriteLine("");
			Console.WriteLine("Received membership message");
			Console.WriteLine("Members:");
			for (int i = 0; i < groups.Length; i++)
				Console.WriteLine("  " + groups[i]);

			Console.WriteLine("");
		}
    }
}
