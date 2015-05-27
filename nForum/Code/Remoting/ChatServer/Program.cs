using ChatMessages;
using Akka;
using Akka.Actor;
using System;
using System.Collections.Generic;
using Akka.Event;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var fluentConfig = FluentConfig.Begin()
                .StdOutLogLevel(LogLevel.DebugLevel)
                .LogConfigOnStart(true)
                .LogLevel(LogLevel.ErrorLevel)
                .LogLocal(
                    receive: true,
                    autoReceive: true,
                    lifecycle: true,
                    eventStream: true,
                    unhandled: true
                )
                .LogRemote(
                    lifecycleEvents: LogLevel.DebugLevel,
                    receivedMessages: true,
                    sentMessages: true
                )
                .StartRemotingOn("localhost", 8081)
                .Build();

            using (var system = ActorSystem.Create("MyServer", fluentConfig))
            {
                system.ActorOf<ChatServerActor>("ChatServer");

                Console.ReadLine();
            }
        }
    }

    class ChatServerActor : TypedActor,
        IHandle<SayRequest>,
        IHandle<ConnectRequest>,
        IHandle<NickRequest>,
        ILogReceive
    {
        private readonly HashSet<ActorRef> _clients = new HashSet<ActorRef>();

        public void Handle(SayRequest message)
        {
            var response = new SayResponse
            {
                Username = message.Username,
                Text = message.Text,
            };
            foreach (var client in _clients) client.Tell(response, Self);
        }

        public void Handle(ConnectRequest message)
        {
            _clients.Add(this.Sender);
            Sender.Tell(new ConnectResponse
            {
                Message = "Hello and welcome to Akka .NET chat example",
            }, Self);
        }

        public void Handle(NickRequest message)
        {
            var response = new NickResponse
            {
                OldUsername = message.OldUsername,
                NewUsername = message.NewUsername,
            };

            foreach (var client in _clients) 
                client.Tell(response, Self);
        }
    }
}
