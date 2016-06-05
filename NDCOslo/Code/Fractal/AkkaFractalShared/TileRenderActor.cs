using System;
using System.Drawing;
using Akka.Actor;

namespace AkkaFractalShared
{
    public class TileRenderActor : ReceiveActor
    {
        public TileRenderActor()
        {
            Receive<RenderTile>(render =>
            {
                //Console.WriteLine("{0} rendering {1},{2}", Self, render.X, render.Y);
                var res = MandelbrotSet(render.X, render.Y, render.Width, render.Height, 4000, 4000, 0.5, -2.5, 1.5,
                    -1.5);
                //        Console.WriteLine("{0} rendered {1},{2}",Self,render.X,render.Y);
                Sender.Tell(new RenderedTile(res.ToByteArray(), render.X, render.Y));
            });
        }

        private static Bitmap MandelbrotSet(int xp, int yp, int w, int h, int width, int height, double maxr,
            double minr, double maxi, double mini)
        {
            var img = new Bitmap(w, h);
            var xjump = (maxr - minr)/Convert.ToDouble(width);
            var yjump = (maxi - mini)/Convert.ToDouble(height);
            var loopmax = 1000;
            for (var x = xp; x < xp + w; x++)
            {
                var cx = xjump*x - Math.Abs(minr);
                for (var y = yp; y < yp + h; y++)
                {
                    double zx = 0;
                    double zy = 0;
                    var cy = yjump*y - Math.Abs(mini);
                    var loopgo = 0;
                    while (zx*zx + zy*zy <= 4 && loopgo < loopmax)
                    {
                        loopgo++;
                        var tempzx = zx;
                        zx = zx*zx - zy*zy + cx;
                        zy = 2*tempzx*zy + cy;
                    }
                    img.SetPixel(x - xp, y - yp,
                        loopgo != loopmax ? Color.FromArgb(loopgo%32*7, loopgo%128*2, loopgo%16*14) : Color.Black);
                }
            }
            return img;
        }
    }
}