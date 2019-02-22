using Xamarin.Auth;

namespace MajaMobile.Extensions
{
    internal static class AccountExtensions
    {

        public static void SetPassword(this Account account, string service)
        {
            account.Properties["password"] = service;
        }

        public static string GetPassword(this Account account)
        {
            if (account.Properties.ContainsKey("password"))
                return account.Properties["password"];
            return null;
        }

    }
}