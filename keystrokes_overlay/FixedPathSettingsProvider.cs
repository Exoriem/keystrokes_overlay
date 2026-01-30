using System;
using System.Configuration;
using System.IO;

namespace keystrokes_overlay
{
    public class FixedPathSettingsProvider : LocalFileSettingsProvider
    {
        private static string ConfigDir =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KeystrokesOverlay"
            );

        private static string ConfigPath =>
            Path.Combine(ConfigDir, "user.config");

        public override SettingsPropertyValueCollection GetPropertyValues(
            SettingsContext context,
            SettingsPropertyCollection properties)
        {
            Directory.CreateDirectory(ConfigDir);

            context["UserConfigPath"] = ConfigDir;
            context["UserConfigFilename"] = "user.config";

            return base.GetPropertyValues(context, properties);
        }

        public override void SetPropertyValues(
            SettingsContext context,
            SettingsPropertyValueCollection values)
        {
            Directory.CreateDirectory(ConfigDir);

            context["UserConfigPath"] = ConfigDir;
            context["UserConfigFilename"] = "user.config";

            base.SetPropertyValues(context, values);
        }
    }
}
