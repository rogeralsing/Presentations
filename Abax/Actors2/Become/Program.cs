using System;
using Akka.Actor;

namespace Become
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to World of Akka Craft");
            Console.WriteLine("Use 'hit' to fight");
            Console.WriteLine("Use 'ress' to ressurect the dead");
            using (ActorSystem system = ActorSystem.Create("foo"))
            {
                var player = system.ActorOf<Player>();
                while (true)
                {
                    string input = Console.ReadLine();
                    if (input == "hit")
                        player.Tell(new Hit());
                    if (input == "ress")
                        player.Tell(new Resurrect());
                }
            }
        }
    }

    internal class Hit
    {
    }

    internal class Resurrect
    {
    }

    internal class Player : UntypedActor
    {
        private int _hitpoints;
        private int _maxHitpoints = 40;

        public Player()
        {
            _hitpoints = _maxHitpoints;
            Become(Alive);
        }

        private void Alive(object message)
        {
            if (message is Resurrect)
            {
                Console.WriteLine("Player is already alive...");
            }
            else if (message is Hit)
            {
                Console.WriteLine("You hit player for 10 damage...");
                _hitpoints -= 10;
                if (_hitpoints <= 0)
                {
                    Console.WriteLine("Player dies...");
                    ScheduleWhisperToRess();
                    Become(Dead);
                }
            }
        }

        private void Dead(object message)
        {
            if (message is Resurrect)
            {
                Console.WriteLine("You resurrect the player, he is alive again!! woot!");
                Become(Alive);
            }
            else if (message is Hit)
            {
                Console.WriteLine("Player is already dead...");
            }
        }


        protected override void OnReceive(object message)
        {
        }

        private static void ScheduleWhisperToRess()
        {
            Context.System.Scheduler.ScheduleOnce(TimeSpan.FromSeconds(3), () =>
            {
                ConsoleColor tmp = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("[DragonKilerFromForest] whispers: ress me plix!!!11");
                Console.ForegroundColor = tmp;
            });
        }
    }
}