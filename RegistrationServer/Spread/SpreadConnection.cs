using RegistrationServer.Listener;
using RegistrationServer.Proto;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Interface;
using spread;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace RegistrationServer.Spread
{
	public class SpreadConnection : ISpreadConnection
	{
		//private static string primaryMessagePrefix = "I am new boban:";

		public spread.SpreadConnection spreadConnection { get; }
		SpreadGroup spreadGroup { get; }

		List<string> groupMembers = new List<string>();

		public int GroupMemberCounter { get => groupMembers.Count; }

        public bool IsPrimary
        {
			get => primaryName == userName;
        }

		private bool threadSuspended;

		private string primaryName;

		public string userName;

        private readonly LobbyRepository _lobbyRepository;

		private MessageListener messageListener;

        public SpreadConnection(LobbyRepository lobbyRepository)
		{ 
            try
            {
                spreadConnection = new spread.SpreadConnection();
				messageListener = new MessageListener(spreadConnection);

			}
            catch (SpreadException e)
            {
                Console.Error.WriteLine("There was an error connecting to the daemon.");
                Console.WriteLine(e);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Can't find the daemon " + ConfigFile.SPREAD_ADDRESS);
                Console.WriteLine(e);
                Environment.Exit(1);
            }

            _lobbyRepository = lobbyRepository;
        }

        //Parameter user must be unique!
        public void Connect(string address, int port, string user, bool priority, bool groupMembership)
        {
            try
            {
                spreadConnection.Connect(address, port, user, priority, groupMembership);
                Console.WriteLine($"Log: connected with: {address}; {port}; {user}; {priority}; {groupMembership}");
				spreadGroup = JoinGroup(ConfigFile.SPREAD_GROUP_NAME);
				userName = user.Substring(0,8);
			}
            catch (SpreadException e)
            {
                Console.Error.WriteLine("There was an error connecting to the daemon.");
                Console.WriteLine(e);
                Environment.Exit(1);
            }
        }

        public SpreadGroup JoinGroup(string groupName)
        {
            SpreadGroup spreadGroup = new SpreadGroup();
            spreadGroup.Join(spreadConnection, groupName);
            return spreadGroup;
        }

        public void Run()
        {
			Test();

			while (true)
            {
                try
                {

					messageListener.Receive += (sender, e) => DisplayMessage(e.Message);
					messageListener.Listen();
                    //DisplayMessage(msg);
					//var thread = new Thread(() => HandleMessage(msg));
					//thread.Start();

					if (threadSuspended)
                    {
                        lock (this)
                        {
                            while (threadSuspended)
                            {
                                Monitor.Wait(this);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

		private void Test()
        {
			messageListener.Receive += (sender, e) => DisplayMessage(e.Message);
        }

		private void HandleDefaultMessages(SpreadMessage message)
		{
			Console.WriteLine("Default Message Recieved");
			MembershipInfo info = message.MembershipInfo;
			UpdateList(info.Members);
			if (info.IsCausedByJoin)
			{
				if (IsPrimary)
					MulticastIamPrimaryMessage();

				if (info.Members.Length == 1)
					primaryName = userName;

			}
			else if (info.IsCausedByLeave || info.IsCausedByDisconnect)
			{
				if (PrimaryLeft && IAmNewPrimary())
				{
					MulticastIamPrimaryMessage();
				}
			}
		}

		private bool IAmNewPrimary()
		{
			return userName == groupMembers.Max();
		}

		private bool PrimaryLeft
		{
			get => !groupMembers.Contains(primaryName);
		}

		private void UpdateList(SpreadGroup[] actualMembers)
		{
			groupMembers.Clear();
			groupMembers.AddRange(actualMembers.Select(m => m.ToString().Trim('#').Substring(0, 8)));
		}

        private bool getAcknForCreateLobby(LobbyInfo lobbyInfo)
        {
			acknowledgements.Add(lobbyInfo.Id, 0);

            while(true)
            {
				SpreadMessage message = ReceiveMessage();

				if(message.Type == (short) SpreadMulticastType.CreateLobbyAckn)
                {
					++acknowledgements[lobbyInfo.Id];
                }

				if(acknowledgements[lobbyInfo.Id] >= groupMembers.Count)
                {
					acknowledgements.Remove(lobbyInfo.Id);
					return true;
                }
            }

			// ToDo: return false after time t
        }

        public void MulticastLobbyInfoToReplicas(LobbyInfo lobbyInfo)
		{
			string jsonString = JsonSerializer.Serialize(lobbyInfo);
			SendMulticast(SpreadMulticastType.CreateLobbyToReplicas, jsonString);
		}

		public void MulticastLobbyInfoToPrimary(LobbyInfo lobbyInfo)
		{
			LobbySpreadDto lobbySpreadDto = new LobbySpreadDto();
			lobbySpreadDto.OriginalSender = userName;
			lobbySpreadDto.LobbyInfo = lobbyInfo;
			string jsonString = JsonSerializer.Serialize(lobbyInfo);
			SendMulticast(SpreadMulticastType.CreateLobbyToPrimary, jsonString);
		}

		private void MulticastIamPrimaryMessage()
		{
			SendMulticast(SpreadMulticastType.NewPrimary, userName);
			//var m = new SpreadMessage();
			//m.Data = Encoding.UTF8.GetBytes(primaryMessagePrefix + userName);
			//m.AddGroup(spreadGroup);
			//spreadConnection.Multicast(m);
		}

		public void SendMulticast(SpreadMulticastType type, string data)
		{
			var msg = new SpreadMessage
			{
				Data = Encode(data),
				Type = (short)type
			};
			msg.AddGroup(spreadGroup);
			msg.IsSafe = true;
			spreadConnection.Multicast(msg);
		}

		public SpreadMessage ReceiveMessage()
		{
			return spreadConnection.Receive();
		}

		private byte[] Encode(string s)
		{
			return Encoding.ASCII.GetBytes(s);
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
