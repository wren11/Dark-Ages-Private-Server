#region

using Darkages.Network.Object;

#endregion

namespace Darkages.Scripting
{
    public abstract class GameScript
    {
        private readonly IObjectManager _objectManager;
        private readonly IServerConstants _settings;

        protected GameScript(IObjectManager objectManager, IServerConstants settings)
        {
            _objectManager = objectManager;
            _settings = settings;
        }
    }
}