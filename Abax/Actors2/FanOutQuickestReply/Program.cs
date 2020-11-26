using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proto;
using Proto.Router;

namespace FanOutQuickestReply
{
    public record DoWork(string Payload);

    public class WorkerActor : IActor
    {
        private readonly Random _rnd = new();

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case DoWork work:
                {
                    await Task.Delay(_rnd.Next(10, 100));
                    context.Respond($"Done {context.Self.Id}");
                    break;
                }
            }
        }
    }

    public class FanOutActor : IActor
    {
        private readonly IReadOnlyCollection<PID> _workers;

        public FanOutActor(IReadOnlyCollection<PID> workers)
        {
            _workers = workers;
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case DoWork work:
                {
                    var tasks = new List<Task<string>>();
                    foreach (var w in _workers)
                    {
                        var t = context.RequestAsync<string>(w, work);
                        tasks.Add(t);
                    }
                    
                    var res = await Task.WhenAny(tasks).Unwrap();
                    
                    context.Respond(res);

                    break;
                }
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var system = new ActorSystem();
            var context = system.Root;
            
            var workerProps = Props.FromProducer(() => new WorkerActor());

            var workers = new List<PID>();
            for (var i = 0; i < 10; i++)
            {
                var w = context.Spawn(workerProps);
                workers.Add(w);
            }

            var fanOutProps = Props.FromProducer(() => new FanOutActor(workers));
            var fanOutPid = context.Spawn(fanOutProps);

            for (var i = 0; i < 10; i++)
            {
                var result = await context.RequestAsync<string>(fanOutPid, new DoWork("somework"));
                Console.WriteLine(result);
                Console.ReadLine();
            }
        }
    }
}