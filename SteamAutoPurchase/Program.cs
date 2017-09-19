using System;
using System.Threading.Tasks;
using SteamAutoPurchase.Utils;

namespace SteamAutoPurchase
{
    class Program
    {
        private static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Введите логин:");
            var username = Console.ReadLine();
            Console.WriteLine("Введите пароль:");
            var password = ConsoleUtils.InputPassword();

            Console.ForegroundColor = ConsoleColor.Gray;

            while (true)
            {
                var login = SteamUtils.LogIn(username, password);
                var steam = new SteamMarketplace(login);

                var startTime = DateTime.Now;
                await steam.LoadMarkplaceItemsAsync(startTime);
            }
        }
    }
}