using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace XTI_Secrets
{
    public abstract class SecretCredentials
    {
        private readonly string key;
        private readonly IDataProtector dataProtector;

        protected SecretCredentials(string key, IDataProtector dataProtector)
        {
            this.key = key;
            this.dataProtector = dataProtector;
        }

        public Task Update(XtiCredentials value)
        {
            var serialized = JsonSerializer.Serialize(new XtiCredentialsRecord(value));
            var encrypted = new EncryptedValue(dataProtector, serialized).Value();
            return Persist(key, encrypted);
        }

        protected abstract Task Persist(string key, string encryptedText);

        public async Task<XtiCredentials> Value()
        {
            var encrypted = await Load(key);
            var serialized = new DecryptedValue(dataProtector, encrypted).Value();
            var deserialized = string.IsNullOrWhiteSpace(serialized)
                ? new XtiCredentialsRecord()
                : JsonSerializer.Deserialize<XtiCredentialsRecord>(serialized);
            return new XtiCredentials(deserialized.UserName, deserialized.Password);
        }

        protected abstract Task<string> Load(string key);

        private class XtiCredentialsRecord
        {
            public XtiCredentialsRecord()
            {
            }

            public XtiCredentialsRecord(XtiCredentials credentials)
            {
                UserName = credentials?.UserName ?? "";
                Password = credentials?.Password ?? "";
            }

            public string UserName { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
