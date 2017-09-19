using System;

namespace SteamAutoPurchase.Utils
{
    public static class ConsoleUtils
    {
        public static string InputPassword()
        {
            var password = string.Empty;

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter) break;

                Console.Write('*');

                password += key.KeyChar;
            }

            return password;
        }
    }
}