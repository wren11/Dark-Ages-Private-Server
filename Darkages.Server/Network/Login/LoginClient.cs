using Darkages.Network.ClientFormats;

namespace Darkages.Network.Login
{
    public class LoginClient : NetworkClient<LoginClient>
    {
        public ClientFormat02 CreateInfo { get; set; }
    }
}