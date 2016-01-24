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

                while (true)
                {
                    var input = Console.ReadLine();
                    var command = Parser.Parse(input);
                    if (command.Command == "/nick")
                    {
                        chatClient.Tell(new BeginRenameUser
                        {
                            NewUsername = command.Text
                        });
                    }
                    else
                    {
                        chatClient.Tell(new BeginSay
                        {
                            Text = input
                        });
                    }
                }
            }
        }
    }

    public class BeginRenameUser
    {
        public string NewUsername { get; set; }
    }

    public class BeginSay
    {
        public string Text { get; set; }
    }

    public class ChatClientActor : ReceiveActor
    {
        public ChatClientActor()
        {
            var nick = "Roger";
            var server = Context.ActorSelection("akka.tcp://MyServer@127.0.0.1:8081/user/ChatServer");
            Console.WriteLine("Connecting....");
            server.Tell(new Connect {Username = nick});

            Receive<BeginSay>(m =>
            {
                var message = new Say
                {
                    Username = nick,
                    Text = m.Text
                };
                server.Tell(message);
            });

            Receive<BeginRenameUser>(m =>
            {
                var message = new RenameUser
                {
                    OldUsername = nick,
                    NewUsername = m.NewUsername
                };

                server.Tell(message);

                Console.WriteLine($"Changing nick to {m.NewUsername}");
                nick = m.NewUsername;
            });

            Receive<Connected>(m =>
            {
                Console.WriteLine("Connected!");
                Console.WriteLine(m.Message);
            });

            Receive<RenamedUser>(m =>
            {
                Console.WriteLine($"{m.OldUsername} is now known as {m.NewUsername}");
            });

            Receive<Said>(m =>
            {
                Console.WriteLine($"{m.Username}: {m.Text}");
            });
        }
    }
}