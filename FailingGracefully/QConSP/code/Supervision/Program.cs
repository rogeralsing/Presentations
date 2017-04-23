using System;
using System.Threading.Tasks;
using Proto;

namespace Supervision
{
    public class ChildActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    Console.WriteLine("Child: I have started..");
                    break;
                }
                case Restarting _:
                {
                    Console.WriteLine("Child: I am restarting..");
                    break;
                }
                case string _:
                {
                    Console.WriteLine("Child: I got boom message..");
                    throw new Exception("Boom!");
                }
            }
            return Actor.Done;
        }
    }

    public class ParentActor : IActor
    {
        public static readonly ISupervisorStrategy MyStrategy = new OneForOneStrategy((pid, exception) =>
        {
            Console.WriteLine("Parent: I am handling failure:" + exception);
            return SupervisorDirective.Restart;
        }, 5, TimeSpan.FromSeconds(1));

        private PID _childPid;

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    var childProps = Actor.FromProducer(() => new ChildActor());
                    _childPid = context.Spawn(childProps);
                    _childPid.Tell("Pew!");

                    break;
                }
            }
            return Actor.Done;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var props = Actor.FromProducer(() => new ParentActor()).WithSupervisor(ParentActor.MyStrategy);
            var pid = Actor.Spawn(props);

            Console.ReadLine();
        }
    }
}