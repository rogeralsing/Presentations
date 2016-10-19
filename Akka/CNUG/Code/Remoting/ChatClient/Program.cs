using System;
using System.Linq;
using Akka.Actor;
using ChatMessages;

namespace ChatClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("MyClient"))
            {
                var chatClient = system.ActorOf(Props.Create<ChatClientActor>());
                chatClient.Tell(new ConnectRequest
                {
                    Username = "Roger"
                });

                while (true)
                {
                    var input = Console.ReadLine();
                    if (input.StartsWith("/"))
                    {
                        var parts = input.Split(' ');
                        var cmd = parts[0].ToLowerInvariant();
                        var rest = string.Join(" ", parts.Skip(1));

                        if (cmd == "/nick")
                        {
                            chatClient.Tell(new NickRequest
                            {
                                NewUsername = rest
                            });
                        }
                    }
                    else
                    {
                        chatClient.Tell(new SayRequest
                        {
                            Text = input
                        });
                    }
                }
            }
        }
    }

    internal class ChatClientActor : TypedActor,
        IHandle<ConnectRequest>,
        IHandle<ConnectResponse>,
        IHandle<NickRequest>,
        IHandle<NickResponse>,
        IHandle<SayRequest>,
        IHandle<SayResponse>, ILogReceive
    {
        private readonly ActorSelection _server =
            Context.ActorSelection("akka.tcp://MyServer@127.0.0.1:8081/user/ChatServer");

        private string _nick = "Roggan";

        public void Handle(ConnectRequest message)
        {
            Console.WriteLine("Connecting....");
            _server.Tell(message);
        }

        public void Handle(ConnectResponse message)
        {
            Console.WriteLine("Connected!");
            Console.WriteLine(message.Message);
        }

        public void Handle(NickRequest message)
        {
            message.OldUsername = _nick;
            Console.WriteLine("Changing nick to {0}", message.NewUsername);
            _nick = message.NewUsername;
            _server.Tell(message);
        }

        public void Handle(NickResponse message)
        {
            Console.WriteLine("{0} is now known as {1}", message.OldUsername, message.NewUsername);
        }

        public void Handle(SayRequest message)
        {
            message.Username = _nick;
            _server.Tell(message);
        }

        public void Handle(SayResponse message)
        {
            Console.WriteLine("{0}: {1}", message.Username, message.Text);
        }
    }
}