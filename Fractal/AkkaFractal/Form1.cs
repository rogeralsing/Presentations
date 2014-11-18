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
using Akka;
using Akka.Configuration;
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
            var config = ConfigurationFactory.ParseString(@"
akka {  
    log-config-on-start = on
    stdout-loglevel = DEBUG
    loglevel = ERROR
    actor {
        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
        
        deployment {
            /render {
                router = round-robin-pool
                nr-of-instances = 16
                remote = ""akka.tcp://worker@127.0.0.1:8090""
            }
        }
    }
    remote {
        helios.tcp {
            transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
		    applied-adapters = []
		    transport-protocol = tcp
		    port = 0
		    hostname = localhost
        }
    }
}
");
            ActorSystem system = ActorSystem.Create("fractal",config);

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
            var actor = system.ActorOf(Props.Create<TileRenderActor>(),"render");

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
