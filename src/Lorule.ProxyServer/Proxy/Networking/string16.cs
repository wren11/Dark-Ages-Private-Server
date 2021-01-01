namespace Proxy.Networking
{
    public class string16
    {
        public string value;

        public string16()
        {
            value = "";
        }

        public string16(string value)
        {
            this.value = value;
        }

        public static implicit operator string16(string value)
        {
            return new string16(value);
        }

        public static implicit operator string(string16 value)
        {
            return value.value;
        }
    }
}
