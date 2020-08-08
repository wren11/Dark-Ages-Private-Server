using System;

namespace Darkages.Network.Game
{
    public class Lorule
    {
        public static bool Update(Action operation)
        {
            if (operation == null)
                return false;

            try
            {
                operation.Invoke();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                ServerContext.Error(exception);
                return false;
            }

            return true;
        }
    }
}