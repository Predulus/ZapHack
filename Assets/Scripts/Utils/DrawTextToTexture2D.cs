using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class DrawTextToTexture2D : MonoBehaviour
    {
        bool shouldOutputAsterisks = false; // Debugging visualisation in Debug.Log
        Color32[] characterPixels;
        public Texture characterSetTexture;
        //Texture characterSetTexture;
        public static int charWidth = 11;
        public static int charHeight = 17;
//        public static int charHeight = 2;
        public static int charPixelCount = 38;
//        public static int numCharsInSourceTexture = 2;
        public static int numCharsInSourceTexture = 38;
        public static int pixelsPerChar = 187;
//        public static int pixelsPerChar = 4;
        Texture2D texture2D;
        // Start is called before the first frame update
        void Start()
        {
            texture2D = (Texture2D)characterSetTexture;
            Color32[] color = texture2D.GetPixels32();
            Color32[] reversedColor = new Color32[texture2D.width * texture2D.height];
            characterPixels = new Color32[texture2D.width * texture2D.height];
            // Casting texture to Texture2D has done a weird reverse on the pixel rows, so correct that
            //            for (int row = 0; row < 646; row++)
            for (int row = 0; row < numCharsInSourceTexture * charHeight; row++)
            {
                //                System.Array.Copy(color, (645 - row) * 11, reversedColor, row * 11, 11);
                System.Array.Copy(color, (numCharsInSourceTexture * charHeight - row - 1) * charWidth, reversedColor, row * charWidth, charWidth);
            }
            characterPixels = reversedColor;
            if (shouldOutputAsterisks)
            {
                string outString;
                for (int chr = 0; chr < numCharsInSourceTexture; chr++)
                {
                    outString = string.Empty;
                    for (int row = 0; row < charHeight; row++)
                    {
                        int startIndex = pixelsPerChar * chr + (row * charWidth);
                        for (int col = 0; col < charWidth; col++)
                        {
                            Color pixelColor = characterPixels[startIndex + col];
                            if (pixelColor == Color.white) outString += "O";
                            if (pixelColor == Color.black) outString += "*";
                        }
                        outString += "\n";
                    }
                    Debug.Log(outString);
                }
            }
        }

        void Update()
        {

        }

        public Color32[] GetPixelsForString(string str)
        {
            int sourceIndex, destIndex;
            int stringPixelRowWidth = str.Length * charWidth;
            Color32[] pixels = new Color32[stringPixelRowWidth * charHeight];
            str = str.ToUpper();
            for (int i=0; i < str.Length; i++) // i is character index into string str
            {
                System.Char c = (System.Char)str[i];
                if ('A' <= c && c <='Z' )
                {
                    for (int row = 0; row < charHeight; row++) {
                        sourceIndex = pixelsPerChar * (c - 'A') + (row * charWidth);
                        destIndex = (row * stringPixelRowWidth + i*charWidth);
                        System.Array.Copy(characterPixels, sourceIndex, pixels, destIndex, charWidth);
                    }
                }
                if ('0' <= c && c <= '9')
                {
                    for (int row = 0; row < charHeight; row++)
                    {
                        sourceIndex = pixelsPerChar * (26 + c - '0') + (row * charWidth);
                        destIndex = (row * stringPixelRowWidth + i * charWidth);
                        System.Array.Copy(characterPixels, sourceIndex, pixels, destIndex, charWidth);
                    }
                }
                if (c == '.')
                {
                    for (int row = 0; row < charHeight; row++)
                    {
                        sourceIndex = pixelsPerChar * 36  + (row * charWidth);
                        destIndex = (row * stringPixelRowWidth + i * charWidth);
                        System.Array.Copy(characterPixels, sourceIndex, pixels, destIndex, charWidth);
                    }
                }
            }
            return pixels;
        }
    }
}