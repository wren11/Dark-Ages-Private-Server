
namespace Proxy.Networking
{
  public class short8
  {
    private short value;

    public short8()
    {
      value = 0;
    }

    public short8(short value)
    {
      this.value = value;
    }

    public static implicit operator short8(short value)
    {
      return new short8(value);
    }

    public static implicit operator short(short8 value)
    {
      return value.value;
    }

    public void SetBytes(byte[] Value)
    {
      if (Value[1] >= 155)
        value = (short) (byte.MaxValue - Value[1]);
      else
        value = Value[1];
    }

    public byte[] GetBytes()
    {
      return value < (short) 0 ? new byte[2]
      {
        0,
        (byte) (byte.MaxValue - -1 * value)
      } : new byte[2]{ 0, (byte) value };
    }
  }
}
