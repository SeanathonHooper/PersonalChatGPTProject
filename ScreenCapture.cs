using Microsoft.CognitiveServices.Speech;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace SeanLibraries
{
    public static class ScreenCapture
    {
        private static int imageIndex = 0;
        private static Bitmap screenCapture;
        private static string fileName = "";

        public static void Initialize(int screenWidth, int screenHeigh)
        {
            screenCapture = new Bitmap(screenWidth, screenHeigh);
        }

        
        public static string TakeScreenshot()
        {
            using (var g = Graphics.FromImage(screenCapture))
            {
                g.CopyFromScreen(0,0,0,0, screenCapture.Size, CopyPixelOperation.SourceCopy);
            }
            string fileName = imageIndex.ToString() + ".jpg";
            screenCapture.Save(fileName, ImageFormat.Jpeg);

            imageIndex++;

            return fileName;
        }
    }
}
