using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534;
namespace SAE
{
    public class Client
    {

    }

    public class ClientRequest
    {
        public bool Receive;
        public delegate bool CompareMessage(J2534Message Message);
        public delegate J2534Message AppendHeader(byte[] Message);
        public delegate void QueueMessage(J2534Message Message);
        
    }

    public class MessageRouter
    {
        private Channel Channel;
        private List<Client> Clients;

        public void QueueRequest(ClientRequest Request)
        {

        }

        public void QueueMessages(List<J2534Message> Messsages)
        {

        }

    }
}
