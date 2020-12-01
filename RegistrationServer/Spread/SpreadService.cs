using RegistrationServer.Listener;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
using System;
using System.Collections.Generic;
using System.Linq;

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

		private bool PrimaryLeft
		{
			get => !groupMembers.Contains(primaryName);
		}

		private string primaryName;

        private readonly ISpreadConnectionWrapper connection;
        private readonly MessageListener messageListener;

        public SpreadService(ISpreadConnectionWrapper connection, MessageListener messageListener)
		{ 
            this.connection = connection;
            this.messageListener = messageListener;
        }

        public void Run()
        {
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
				UpdateList(info.Members);

				if (info.IsCausedByJoin)
				{
					if (IsPrimary)
						SendMulticast(MulticastType.NewPrimary, UserName);

					if (info.Members.Length == 1)
						primaryName = UserName;

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
						break;
				}
			}
		}

		private void UpdateList(SpreadGroup[] actualMembers)
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
