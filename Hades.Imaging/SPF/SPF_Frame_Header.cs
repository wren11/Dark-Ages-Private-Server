using System.IO;
using System.Runtime.InteropServices;

namespace Hades.Imaging.SPF
{
    public struct SpfFrameHeader
    {
        public ushort PadWidth;
        public ushort PadHeight;
        public ushort PixelWidth;
        public ushort PixelHeight;
        public uint Unknown;
        public uint Reserved;
        public uint StartAddress;
        public uint ByteWidth;
        public uint ByteCount;
        public uint SemiByteCount;

        public static SpfFrameHeader FromBinaryReaderBlock(BinaryReader br)
        {
            var gcHandle = GCHandle.Alloc(br.ReadBytes(Marshal.SizeOf(typeof(SpfFrameHeader))), GCHandleType.Pinned);
            var structure = (SpfFrameHeader) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(SpfFrameHeader));
            gcHandle.Free();
            return structure;
        }
    }
}
