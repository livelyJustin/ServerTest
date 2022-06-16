using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            _sessionFactory = sessionFactory;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }


        public void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pedning = socket.ConnectAsync(args);
            if (pedning == false)
                OnConnectCompleted(null, args);

        }

        public void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();

                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);

            }
            else
                Console.WriteLine($"OnConnectCompleted{args.SocketError}");
                
        }

    }
}
