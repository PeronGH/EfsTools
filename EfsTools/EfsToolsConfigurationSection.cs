using System;
using Microsoft.Extensions.Configuration;

namespace EfsTools
{
    internal class EfsToolsConfigurationSection
    {
        private readonly IConfigurationSection _configurationSection;

        public EfsToolsConfigurationSection(IConfigurationRoot configurationRoot)
        {
            _configurationSection = configurationRoot == null ? null : configurationRoot.GetSection("efstool");
        }

        public int Vid
        {
            get => Convert.ToInt32(GetValue("vid", "05C6"), 16);
            set => SetValue("vid", $"{value:X4}");
        }

        public int Pid
        {
            get
            {
                var val = GetValue("pid", "0");
                return val.ToLowerInvariant() == "auto" ? 0 : Convert.ToInt32(val, 16);
            }
            set => SetValue("pid", $"{value:X4}");
        }

        public string Password
        {
            get => GetValue("password", "00000000");
            set => SetValue("password", value);
        }

        public string Spc
        {
            get => GetValue("spc", "000000");
            set => SetValue("spc", value);
        }

        public bool HdlcSendControlChar
        {
            get => bool.Parse(GetValue("hdlcSendControlChar", "False"));
            set => SetValue("hdlcSendControlChar", $"{value}");
        }
        public bool IgnoreUnsupportedCommands
        {
            get => bool.Parse(GetValue("ignoreUnsupportedCommands", "False"));
            set => SetValue("ignoreUnsupportedCommands", $"{value}");
        }

        private string GetValue(string name, string defaultValue)
        {
            if (_configurationSection != null)
            {
                var val = _configurationSection.GetSection(name);
                if (val != null)
                {
                    return val.Value;
                }
            }

            return defaultValue;
        }

        private void SetValue(string name, string value)
        {
            if (_configurationSection != null)
            {
                var val = _configurationSection.GetSection(name);
                if (val != null)
                {
                    val.Value = value;
                }
            }
        }
    }
}