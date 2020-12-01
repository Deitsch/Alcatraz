using RegistrationServer.Listener;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
		} //

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
				DisplayMessage(message);
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
				switch (message.Type)
				{
					case (short)MulticastType.NewPrimary:
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

        public void SendMulticast(MulticastType type, string data)
		{
			SendMulticast(type, data.EncodeToByteArray());
		}

		public void SendMulticast(MulticastType type, byte[] data)
		{
			var msg = new SpreadMessage
			{
				Data = data,
				Type = (short)type
			};
			msg.AddGroup(connection.SpreadGroup);
			msg.IsSafe = true;
			connection.SpreadConnection.Multicast(msg);
		}

		private void DisplayMessage(SpreadMessage msg)
		{
			try
			{
				Console.WriteLine("*****************RECTHREAD Received Message************");
				if (msg.IsRegular)
				{
					Console.Write("Received a ");
					if (msg.IsUnreliable)
						Console.Write("UNRELIABLE");
					else if (msg.IsReliable)
						Console.Write("RELIABLE");
					else if (msg.IsFifo)
						Console.Write("FIFO");
					else if (msg.IsCausal)
						Console.Write("CAUSAL");
					else if (msg.IsAgreed)
						Console.Write("AGREED");
					else if (msg.IsSafe)
						Console.Write("SAFE");
					Console.WriteLine(" message.");

					Console.WriteLine("Sent by  " + msg.Sender + ".");

					Console.WriteLine("Type is " + msg.Type + ".");

					if (msg.EndianMismatch == true)
						Console.WriteLine("There is an endian mismatch.");
					else
						Console.WriteLine("There is no endian mismatch.");

					SpreadGroup[] groups = msg.Groups;
					Console.WriteLine("To " + groups.Length + " groups.");

					byte[] data = msg.Data;
					Console.WriteLine("The data is " + data.Length + " bytes.");

					Console.WriteLine("The message is: " + System.Text.Encoding.ASCII.GetString(data));
				}
				else if (msg.IsMembership)
				{
					MembershipInfo info = msg.MembershipInfo;

					if (info.IsRegularMembership)
					{
						SpreadGroup[] groups = msg.Groups;

						Console.WriteLine("Received a REGULAR membership.");
						Console.WriteLine("For group " + info.Group + ".");
						Console.WriteLine("With " + groups.Length + " members.");
						Console.WriteLine("I am member " + msg.Type + ".");
						for (int i = 0; i < groups.Length; i++)
							Console.WriteLine("  " + groups[i]);

						Console.WriteLine("Group ID is " + info.GroupID);

						Console.Write("Due to ");
						if (info.IsCausedByJoin)
						{
							Console.WriteLine("the JOIN of " + info.Joined + ".");
						}
						else if (info.IsCausedByLeave)
						{
							Console.WriteLine("the LEAVE of " + info.Left + ".");
						}
						else if (info.IsCausedByDisconnect)
						{
							Console.WriteLine("the DISCONNECT of " + info.Disconnected + ".");
						}
						else if (info.IsCausedByNetwork)
						{
							SpreadGroup[] stayed = info.Stayed;
							Console.WriteLine("NETWORK change.");
							Console.WriteLine("VS set has " + stayed.Length + " members:");
							for (int i = 0; i < stayed.Length; i++)
								Console.WriteLine("  " + stayed[i]);
						}
					}
					else if (info.IsTransition)
					{
						Console.WriteLine("Received a TRANSITIONAL membership for group " + info.Group);
					}
					else if (info.IsSelfLeave)
					{
						Console.WriteLine("Received a SELF-LEAVE message for group " + info.Group);
					}
				}
				else if (msg.IsReject)
				{
					// Received a Reject message 
					Console.Write("Received a ");
					if (msg.IsUnreliable)
						Console.Write("UNRELIABLE");
					else if (msg.IsReliable)
						Console.Write("RELIABLE");
					else if (msg.IsFifo)
						Console.Write("FIFO");
					else if (msg.IsCausal)
						Console.Write("CAUSAL");
					else if (msg.IsAgreed)
						Console.Write("AGREED");
					else if (msg.IsSafe)
						Console.Write("SAFE");
					Console.WriteLine(" REJECTED message.");

					Console.WriteLine("Sent by  " + msg.Sender + ".");

					Console.WriteLine("Type is " + msg.Type + ".");

					if (msg.EndianMismatch == true)
						Console.WriteLine("There is an endian mismatch.");
					else
						Console.WriteLine("There is no endian mismatch.");

					SpreadGroup[] groups = msg.Groups;
					Console.WriteLine("To " + groups.Length + " groups.");

					byte[] data = msg.Data;
					Console.WriteLine("The data is " + data.Length + " bytes.");

					Console.WriteLine("The message is: " + System.Text.Encoding.ASCII.GetString(data));
				}
				else
				{
					Console.WriteLine("Message is of unknown type: " + msg.ServiceType);
					byte[] data = msg.Data;
					Console.WriteLine("The data is " + data.Length + " bytes.");

					Console.WriteLine("The message is: " + System.Text.Encoding.ASCII.GetString(data));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Environment.Exit(1);
			}
		}
    }
}
