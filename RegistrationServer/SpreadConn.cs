using spread;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationServer.Spread
{
    public class SpreadConn
    {
        SpreadConnection spreadConnection;

        public SpreadConn(SpreadConnection spreadConnection)
        {
            this.spreadConnection = spreadConnection;
        }

        //Parameter user must be unique!
        public void Connect(string address, int port, string user, bool priority, bool groupMembership)
        {
            try
            {
                spreadConnection.Connect(address, port, user, priority, groupMembership);
                Console.WriteLine($"Log: connected with: {address}; {port}; {user}; {priority}; {groupMembership}");
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

        public void SendMessage(string message, SpreadGroup spreadGroup)
        {
            SpreadMessage msg = new SpreadMessage();
            msg.Data = Encoding.ASCII.GetBytes(message);
            msg.AddGroup(spreadGroup);
            msg.IsSafe = true;
            spreadConnection.Multicast(msg);
        }

        public string ReceiveMessage()
        {
            SpreadMessage spreadMessage = spreadConnection.Receive();

            if (spreadMessage.IsMembership)
            {
                MembershipInfo info = spreadMessage.MembershipInfo;
                if (info.IsCausedByJoin)
                    return "Joined: " + info.Joined;
                else if (info.IsCausedByLeave)
                    return $"Left: {info.Left}";
                else if (info.IsCausedByDisconnect)
                    return $"Disconnected: {info.Disconnected}";
                else
                    return $"Memership change";
            }
            else
            {
                return Encoding.ASCII.GetString(spreadMessage.Data);
            }
        }
    }
}
