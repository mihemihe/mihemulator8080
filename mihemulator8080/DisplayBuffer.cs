using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace mihemulator8080
{
    public static class DisplayBuffer
    {
        //RAM
        //$2000-$23ff:    work RAM. Stack starts from $2400 downwards
        //$2400-$3fff:    video RAM

        //Video: 256(x) *224(y) @ 60Hz, vertical monitor.Colours are simulated with a

        //plastic transparent overlay and a background picture.
        //Video hardware is very simple: 7168 bytes 1bpp bitmap (32 bytes per scanline).

        //VIDEO
        public static Texture2D videoTexture;
        private const int X = 256;
        private const int Y = 224;
        private const int startVideoRAM = 0x2400; //First bye of video RAM //TODO was not 0x!! noob mistake..
        private const int lengthVideoRAM = 7168; // RAM size, starting 1: 0-7167
        private static Color[] pixelsArray;
        private static int pixel;
        private static Color colorBG, colorPixel;

        //RAM
        public static Texture2D RAMtexture;
        private static Color[] pixelsArrayRAM;


        public static void Init(GraphicsDevice device, Color BG, Color cixel)
        {
            //VIDEO
            videoTexture = new Texture2D(device, X, Y);
            pixelsArray = new Color[X * Y];
            pixel = 0;
            colorBG = BG;
            colorPixel = cixel;

            //RAM
            RAMtexture = new Texture2D(device, 256, 2048);
            pixelsArrayRAM = new Color[256 *2048];



        }

        public static void ChangeColors(Color BG, Color cixel)
        {
            colorBG = BG;
            colorPixel = cixel;
        }

        //TODO create another method to set colors, instead os passing colors every frame

        public static void GenerateDisplay()
        {
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            pixel = 0;
            for (int byteVideo = 0; byteVideo < lengthVideoRAM; byteVideo++)
            {
                //BitArray eightPixels = new BitArray(new int[] { byteVideo });
                BitArray eightPixels = new BitArray(new byte[] { Memory.RAMMemory[byteVideo + startVideoRAM] });
                bool[] bits = new bool[8];
                eightPixels.CopyTo(bits, 0);
                for (int bit = 0; bit < 8; bit++)
                {

                        pixelsArray[pixel] = (bits[bit] == true) ? colorPixel : colorBG; //TODO remove this if, is just for debug


                    pixel++;
                }
                //Inlining the for to increase performance, maybe use aggresive inlining attribute?
                // Inlining tested, no gain. Rule #1 don't try to outsmart the compiler optimizations
            }
            videoTexture.SetData(pixelsArray);

            //Debug.WriteLine(watch.ElapsedTicks);
        }

        public static void GenerateRAMDisplay()
        {
            int pointerRAM = 0;
            foreach (byte octet in Memory.RAMMemory)
            {
                BitArray octetToPixels = new BitArray(new byte[] { octet });
                foreach (bool point in octetToPixels)
                {
                    pixelsArrayRAM[pointerRAM] = (point) ? Color.White : Color.Black;
                    pointerRAM++;
                }
            }

            RAMtexture.SetData(pixelsArrayRAM);
        }
    }
}