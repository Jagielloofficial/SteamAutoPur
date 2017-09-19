using System;
using SteamAuth;

namespace SteamAutoPurchase.Utils
{
    public static class SteamUtils
    {
        public static UserLogin LogIn(string username, string password)
        {
            var login = new UserLogin(username, password);
            LoginResult response;
            while ((response = login.DoLogin()) != LoginResult.LoginOkay)
            {
                switch (response)
                {
                    case LoginResult.NeedEmail:
                        Console.WriteLine("Введите код, высланный на почту: ");
                        string code = Console.ReadLine();
                        login.EmailCode = code;
                        break;

                    case LoginResult.NeedCaptcha:
                        System.Diagnostics.Process.Start(APIEndpoints.COMMUNITY_BASE + "/public/captcha.php?gid=" + login.CaptchaGID);
                        Console.WriteLine("Введите капчу: ");
                        string captchaText = Console.ReadLine();
                        login.CaptchaText = captchaText;
                        break;

                    case LoginResult.Need2FA:
                        Console.WriteLine("Введите двухфакторный код: ");
                        code = Console.ReadLine();
                        login.TwoFactorCode = code;
                        break;
                }
            }
            return login;
        }
    }
}