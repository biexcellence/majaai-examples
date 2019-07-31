using BiExcellence.OpenBi.Api;
using BiExcellence.OpenBi.Api.Commands;
using BiExcellence.OpenBi.Api.Commands.Users;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace MajaUWP.Utilities
{
    public class SessionHandler : IDisposable
    {
        public IOpenBiSession Session { get; private set; }
        private List<string> _packages = new List<string>();
        public IReadOnlyList<string> Packages => _packages;

        private static IOpenBiConfiguration _openBiConfiguration = new OpenBiConfiguration(Protocol.HTTPS, "maja.ai", 443, "Maja UWP");

        public IUser OpenBiUser { get; set; }

        public SessionHandler(IEnumerable<string> packages)
        {
            _packages.AddRange(packages);
        }

        /// <summary>
        /// Uses packages from database
        /// </summary>
        public SessionHandler()
        {
            var utilsPackages = Utils.MajaPackages;
            _packages.AddRange(utilsPackages);
        }

        private Task _currentUserLoginTask;

        public async Task<bool> LoginWithSavedCredential()
        {
            PasswordCredential credentials = await AppSettingHandler.GetCredentials();
            if (credentials != null)
            {
                credentials.RetrievePassword();
                await OpenbiUserLogin(credentials.UserName, credentials.Password);
                return true;
            }
            return false;
        
        }


        public async Task OpenbiUserLogin(string username = null, string password = null)
        {
            await Task.Yield();
            var oldsession = Session;
            var sess = new OpenBiSession(_openBiConfiguration);
            try
            {
                if (username != null && password != null)
                {
                    await sess.OpenBiLogin(username, password);
                    OpenBiUser = await sess.GetUserByUsername(username);
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
            var session = Session;
            Session = null;
            session?.Dispose();
            OpenBiUser = null;
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
            catch (TaskCanceledException) {
                throw new OperationCanceledException();
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
            session?.Dispose();
            OpenBiUser = null;
        }
    }
}
