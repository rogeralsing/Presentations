using System;
using Akka.Actor;

namespace AkkaFractalShared
{
    public class DisplayTileActor : ReceiveActor
    {
        private readonly Action<RenderedTile> _renderer;

        public DisplayTileActor(Action<RenderedTile> renderer)
        {
            _renderer = renderer;
            Receive<RenderedTile>(tile => { _renderer(tile); });
        }
    }
}