using System.Net;

namespace Proxy.Networking.ServerStructs
{
  public class Redirect
  {
    public byte Action
    {
      get => 3;
      set
      {
      }
    }

    public bool SaveOrdinal { get; set; }

    public IPEndPoint EndPoint { get; set; }

    public byte RemainingLength
    {
      get => (byte) (CryptoParams.PrivateKey.Length + Name.Length + 7);
      set
      {
      }
    }

    public CryptoParams CryptoParams { get; set; }

    public string8 Name { get; set; }

    public uint Id { get; set; }
  }
}
