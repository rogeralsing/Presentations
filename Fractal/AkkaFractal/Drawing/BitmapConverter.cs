using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaFractal.Drawing
{
    public static class BitmapConverter
    {
        public static byte[] ToByteArray(this Bitmap imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static Bitmap ToBitmap(this byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                var returnImage = (Bitmap)Image.FromStream(ms);
                return returnImage;
            }
        }
    }
}
