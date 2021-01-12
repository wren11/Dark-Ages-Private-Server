#region

using System;

#endregion

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
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Collection"))
                {
                    ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                    ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
                }

                return false;
            }

            return true;
        }
    }
}