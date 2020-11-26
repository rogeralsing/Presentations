using System;
using System.Linq;
using System.Threading.Tasks;
using Proto;
using Proto.Router;

namespace FanOutQuickestReply
{
    public class DoWork
    {
        public string Payload { get; set; }
    }

    public class WorkerActor : IActor
    {
        private readonly Random _rnd = new Random();

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case DoWork work:
                {
                    await Task.Delay(_rnd.Next(10, 100));
                    context.Respond("Done " + context.Self.Id);
                    break;
                }
            }
        }
    }

    public class FanOutActor : IActor
    {
        private readonly PID[] _workers;

        public FanOutActor()
        {
            var workerProps = Actor.FromProducer(() => new WorkerActor());
            _workers = new PID[3];

            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = Actor.Spawn(workerProps);
            }
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case DoWork work:
                {
                    var tasks = _workers.Select(w => w.RequestAsync<string>(work)).ToList();
                    var any = Task.WhenAny(tasks);
                    context.ReenterAfter(any, t =>
                    {
                        context.Respond(t.Unwrap().Result);
                        return Actor.Done;
                    });

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
            var fanOutProps = Actor.FromProducer(() => new FanOutActor());
            var fanOutPid = Actor.Spawn(fanOutProps);

            var result = fanOutPid.RequestAsync<string>(new DoWork()).Result;
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}