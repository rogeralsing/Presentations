using System;
using System.Threading;
using System.Threading.Tasks;
using Proto;
using Proto.Schedulers.SimpleScheduler;

namespace CircuitBreaker
{
    public class DB
    {
        public static Task<string[]> Exec(string msgQuery)
        {
            throw new NotImplementedException();
        }

        public static Task<bool> Ping()
        {
            throw new NotImplementedException();
        }
    }

    public class DbQuery
    {
        public string Query { get; set; }
    }

    public class DbResponse
    {
        public string[] Result { get; set; }
    }

    public class DbError
    {
        public string Message { get; set; }
    }

    public class DbPing {}

    public class DbActor : IActor
    {
        private readonly ISimpleScheduler _scheduler = new SimpleScheduler();
        private CancellationTokenSource _cts;

        public async Task DbUp(IContext context)
        {
            switch (context.Message)
            {
                case DbQuery msg:
                {
                    try
                    {
                        var res = await DB.Exec(msg.Query);
                        context.Respond(new DbResponse() {Result = res});
                    }
                    //ofc catch valid exceptions only, this is a demo..
                    catch 
                    {
                        //for a "proper" circuit breaker, we could count failures over time here
                        context.Respond(new DbError());
                        context.SetBehavior(DbDown);
                        _scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), context.Self, new DbPing(), out _cts);
                    }

                    break;
                }
            }
        }

        public async Task DbDown(IContext context)
        {
            switch (context.Message)
            {
                case DbPing _:
                {
                    var ok = await DB.Ping();
                    if (ok)
                    {
                        _cts.Cancel();
                        context.SetBehavior(DbUp);
                    }
                    break;
                }
                case DbQuery msg:
                {
                    //Fail fast, return DbError, or Cached or fallback value
                    context.Respond(new DbError());
                    break;
                }
            }
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started _:
                {
                    context.SetBehavior(DbUp);
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
            Console.WriteLine("Hello World!");
        }
    }
}