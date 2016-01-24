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

    public class ChatServerActor : ReceiveActor
    {
        public ChatServerActor()
        {
            var clients = new HashSet<IActorRef>();

            Receive<Connect>(m =>
            {
                clients.Add(Sender);
                Sender.Tell(new Connected
                {
                    Message = $"Hello {m.Username}, Welcome to Akka .NET chat example"
                }, Self);
            });

            Receive<RenameUser>(m =>
            {
                var response = new RenamedUser
                {
                    OldUsername = m.OldUsername,
                    NewUsername = m.NewUsername
                };

                foreach (var client in clients)
                    client.Tell(response, Self);
            });

            Receive<Say>(m =>
            {
                var response = new Said
                {
                    Username = m.Username,
                    Text = m.Text
                };
                foreach (var client in clients) client.Tell(response, Self);
            });
        }
    }    
}