using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaFractal.Akka
{
    public class RenderedTile
    {
        public RenderedTile(Bitmap tile,int x,int y)
        {
            Tile = tile;
            X = x;
            Y = y;
        }

        public Bitmap Tile { get;private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
    }
}
