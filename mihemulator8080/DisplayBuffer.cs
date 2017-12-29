using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace mihemulator8080
{
    internal class DisplayBuffer
    {
        //RAM
        //$2000-$23ff:    work RAM
        //$2400-$3fff:    video RAM

        //Video: 256(x) *224(y) @ 60Hz, vertical monitor.Colours are simulated with a

        //plastic transparent overlay and a background picture.
        //Video hardware is very simple: 7168 bytes 1bpp bitmap (32 bytes per scanline).

        public static Texture2D videoTexture;
        private const int X = 256;
        private const int Y = 224;
        private const int startVideoRAM = 2400; //First bye of video RAM
        private const int lengthVideoRAM = 7168; // RAM size, starting 1: 0-7167


        public static Texture2D GenerateDisplay(GraphicsDevice device)
        {
            Random rnd = new Random();//remove this
            int startVideoRAM = 2400;
            //initialize a texture
            Texture2D texture = new Texture2D(device, X, Y);

            //the array holds the color for each pixel in the texture
            Color[] pixelsArray = new Color[X * Y];

            int pixel = 0;
            for (int byteVideo = 0; byteVideo < lengthVideoRAM; byteVideo++)
            {
                //BitArray eightPixels = new BitArray(new int[] { byteVideo });
                BitArray eightPixels = new BitArray(new byte[] { Memory.RAMMemory[byteVideo + 2400] });
                bool[] bits = new bool[8];
                eightPixels.CopyTo(bits, 0);
                for (int bit = 0; bit < 8; bit++)
                {
                    
                    int randomValue = rnd.Next(1, 3);
                    pixelsArray[pixel] = (randomValue == 1) ? Color.White : Color.Red;
                    //pixelsArray[pixel] = (bits[bit] == true) ? Color.White : Color.Red;

                    pixel++;
                }
            }
            texture.SetData(pixelsArray);

            return texture;
        }

    }
}