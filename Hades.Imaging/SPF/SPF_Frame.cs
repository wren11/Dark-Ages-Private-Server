using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Hades.Imaging.SPF
{
    public sealed class SpfFrame
    {
        private readonly SpfFrameHeader _mHeader;

        public int PadWidth => _mHeader.PadWidth;

        public int PadHeight => _mHeader.PadHeight;

        public int PixelWidth => _mHeader.PixelWidth;

        public int PixelHeight => _mHeader.PixelHeight;

        public uint Unknown => _mHeader.Unknown;

        public uint Reserved => _mHeader.Reserved;

        public uint StartAddress => _mHeader.StartAddress;

        public uint ByteWidth => _mHeader.ByteWidth;

        public uint ByteCount => _mHeader.ByteCount;

        public uint SemiByteCount => _mHeader.SemiByteCount;

        public Bitmap FrameBitmap { get; }

        public SpfFrame(SpfFrameHeader h, uint format, SpfPalette p)
        {
            _mHeader = h;

            if (ByteCount == 0U)
                return;

            if (format == 0U)
            {
                FrameBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format8bppIndexed);
                var palette = FrameBitmap.Palette;
                Array.Copy(p.Colors, palette.Entries, 256);
                FrameBitmap.Palette = palette;
            }
            else
                FrameBitmap = new Bitmap(PixelWidth, PixelHeight, PixelFormat.Format16bppRgb555);
        }

        public void Render(byte[] rawBits)
        {
            if (ByteCount == 0U)
                return;
            if (FrameBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                Render8Bppi(rawBits);
            else
                Render32Bpp(rawBits);
        }

        private void Render8Bppi(byte[] rawBits)
        {
            var num1 = 1;
            if (FrameBitmap.PixelFormat == PixelFormat.Format16bppRgb555)
                num1 = 2;
            var bitmapdata = FrameBitmap.LockBits(new Rectangle(0, 0, PixelWidth, PixelHeight), ImageLockMode.ReadWrite,
                FrameBitmap.PixelFormat);
            var scan0 = bitmapdata.Scan0;
            var num2 = 4 - PixelWidth * num1 % 4;
            if (num2 < 4 || PadWidth > 0 || PadHeight > 0)
            {
                if (num2 == 4)
                    num2 = 0;
                var num3 = PixelWidth * num1 + num2;
                var source = new byte[PixelHeight * num3];
                for (var index = 0; index < PixelHeight - PadHeight; ++index)
                {
                    Array.Copy(rawBits, index * num1 * (PixelWidth - PadWidth), source, index * num3,
                        num1 * (PixelWidth - PadWidth));
                    Marshal.Copy(source, 0, scan0, PixelHeight * num3);
                }
            }
            else
                Marshal.Copy(rawBits, 0, scan0, PixelHeight * PixelWidth * num1);

            FrameBitmap.UnlockBits(bitmapdata);
        }

        private void Render32Bpp(byte[] rawBits)
        {
            var length = rawBits.Length / 2;
            var rawBits1 = new byte[length];
            Array.Copy(rawBits, length, rawBits1, 0, length);
            Render8Bppi(rawBits1);
        }
    }
}
