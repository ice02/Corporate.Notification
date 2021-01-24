using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Corporate.Chat.Console.client.Helpers
{
    public static class DarkModeHelper
    {
        public static bool GetDarkMode()
        {
            var result = false;

            //HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize

            string personalizeRegLoc = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            // Open Registry Key in RegistryHive.LocalMachine
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(personalizeRegLoc, false);

            // Read Value from Registry Sub Key
            int lightTheme = (int)regKey.GetValue("AppsUseLightTheme");

            if (lightTheme == 1)
            {
                result = true;
            }

            return result;
        }
    }
}
