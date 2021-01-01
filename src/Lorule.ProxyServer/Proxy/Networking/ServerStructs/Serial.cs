namespace Proxy.Networking.ServerStructs
{
  internal class Serial
  {
    public byte Action
    {
      get => 5;
      set
      {
      }
    }

    public byte Ordinal { get; set; }

    public uint Id { get; set; }
  }
}
