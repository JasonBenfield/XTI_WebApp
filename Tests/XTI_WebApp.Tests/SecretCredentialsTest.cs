﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_Secrets;
using XTI_WebApp.Fakes;

namespace XTI_WebApp.Tests
{
    public class SecretCredentialsTest
    {
        [Test]
        public async Task ShouldStoreAndRetrieveCredentials()
        {
            var input = setup();
            var secretCredentials = input.Factory.Create("Test");
            var storedCredentials = new XtiCredentials("Someone", "Password");
            await secretCredentials.Update(storedCredentials);
            var retrievedCredentials = await secretCredentials.Value();
            Assert.That(retrievedCredentials, Is.EqualTo(storedCredentials), "Should store and retrieve credentials");
        }

        [Test]
        public async Task ShouldEncryptCredentials()
        {
            var input = setup();
            var secretCredentials = (FakeSecretCredentials)input.Factory.Create("Test");
            var storedCredentials = new XtiCredentials("Someone", "Password");
            await secretCredentials.Update(storedCredentials);
            Assert.That
            (
                secretCredentials.StoredText,
                Is.Not.EqualTo(JsonSerializer.Serialize(storedCredentials)),
                "Should encrypt credentials"
            );
        }

        private TestInput setup()
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            services.AddScoped<SecretCredentialsFactory>(sp =>
            {
                var dataProtector = sp.GetDataProtector(new[] { "XTI_Secrets" });
                return new FakeSecretCredentialsFactory(dataProtector);
            });
            var sp = services.BuildServiceProvider();
            return new TestInput(sp);
        }

        private sealed class TestInput
        {
            public TestInput(ServiceProvider sp)
            {
                Factory = (FakeSecretCredentialsFactory)sp.GetService<SecretCredentialsFactory>();
            }

            public FakeSecretCredentialsFactory Factory { get; }
        }
    }
}
