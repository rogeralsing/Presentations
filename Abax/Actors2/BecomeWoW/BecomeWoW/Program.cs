using System;
using System.Threading.Tasks;
using Proto;

namespace Become
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to World of Akka Craft");
            Console.WriteLine("Use 'hit' to fight");
            Console.WriteLine("Use 'ress' to ressurect the dead");
            var system = new ActorSystem();
            {
                var playerProps = Props.FromProducer(() => new Player());
                var player = system.Root.Spawn(playerProps);
                while (true)
                {
                    var input = Console.ReadLine();
                    switch (input)
                    {
                        case "hit":
                            system.Root.Send(player,new Hit());
                            break;
                        case "ress":
                            system.Root.Send(player,new Resurrect());
                            break;
                    }
                }
            }
        }
    }

    internal record Hit;

    internal record Resurrect;

    internal class Player : IActor
    {
        private int _hitpoints;
        private int _maxHitpoints = 40;
        private readonly Behavior _behavior;
        private string _name;

        public Player()
        {
            _name = "Magne the ferocious";
            _behavior = new Behavior();
            _hitpoints = _maxHitpoints;
            _behavior.Become(Alive);
        }

        private Task Alive(IContext context)
        {
            var message = context.Message;

            switch (message)
            {
                case Resurrect:
                    Console.WriteLine($"{_name} is already alive...");
                    break;
                case Hit:
                {
                    _hitpoints -= 10;
                    Console.WriteLine($"You hit {_name} for 10 damage...");
                    
                    switch (_hitpoints)
                    {
                        case <= 0:
                            Console.WriteLine("Player dies...");
                            Whisper("I had lag! bad lag!");
                            ScheduleWhisperToRess(context);
                            _behavior.Become(Dead);
                            break;
                        case <= 10:
                            Console.WriteLine($"{_name} is about to die");
                            break;
                        case <= 20:
                            Console.WriteLine($"{_name} is critically wounded");
                            break;
                    }
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private Task Dead(IContext context)
        {
            var message = context.Message;
            
            switch (message)
            {
                case Resurrect:
                    Console.WriteLine("You resurrect the player, he is alive again!! woot!");
                    _hitpoints = 40;
                    _behavior.Become(Alive);
                    break;
                case Hit:
                    Console.WriteLine("Player is already dead...");
                    break;
            }

            return Task.CompletedTask;
        }

        private void ScheduleWhisperToRess(IContext context)
        {
            context.ReenterAfter(Task.Delay(5000), () => Whisper("ress me plix!!!11"));
        }

        private  void Whisper(string text)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{_name}] whispers:{text}");
            Console.ForegroundColor = tmp;
        }

        public Task ReceiveAsync(IContext context) => _behavior.ReceiveAsync(context);
    }
}