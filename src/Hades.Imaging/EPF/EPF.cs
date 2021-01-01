using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;
using Lorule.Content.Editor.Dat;


namespace Capricorn.Drawing
{
    /// <summary>
    /// EPF Image Class
    /// </summary>
    public class EPFImage
    {
        private int expectedFrames;
        private int width;
        private int height;
        private int unknown;
        private long tocAddress;
        private EPFFrame[] frames;

        /// <summary>
        /// Gets or sets a frame from this image.
        /// </summary>
        /// <param name="index">Zero-based index of the frame.</param>
        /// <returns>EPF frame.</returns>
        public EPFFrame this[int index]
        {
            get { return frames[index]; }
            set { frames[index] = value; }
        }	

        /// <summary>
        /// Image frames.
        /// </summary>
        public EPFFrame[] Frames
        {
            get { return frames; }
        }
	
        /// <summary>
        /// Gets the Table of Contents starting address.
        /// </summary>
        public long TOCAddress
        {
            get { return tocAddress; }
        }
	
        /// <summary>
        /// Gets the unknown variable value.
        /// </summary>
        public int Unknown
        {
            get { return unknown; }
        }
	
        /// <summary>
        /// Gets the height of the frames' containing canvas, in pixels.
        /// </summary>
        public int Height
        {
            get { return height; }
        }
	
        /// <summary>
        /// Gets the width of the frames' containing canvas, in pixels.
        /// </summary>
        public int Width
        {
            get { return width; }
        }
	
        /// <summary>
        /// Gets the number of frames expected in this image.
        /// </summary>
        public int ExpectedFrames
        {
            get { return expectedFrames; }
        }

        /// <summary>
        /// Loads an EPF image file from disk.
        /// </summary>
        /// <param name="file">EPF image to load.</param>
        /// <returns>EPF image.</returns>
        public static EPFImage FromFile(string file)
        {
            // Create File Stream
            FileStream stream = new FileStream(file,
                FileMode.Open, FileAccess.Read, FileShare.Read);

            // Load EPF from File Stream
            return LoadEPF(stream);
        }

        /// <summary>
        /// Loads an EPF image file from raw data bytes.
        /// </summary>
        /// <param name="data">Raw data to use.</param>
        /// <returns>EPF image.</returns>
        public static EPFImage FromRawData(byte[] data)
        {
            // Create Memory Stream
            MemoryStream stream = new MemoryStream(data);

            // Load HPF from Memory Stream
            return LoadEPF(stream);
        }


        /// <summary>
        /// Internal function that loads an EPF image from a given data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <returns></returns>
        private static EPFImage LoadEPF(Stream stream)
        {
            // Create Binary Reader
            stream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(stream);

            // Create EPF Image
            EPFImage epf = new EPFImage();

            #region Get Header Values
            epf.expectedFrames = reader.ReadUInt16();
            epf.width = reader.ReadUInt16();
            epf.height = reader.ReadUInt16();
            epf.unknown = reader.ReadUInt16();
            epf.tocAddress = reader.ReadUInt32() + 0x0C;
            #endregion

            // Create Frames
            if (epf.expectedFrames > 0)
                epf.frames = new EPFFrame[epf.expectedFrames];
            else
                return epf;

            #region Get Each Frame
            for (int i = 0; i < epf.expectedFrames; i++)
            {
                // Seek to Table of Contents
                reader.BaseStream.Seek(epf.tocAddress + i * 16, SeekOrigin.Begin);

                #region Get Frame Header
                int left = reader.ReadUInt16();
                int top = reader.ReadUInt16();
                int right = reader.ReadUInt16();
                int bottom = reader.ReadUInt16();

                int width = right - left;
                int height = bottom - top;

                uint startAddress = reader.ReadUInt32() + 0x0C;
                uint endAddress = reader.ReadUInt32() + 0x0C;
                #endregion

                #region Get Frame Data
                // Seek to Address
                reader.BaseStream.Seek(startAddress, SeekOrigin.Begin);

                byte[] frameBytes = null;
                if ((endAddress - startAddress) != (width * height))
                {
                    // Get Whole File as Single Frame
                    frameBytes = reader.ReadBytes((int)(epf.tocAddress - startAddress));
                }
                else
                {
                    // Get Frame Data
                    frameBytes = reader.ReadBytes((int)(endAddress - startAddress));
                }
                #endregion

                // Create Frame
                epf.frames[i] = new EPFFrame(left, top, width, height, frameBytes);
            }
            #endregion

            // Return EPF
            return epf;
        }

        /// <summary>
        /// Gets the string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{Frames = " + expectedFrames.ToString() + ", Width = " +
                width.ToString() + ", Height = " +
                height.ToString() + ", TOC Address = 0x" +
                tocAddress.ToString("X").PadLeft(8, '0') + "}";
        }
    }

    /// <summary>
    /// EPF Image Frame Class
    /// </summary>
    public class EPFFrame
    {
        private int left;
        private int top;
        private int width;
        private int height;
        private byte[] rawData;

        /// <summary>
        /// Gets whether the frame is renderable or not.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (rawData == null || rawData.Length < 1)
                    return false;
                else if (width < 1 || height < 1)
                    return false;
                else if (width * height != rawData.Length)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Gets the frame's raw data bytes.
        /// </summary>
        public byte[] RawData
        {
            get { return rawData; }
        }
	
        /// <summary>
        /// Gets the height of the image, in pixels.
        /// </summary>
        public int Height
        {
            get { return height; }
        }
	
        /// <summary>
        /// Gets the width of the image, in pixels.
        /// </summary>
        public int Width
        {
            get { return width; }
        }	

        /// <summary>
        /// Gets or sets the vertical location of the frame's origin.
        /// </summary>
        public int Top
        {
            get { return top; }
            set { top = value; }
        }
	
        /// <summary>
        /// Gets or sets the horizontal location of the frame's origin.
        /// </summary>
        public int Left
        {
            get { return left; }
            set { left = value; }
        }

        public enum ImageType
        {
            EPF,
            EFA,
            ZP,
            Tile
        }

        public unsafe Bitmap Render(int width, int height, byte[] data, Palette palette, ImageType type)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(data));
            var image = new Bitmap(width, height);
            var bmd = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.WriteOnly,
                image.PixelFormat);

            for (var y = 0; y < bmd.Height; y++)
            {
                var row = (byte*)bmd.Scan0 + (y * bmd.Stride);

                for (var x = 0; x < bmd.Width; x++)
                {
                    #region Get Value from Raw Data

                    int colorIndex;

                    if (type == ImageType.EPF)
                    {
                        colorIndex = data[x * height + y];
                    }
                    else
                    {
                        colorIndex = data[y * width + x];
                    }
                    #endregion

                    if (colorIndex > 0)
                    {
                        if (bmd.PixelFormat == PixelFormat.Format32bppArgb)
                        {
                            row[x * 4] = palette[colorIndex].B;
                            row[x * 4 + 1] = palette[colorIndex].G;
                            row[x * 4 + 2] = palette[colorIndex].R;
                            row[x * 4 + 3] = palette[colorIndex].A;
                        }
                        else if (bmd.PixelFormat == PixelFormat.Format24bppRgb)
                        {
                            row[x * 3] = palette[colorIndex].B;
                            row[x * 3 + 1] = palette[colorIndex].G;
                            row[x * 3 + 2] = palette[colorIndex].R;
                        }
                        else if (bmd.PixelFormat == PixelFormat.Format16bppRgb555)
                        {
                            var colorWord = (ushort)(((palette[colorIndex].R & 248) << 7) +
                                                     ((palette[colorIndex].G & 248) << 2) +
                                                     (palette[colorIndex].B >> 3));

                            row[x * 2] = (byte)(colorWord % 256);
                            row[x * 2 + 1] = (byte)(colorWord / 256);
                        }
                        else if (bmd.PixelFormat == PixelFormat.Format16bppRgb565)
                        {
                            var colorWord = (ushort)(((palette[colorIndex].R & 248) << 8)
                                                     + ((palette[colorIndex].G & 252) << 3) +
                                                     (palette[colorIndex].B >> 3));

                            row[x * 2] = (byte)(colorWord % 256);
                            row[x * 2 + 1] = (byte)(colorWord / 256);
                        }
                    }
                }
            }

            image.UnlockBits(bmd);

            if (type == ImageType.EPF)
            {
                image.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            return image;
        }

        /// <summary>
        /// Creates an EPF frame with the specified dimensions and data.
        /// </summary>
        /// <param name="left">X coordinate position of the frame's origin.</param>
        /// <param name="top">Y coordinate position of the frame's origin.</param>
        /// <param name="width">Width of the frame.</param>
        /// <param name="height">Height of the frame.</param>
        /// <param name="rawData">Raw frame data bytes.</param>
        public EPFFrame(int left, int top, int width, int height, byte[] rawData)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
            this.rawData = rawData;
        }

        /// <summary>
        /// Gets the string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{X = " + left.ToString() + ", Y = " + top.ToString() + ", Width = " +
                width.ToString() + ", Height = " + height.ToString() + "}";
        }
    }
}
