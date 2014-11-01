using System;
using Akka.Actor;

namespace Shared
{
    public class SomeActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            Console.WriteLine("{0} got {1}", Self.Path.ToStringWithAddress(), message);
        }
    }
}