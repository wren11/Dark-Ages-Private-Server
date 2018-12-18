using System.Text;
using Darkages;

namespace Server
{
    class Server
    {
        public class Instance : ServerContext
        {
            public Instance()
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);                
                LoadConstants();
            }
        }
        
        public static Instance hInstance = new Instance();

        static void Main()
        {            
            hInstance.Start();
            System.Threading.Thread.CurrentThread.Join();
        }
    }
}
