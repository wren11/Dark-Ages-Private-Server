using SPF;
using System.IO;

namespace Hades.Imaging.SPF
{
    public sealed class SpfFile
    {
        private SpfFileHeader _mHeader;
        private SpfPalette _mPalette;

        public SpfFrame this[int index]
        {
            get => Frames[index];
            set => Frames[index] = value;
        }

        public SpfFrame[] Frames { get; private set; }

        public string FileName { get; private set; }

        private void FrameHeadersFromReader(BinaryReader reader)
        {
            for (var index = 0; (long) index < (long) FrameCount; ++index)
            {
                var h = SpfFrameHeader.FromBinaryReaderBlock(reader);
                Frames[index] = new SpfFrame(h, ColorFormat, _mPalette);
            }
        }

        private void FrameDataFromReader(BinaryReader reader)
        {
            for (var index = 0; index < FrameCount; ++index)
            {
                var byteCount = (int) Frames[index].ByteCount;
                var rawBits = reader.ReadBytes(byteCount);
                Frames[index].Render(rawBits);
            }
        }

        public static SpfFile FromFile(string fileName)
        {
            if (!File.Exists(fileName))
                return null;
            var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
            var spfFile = new SpfFile
                {FileName = fileName, _mHeader = SpfFileHeader.FromBinaryReaderBlock(binaryReader)};
            if (spfFile.ColorFormat == 0U)
                spfFile._mPalette = SpfPalette.FromBinaryReaderBlock(binaryReader);
            spfFile.FrameCount = binaryReader.ReadUInt32();
            spfFile.Frames = new SpfFrame[spfFile.FrameCount];
            spfFile.FrameHeadersFromReader(binaryReader);
            spfFile.ByteTotal = binaryReader.ReadUInt32();
            spfFile.FrameDataFromReader(binaryReader);
            binaryReader.Close();
            return spfFile;
        }

        public uint Unknown1 => _mHeader.Unknown1;

        public uint Unknown2 => _mHeader.Unknown2;

        public uint ColorFormat => _mHeader.ColorFormat;

        public uint FrameCount { get; private set; }

        public uint ByteTotal { get; private set; }
    }
}
