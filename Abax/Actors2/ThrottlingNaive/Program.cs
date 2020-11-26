using System;
using System.Threading.Tasks;
using Proto;

namespace ThrottlingNaive
{
    public class ThrottledActor : IActor
    {
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case string msg:
                {
                    Console.WriteLine("Got Message " + msg);
                    await Task.Delay(TimeSpan.FromMilliseconds(333));
                    break;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var system = new ActorSystem();
            var props = Props.FromProducer(() => new ThrottledActor());
            var pid = system.Root.Spawn(props);

            for (int i = 0; i < 100; i++)
            {
                system.Root.Send(pid, $"Hello {i}");
            }
            Console.ReadLine();
        }
    }
}