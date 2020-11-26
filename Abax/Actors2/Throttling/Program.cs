using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proto;
using Proto.Schedulers.SimpleScheduler;

namespace Throttling
{
    public class Tick {}

    public class ThrottledActor : IActor
    {
        private readonly Queue<object> _messages = new Queue<object>();
        private readonly ISimpleScheduler _scheduler = new SimpleScheduler();
        private int _tokens = 0;

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
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
            return Actor.Done;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var props = Actor.FromProducer(() => new ThrottledActor());
            var pid = Actor.Spawn(props);

            for (int i = 0; i < 100; i++)
            {
                pid.Tell("Hello " + i);
            }
            Console.ReadLine();
        }
    }
}