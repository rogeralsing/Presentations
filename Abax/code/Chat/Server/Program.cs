using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using chat.messages;
using Proto;
using Proto.Remote;

// set up the actor system
var system = new ActorSystem();
var context = new RootContext(system);

//set up the remoting layer
var remote = new Remote(system, RemoteConfig.BindToLocalhost(8000)
    .WithProtoMessages(ChatReflection.Descriptor));
remote.StartAsync();
var clients = new HashSet<PID>();

//define the server actor
var props = Props.FromFunc(
    ctx =>
    {
        switch (ctx.Message)
        {
            case Connect connect:
                Console.WriteLine($"Client {connect.Sender} connected");
                clients.Add(connect.Sender);
                ctx.Send(connect.Sender, new Connected {Message = "Welcome!"});
                break;
            case SayRequest sayRequest:
                foreach (var client in clients)
                {
                    ctx.Send(
                        client, new SayResponse
                        {
                            UserName = sayRequest.UserName,
                            Message = sayRequest.Message
                        }
                    );
                }

                break;
            case NickRequest nickRequest:
                foreach (var client in clients)
                {
                    ctx.Send(
                        client, new NickResponse
                        {
                            OldUserName = nickRequest.OldUserName,
                            NewUserName = nickRequest.NewUserName
                        }
                    );
                }

                break;
        }

        return Task.CompletedTask;
    }
);

//start the server actor
context.SpawnNamed(props, "chatserver");

//just keep the server app running
Console.ReadLine();