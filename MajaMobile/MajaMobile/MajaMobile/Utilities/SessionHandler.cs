using BiExcellence.OpenBi.Api;
using BiExcellence.OpenBi.Api.Commands;
using BiExcellence.OpenBi.Api.Commands.MajaAi;
using BiExcellence.OpenBi.Api.Commands.Users;
using MajaMobile.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;

namespace MajaMobile.Utilities
{
    public class SessionHandler
    {

        private static SessionHandler _instance;
        public static SessionHandler Instance => _instance ?? (_instance = new SessionHandler());
        public static IOpenBiSession Session { get; private set; }
        private static List<string> _packages = new List<string>();
        public static IReadOnlyList<string> Packages => _packages;

        private static IOpenBiConfiguration _openBiConfiguration = new OpenBiConfiguration(Protocol.HTTPS, "maja.ai", 443, "MajaApp");

        public const string UserChangedMessage = "USER_CHANGED";

        private IUser _openBiUser;
        public IUser OpenBiUser
        {
            get => _openBiUser;
            set
            {
                _openBiUser = value;
                MessagingCenter.Send(this, UserChangedMessage, value);
            }
        }

        private SessionHandler()
        {

            using (var db = new AppDatabase())
            {
                var ids = db.GetTalentIds();
                _packages.AddRange(ids);
                if (_packages.Count == 0)
                    _packages.AddRange(Utils.DefaultPackages);
            }
        }

        public static void SetPackages(IEnumerable<IMajaTalent> talents)
        {
            _packages.Clear();
            _packages.AddRange(talents.Select(t => t.Id));
            using (var db = new AppDatabase())
            {
                db.SetMajaTalentData(talents);
            }
        }

        private Task _currentUserLoginTask;
        private const string _accountStoreServiceId = "MajaAiAccount";

        public async Task OpenbiUserLogin(string username = null, string password = null)
        {
            await Task.Yield();
            Account account;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var accstore = DeleteAccountFromAccountStore();
                account = new Account();
                account.Username = username;
                account.SetPassword(password);
                accstore.Save(account, _accountStoreServiceId);
            }
            else
            {
                account = AccountStore.Create().FindAccountsForService(_accountStoreServiceId).FirstOrDefault();
            }

            var oldsession = Session;
            var sess = new OpenBiSession(_openBiConfiguration);

            try
            {
                if (account != null)
                {
                    await sess.OpenBiLogin(account.Username, account.GetPassword());
                    OpenBiUser = await sess.GetUserByUsername(account.Username);
                }
            }
            catch (OpenBiServerErrorException serverException) when (serverException.Response.Code == OpenBiResponseCodes.LoginFailed)
            {
                Logout();
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _currentUserLoginTask = null;
            }
            oldsession?.Dispose();
            Session = sess;
        }

        public void Logout()
        {
            DeleteAccountFromAccountStore();

            var session = Session;
            Session = null;
            session?.Dispose();
            OpenBiUser = null;

            using (var db = new AppDatabase())
            {
                _packages.Clear();
                _packages.AddRange(db.DeletePrivateTalentData());
            }
            if (_packages.Count == 0)
                _packages.AddRange(Utils.DefaultPackages);
        }

        private AccountStore DeleteAccountFromAccountStore()
        {
            var accountstore = AccountStore.Create();
            foreach (var acc in accountstore.FindAccountsForService(_accountStoreServiceId))
            {
                accountstore.Delete(acc, _accountStoreServiceId);
            }
            return accountstore;
        }

        private object _lock = new object();
        public async Task<T> ExecuteOpenbiCommand<T>(Func<IOpenBiSession, CancellationToken, Task<T>> fn, CancellationToken token = default(CancellationToken))
        {
            var session = Session;
            if (session != null)
            {
                try
                {
                    return await fn(session, token);
                }
                catch (Exception e)
                {
                    var handled = false;
                    if (e is OpenBiServerErrorException)
                    {
                        if (((OpenBiServerErrorException)e).Response.Code == OpenBiResponseCodes.IllegalHandle)
                        {
                            handled = true;
                            Session = null;
                            session.Dispose();
                        }
                    }
                    else if (e is HttpRequestException)
                    {
                        // HttpRequestException has only a message which concatenates StatusCode and ReasonPhrase
                        if (string.Equals(e.Message, "500 (Illegal Handle: Handle Not created from your IP)", StringComparison.OrdinalIgnoreCase))
                        {
                            handled = true;
                            Session = null;
                            session.Dispose();
                        }
                    }
                    if (!handled)
                        throw;
                }
            }

            try
            {
                var currentLoginTask = _currentUserLoginTask;
                if (currentLoginTask == null)
                {
                    lock (_lock)
                    {
                        currentLoginTask = _currentUserLoginTask;
                        if (currentLoginTask == null)
                            currentLoginTask = _currentUserLoginTask = OpenbiUserLogin();
                    }
                }
                await currentLoginTask;
                session = Session;
                var result = await fn(session, token);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task ExecuteOpenbiCommand(Func<IOpenBiSession, CancellationToken, Task> fn, CancellationToken token = default(CancellationToken))
        {
            return ExecuteOpenbiCommand(async (sess, t) =>
            {
                await fn(sess, t);
                return true;
            }, token);
        }

    }
}