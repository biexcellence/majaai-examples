using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MajaUWP.Utilities
{
    public class AppSettingHandler
    {

        static Windows.Storage.ApplicationDataContainer localSettings =
            Windows.Storage.ApplicationData.Current.LocalSettings;
        static Windows.Storage.StorageFolder localFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;


        public static void SetAppSetting(string setting, object value)
        {
            localSettings.Values[setting] = value;
        }

        public static object GetAppSetting(string setting)
        {
            try
            {
                object value = localSettings.Values[setting];
                return value;
            }
            catch (Exception)
            {

                return null;
            }
            
        }

        public static void StoreCredentials(string username, string password)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential("MajaUwp", username, password));

        }

        public static void Logout()
        {
            var vault = new PasswordVault();
            try
            {
                var creds = vault.RetrieveAll();
                foreach (var item in creds)
                {
                    vault.Remove(item);
                }
                Utils.IsMajaLoggedIn = false;

            }
            catch (Exception)
            {

            
            }
        }
        public async static Task<PasswordCredential> GetCredentials()
        {
            
            var vault = new PasswordVault();
            try
            {
                var credentials = vault.FindAllByResource("MajaUwp");
                bool validCredentials = await IsCredentialValid(credentials[0]);
                if (validCredentials)
                {
                    Utils.IsMajaLoggedIn = true;
                    return credentials[0];
                }
                else
                {
                    vault.Remove(credentials[0]);
                    return null;
                }
                
            }
            catch (Exception)
            {

                return null;
            }
            
        }

        public async static Task<bool> IsCredentialValid(PasswordCredential cred)
        {
            SessionHandler sh = new SessionHandler();
            if (cred == null) return false;
            
            try
            {
                cred.RetrievePassword();
                await sh.OpenbiUserLogin(cred.UserName, cred.Password);

                return true;
            }
            catch (Exception)
            {
                return false;                
            }
        }

        public async static void SetUpLogin()
        {
            if (await IsCredentialValid(await GetCredentials()))
            {
                Utils.IsMajaLoggedIn = true;
            }
            else
            {
                Utils.IsMajaLoggedIn = false;
            }
        }
    }

    [Serializable]
    internal class NoPasswordSavedException : Exception
    {
        public NoPasswordSavedException()
        {
        }

        public NoPasswordSavedException(string message) : base(message)
        {
        }

        public NoPasswordSavedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoPasswordSavedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
