using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XTI_App;
using XTI_Secrets;

namespace XTI_UserApp
{
    public sealed class UserAppService : IHostedService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly AppFactory appFactory;
        private readonly IHashedPasswordFactory hashedPasswordFactory;
        private readonly SecretCredentialsFactory secretCredentialsFactory;
        private readonly Clock clock;
        private readonly UserOptions userOptions;

        public UserAppService
        (
            IHostApplicationLifetime lifetime,
            AppFactory appFactory,
            IHashedPasswordFactory hashedPasswordFactory,
            SecretCredentialsFactory secretCredentialsFactory,
            Clock clock,
            IOptions<UserOptions> userOptions
        )
        {
            this.lifetime = lifetime;
            this.appFactory = appFactory;
            this.hashedPasswordFactory = hashedPasswordFactory;
            this.secretCredentialsFactory = secretCredentialsFactory;
            this.clock = clock;
            this.userOptions = userOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(userOptions.AppKey)) { throw new ArgumentException("App key is required"); }
            if (string.IsNullOrWhiteSpace(userOptions.UserName)) { throw new ArgumentException("User name is required"); }
            if (string.IsNullOrWhiteSpace(userOptions.CredentialKey)) { throw new ArgumentException("Credential key is required"); }
            try
            {
                var app = await appFactory.Apps().App(new AppKey(userOptions.AppKey));
                var userName = new AppUserName(userOptions.UserName);
                var password = Guid.NewGuid().ToString("N") + "!?";
                var hashedPassword = hashedPasswordFactory.Create(password);
                var user = await appFactory.Users().User(userName);
                var roles = new List<AppRole>();
                if (!string.IsNullOrWhiteSpace(userOptions.RoleNames))
                {
                    foreach (var roleName in userOptions.RoleNames.Split(","))
                    {
                        if (!string.IsNullOrWhiteSpace(roleName))
                        {
                            var role = await app.Role(new AppRoleName(roleName));
                            if (role.Exists())
                            {
                                roles.Add(role);
                            }
                        }
                    }
                }
                if (user.Exists())
                {
                    await user.ChangePassword(hashedPassword);
                    var userRoles = (await user.RolesForApp(app)).ToArray();
                    foreach (var role in roles)
                    {
                        if (!userRoles.Any(ur => ur.IsRole(role)))
                        {
                            await user.AddRole(role);
                        }
                    }
                }
                else
                {
                    user = await appFactory.Users().Add(userName, hashedPassword, clock.Now());
                    foreach (var role in roles)
                    {
                        await user.AddRole(role);
                    }
                }
                var secretCredentials = secretCredentialsFactory.Create(userOptions.CredentialKey);
                await secretCredentials.Update
                (
                    new XtiCredentials(userName.Value, password)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.ExitCode = 999;
            }
            lifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
