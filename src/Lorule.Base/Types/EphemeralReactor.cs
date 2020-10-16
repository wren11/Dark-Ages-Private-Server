#region

using System;
using Darkages.Network.Game;

#endregion

namespace Darkages
{
    public class EphemeralReactor
    {
        private GameServerTimer _timer;

        public EphemeralReactor(string lpKey, int lpTimeout)
        {
            YamlKey = lpKey;
            _timer = new GameServerTimer(TimeSpan.FromSeconds(lpTimeout));
        }

        public bool Expired { get; set; }
        public string YamlKey { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            _timer.Update(elapsedTime);

            if (_timer.Elapsed)
            {
                Expired = true;
                _timer = null;
            }
        }
    }
}