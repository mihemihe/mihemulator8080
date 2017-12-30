using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

        public static Texture2D videoTexture;
        private const int X = 256;
        private const int Y = 224;
        private const int startVideoRAM = 0x2400; //First bye of video RAM //TODO was not 0x!! noob mistake..
        private const int lengthVideoRAM = 7168; // RAM size, starting 1: 0-7167


        public static void GenerateDisplay(GraphicsDevice device)
        {
            //Random rnd = new Random();//remove this
            
            //initialize a texture
             videoTexture = new Texture2D(device, X, Y);

            //the array holds the color for each pixel in the texture
            Color[] pixelsArray = new Color[X * Y];

            int pixel = 0;
            for (int byteVideo = 0; byteVideo < lengthVideoRAM; byteVideo++)
            {
                //BitArray eightPixels = new BitArray(new int[] { byteVideo });
                BitArray eightPixels = new BitArray(new byte[] { Memory.RAMMemory[byteVideo + startVideoRAM] });
                bool[] bits = new bool[8];
                eightPixels.CopyTo(bits, 0);
                for (int bit = 0; bit < 8; bit++)
                {
                    
                    //int randomValue = rnd.Next(1, 3);
                    //pixelsArray[pixel] = (randomValue == 1) ? Color.White : Color.Red;
                    pixelsArray[pixel] = (bits[bit] == true) ? Color.White : Color.Red;

                    pixel++;
                }
            }
            videoTexture.SetData(pixelsArray);

            
        }

    }
}