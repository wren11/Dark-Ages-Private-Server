namespace Proxy.Networking
{
    public class string8
    {
        public string value;

        public string8()
        {
            value = "";
        }

        public string8(string value)
        {
            this.value = value;
        }

        public static implicit operator string8(string value)
        {
            return new string8(value);
        }

        public static implicit operator string(string8 value)
        {
            return value.value;
        }

        public int Length => value.Length;
    }
}