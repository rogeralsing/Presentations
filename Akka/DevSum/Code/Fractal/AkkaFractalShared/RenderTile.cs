namespace AkkaFractalShared
{
    public class RenderTile
    {
        public RenderTile(int x, int y, int height, int width)
        {
            X = x;
            Y = y;
            Height = height;
            Width = width;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
    }
}