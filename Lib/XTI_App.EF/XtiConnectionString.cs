using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace XTI_App.EF
{
    public class XtiConnectionString
    {
        private readonly string value;

        public XtiConnectionString(IOptions<DbOptions> options, string envName, string dbName)
            : this(options.Value, envName, dbName)
        {
        }

        public XtiConnectionString(DbOptions options, string envName, string dbName)
        {
            var connStr = new Dictionary<string, string>();
            connStr.Add("Data Source", options.Source);
            if (string.IsNullOrWhiteSpace(options.UserName))
            {
                connStr.Add("Trusted_Connection", "True");
            }
            else
            {
                connStr.Add("User Id", options.UserName);
                connStr.Add("Password", options.Password);
            }
            connStr.Add("Initial Catalog", $"XTI_{envName}_{dbName}");
            value = string.Join(";", connStr.Keys.Select(key => $"{key}={connStr[key]}"));
        }

        public string Value() => value;

        public override string ToString() => $"{GetType().Name} {value}";
    }
}
