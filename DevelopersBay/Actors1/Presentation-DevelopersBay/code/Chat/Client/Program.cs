using System;
using System.Threading.Tasks;
using chat.messages;
using Proto;
using Proto.Remote;

//create the actor system
var system = new ActorSystem();
var context = system.Root;

//set up the remoting layer
var remote = new Remote(system,
    RemoteConfig.BindToLocalhost()
        .WithProtoMessages(ChatReflection.Descriptor));
remote.StartAsync();

//create a PID pointing to the server
var server = PID.FromAddress("127.0.0.1:8000", "chatserver");

//define our chat actor
var props = Props.FromFunc(
    ctx =>
    {
        switch (ctx.Message)
        {
            case Connected connected:
                Console.WriteLine(connected.Message);
                break;
            case SayResponse sayResponse:
                Console.WriteLine($"{sayResponse.UserName} {sayResponse.Message}");
                break;
            case NickResponse nickResponse:
                Console.WriteLine($"{nickResponse.OldUserName} is now {nickResponse.NewUserName}");
                break;
        }

        return Task.CompletedTask;
    }
);

//spawn the chat actor
var client = context.Spawn(props);

//connect the chat client to the server
context.Send(
    server, new Connect
    {
        Sender = client
    }
);

var nick = "Roger";
while (true)
{
    var text = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(text))
        continue;

    if (text.Equals("/exit"))
        return;

    if (text.StartsWith("/nick "))
    {
        var t = text.Split(' ')[1];

        context.Send(
            server, new NickRequest
            {
                OldUserName = nick,
                NewUserName = t
            }
        );
        nick = t;
    }
    else
    {
        context.Send(
            server, new SayRequest
            {
                UserName = nick,
                Message = text
            }
        );
    }
}