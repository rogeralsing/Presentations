namespace AkkaFractalShared
{
    public class RenderedTile
    {
        public RenderedTile(byte[] bytes, int x, int y)
        {
            Bytes = bytes;
            X = x;
            Y = y;
        }

        public byte[] Bytes { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
    }
}