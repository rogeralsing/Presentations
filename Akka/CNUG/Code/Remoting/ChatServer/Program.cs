using System;
using System.Collections.Generic;
using Akka.Actor;
using ChatMessages;

namespace ChatServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("MyServer"))
            {
                system.ActorOf<ChatServerActor>("ChatServer");

                Console.ReadLine();
            }
        }
    }

    internal class ChatServerActor : TypedActor,
        IHandle<SayRequest>,
        IHandle<ConnectRequest>,
        IHandle<NickRequest>,
        ILogReceive
    {
        private readonly HashSet<IActorRef> _clients = new HashSet<IActorRef>();

        public void Handle(ConnectRequest message)
        {
            _clients.Add(Sender);
            Sender.Tell(new ConnectResponse
            {
                Message = "Hello and welcome to Akka .NET chat example"
            }, Self);
        }

        public void Handle(NickRequest message)
        {
            var response = new NickResponse
            {
                OldUsername = message.OldUsername,
                NewUsername = message.NewUsername
            };

            foreach (var client in _clients)
                client.Tell(response, Self);
        }

        public void Handle(SayRequest message)
        {
            var response = new SayResponse
            {
                Username = message.Username,
                Text = message.Text
            };
            foreach (var client in _clients) client.Tell(response, Self);
        }
    }
}