using System.IO;
using System.Runtime.InteropServices;

namespace SPF
{
  public struct SpfFileHeader
  {
    public uint Unknown1;
    public uint Unknown2;
    public uint ColorFormat;

    public static SpfFileHeader FromBinaryReaderBlock(BinaryReader br)
    {
      var gcHandle = GCHandle.Alloc(br.ReadBytes(Marshal.SizeOf(typeof (SpfFileHeader))), GCHandleType.Pinned);
      var structure = (SpfFileHeader) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof (SpfFileHeader));
      gcHandle.Free();
      return structure;
    }
  }
}
