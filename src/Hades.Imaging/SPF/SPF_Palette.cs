using System;
using System.Drawing;
using System.IO;

namespace Hades.Imaging.SPF
{
    public struct SpfPalette
    {
        public byte[] Alpha;
        public byte[] Rgb;
        public Color[] Colors;

        public static SpfPalette FromBinaryReaderBlock(BinaryReader br)
        {
            SpfPalette spfPalette;
            spfPalette.Alpha = br.ReadBytes(512);
            spfPalette.Rgb = br.ReadBytes(512);
            spfPalette.Colors = new Color[256];
            for (var index = 0; index < 256; ++index)
            {
                var uint16 = BitConverter.ToUInt16(spfPalette.Rgb, 2 * index);
                var blue = 8 * (uint16 % 32);
                var green = 8 * (uint16 / 32 % 32);
                var red = 8 * (uint16 / 32 / 32 % 32);
                spfPalette.Colors[index] = Color.FromArgb(red, green, blue);
            }

            return spfPalette;
        }
    }
}
