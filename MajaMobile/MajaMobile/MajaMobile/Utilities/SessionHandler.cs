using BiExcellence.OpenBi.Api;
using BiExcellence.OpenBi.Api.Commands;
using BiExcellence.OpenBi.Api.Commands.Organisations;
using BiExcellence.OpenBi.Api.Commands.Users;
using MajaMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MajaMobile.Utilities
{
    public class SessionHandler : IDisposable
    {
        public IOpenBiSession Session { get; private set; }
        private List<string> _packages = new List<string>();
        public IReadOnlyList<string> Packages => _packages;

        private static IOpenBiConfiguration _openBiConfiguration = new OpenBiConfiguration(Protocol.HTTPS, "maja.ai", 443, "MajaApp");

        public event EventHandler<UserChangedEventArgs> UserChanged;
        private IUser _openBiUser;
        public IUser OpenBiUser
        {
            get => _openBiUser;
            set
            {
                _openBiUser = value;
                if (value == null)
                {
                    Organisation = null;
                }
                UserChanged?.Invoke(this, new UserChangedEventArgs(value));
            }
        }

        public IOrganisation Organisation { get; private set; }

        public SessionHandler(IEnumerable<string> packages)
        {
            _packages.AddRange(packages);
        }

        /// <summary>
        /// Uses packages from database
        /// </summary>
        public SessionHandler()
        {
            using (var db = new AppDatabase())
            {
                var ids = db.GetTalentIds();
                _packages.AddRange(ids);
                if (_packages.Count == 0)
                    _packages.AddRange(Utils.DefaultPackages);
            }
        }

        public void SaveTalentSelection(MajaTalent talent)
        {
            if (!talent.Selected)
            {
                _packages.Remove(talent.Id);
                using (var db = new AppDatabase())
                {
                    db.DeleteMajaTalentData(talent);
                }
            }
            else if (!_packages.Contains(talent.Id))
            {
                _packages.Add(talent.Id);
                using (var db = new AppDatabase())
                {
                    db.InsertMajaTalentData(talent);
                }
            }

        }

        private Task _currentUserLoginTask;
        private const string _accountStoreServiceId = "MajaAiAccount";

        public async Task OpenbiUserLogin(string username = null, string password = null)
        {
            await Task.Yield();
            AccountUser account = null;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                account = new AccountUser();
                account.Username = username;
                account.Password = password;
                await SecureStorage.SetAsync(_accountStoreServiceId, JsonSerializer.Serialize(account));
            }
            else
            {
                var json = await SecureStorage.GetAsync(_accountStoreServiceId);
                if (json != null)
                {
                    account = JsonSerializer.Deserialize<AccountUser>(json);
                }
            }

            var oldsession = Session;
            var sess = new OpenBiSession(_openBiConfiguration);

            try
            {
                if (account != null)
                {
                    await sess.OpenBiLogin(account.Username, account.Password);
                    var user = await sess.GetUserByUsername(account.Username);
                    Organisation = (await sess.GetOrganisationsByUser(user.Username)).FirstOrDefault((o) =>
                    {
                        if (o.CustomAttributes.TryGetValue("DocumentRole", out var roleId) && !string.IsNullOrEmpty(roleId.ToString()))
                            return true;
                        return false;
                    });
                    OpenBiUser = user;
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
            if (oldsession != null)
                await oldsession.DisposeAsync();
            Session = sess;
        }

        public void Logout()
        {
            SecureStorage.Remove(_accountStoreServiceId);

            var session = Session;
            Session = null;
            session?.DisposeAsync();
            OpenBiUser = null;

            using (var db = new AppDatabase())
            {
                _packages.Clear();
                _packages.AddRange(db.DeletePrivateTalentData());
            }
            if (_packages.Count == 0)
                _packages.AddRange(Utils.DefaultPackages);
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
                            await session.DisposeAsync();
                        }
                    }
                    else if (e is HttpRequestException)
                    {
                        // HttpRequestException has only a message which concatenates StatusCode and ReasonPhrase
                        if (string.Equals(e.Message, "500 (Illegal Handle: Handle Not created from your IP)", StringComparison.OrdinalIgnoreCase))
                        {
                            handled = true;
                            Session = null;
                            await session.DisposeAsync();
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

        public void Dispose()
        {
            var session = Session;
            Session = null;
            session?.DisposeAsync();
            OpenBiUser = null;
        }
    }
    public class UserChangedEventArgs : EventArgs
    {
        public IUser User { get; }
        public UserChangedEventArgs(IUser user)
        {
            User = user;
        }
    }
}