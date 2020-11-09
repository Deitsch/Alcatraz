using spread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationServer.Spread.Interface
{
    public interface ISpreadConn
    {

        public void Connect(string address, int port, string user, bool priority, bool groupMembership);

        public SpreadGroup JoinGroup(string groupName);

        public void SendMessage(string message);

        public void SendMessage(string message, SpreadGroup spreadGroup);

        public string ReceiveMessage();
    }
}
