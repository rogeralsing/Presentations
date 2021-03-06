﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaFractalShared
{
    public class DisplayTileActor : ReceiveActor
    {
        private Action<RenderedTile> _renderer;
        public DisplayTileActor(Action<RenderedTile> renderer)
        {
            _renderer = renderer;
            Receive<RenderedTile>(tile =>
            {
                _renderer(tile);
            });
        }
    }
}
