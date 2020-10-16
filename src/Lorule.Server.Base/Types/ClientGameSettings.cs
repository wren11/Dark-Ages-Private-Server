namespace Darkages.Types
{
    public class ClientGameSettings
    {
        public string EnabledSettingStr, DisabledSettingStr;

        public ClientGameSettings(string lpEnabledStr, string lpDisabledStr, bool state = false)
        {
            EnabledSettingStr = lpEnabledStr;
            DisabledSettingStr = lpDisabledStr;
            Enabled = state;
        }

        public bool Enabled { get; set; }

        public void Toggle()
        {
            Enabled = !Enabled;
        }
    }
}