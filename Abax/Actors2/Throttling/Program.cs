using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proto;
using Proto.Schedulers.SimpleScheduler;

namespace Throttling
{
    public record Tick;

    public class ThrottledActor : IActor
    {
        private readonly Queue<object> _messages = new();
        private ISimpleScheduler _scheduler;
        private int _tokens;

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    _scheduler = new SimpleScheduler(context);
                    _scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), context.Self, new Tick(), out _);
                    break;
                }
                case Tick _:
                {
                    _tokens = 3;
                    await ConsumeTimeSliceAsync();
                    break;
                }
                default:
                {
                    _messages.Enqueue(context.Message);
                    await ConsumeTimeSliceAsync();
                    break;
                }
            }
        }

        private async Task ConsumeTimeSliceAsync()
        {
            while (_tokens > 0 && _messages.Any())
            {
                var message = _messages.Dequeue();
                _tokens--;
                await ThrottledReceiveAsync(message);
            }
        }

        private Task ThrottledReceiveAsync(object message)
        {
            Console.WriteLine("Got Message " + message);
            return Task.CompletedTask;
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