using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AkkaFractalShared;
namespace AkkaFractal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {            
            RenderFractal();                            
        }

        private void RenderFractal()
        {
            ActorSystem system = ActorSystem.Create("fractal");

            var w = 8000;
            var h = 8000;

            var img = new Bitmap(w, h);
            pictureBox1.Image = img;

            var split = 80;
            var ys = h / split;
            var xs = w / split;

            var g = Graphics.FromImage(img);
            Action<RenderedTile> renderer = tile =>
            {
                g.DrawImageUnscaled(tile.Bytes.ToBitmap(), tile.X, tile.Y);
                pictureBox1.Invalidate();
            };
            var displayTile = system.ActorOf(Props.Create(() => new DisplayTileActor(renderer)).WithDispatcher("akka.actor.synchronized-dispatcher"), "display-tile");
            var actor = system.ActorOf(Props.Create<TileRenderActor>().WithRouter(new RoundRobinPool(16)));

            for (int y = 0; y < split; y++)
            {
                var yy = ys * y;
                for (int x = 0; x < split; x++)
                {
                    var xx = xs * x;
                    g.DrawRectangle(Pens.Red, xx, yy, xs - 1, ys - 1);                    
                    actor.Tell(new RenderTile(yy, xx, xs, ys), displayTile);                    
                }
            }
        }
    }
}
