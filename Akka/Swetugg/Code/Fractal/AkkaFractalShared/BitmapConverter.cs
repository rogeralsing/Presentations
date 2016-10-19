using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AkkaFractalShared
{
    public static class BitmapConverter
    {
        public static byte[] ToByteArray(this Bitmap imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static Bitmap ToBitmap(this byte[] byteArrayIn)
        {
            using (var ms = new MemoryStream(byteArrayIn))
            {
                var returnImage = (Bitmap) Image.FromStream(ms);
                return returnImage;
            }
        }
    }
}