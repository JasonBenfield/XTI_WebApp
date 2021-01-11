using NUnit.Framework;
using System.Text.Json;
using XTI_WebApp.TestFakes;
using XTI_WebAppClient;

namespace XTI_WebApp.Tests
{
    public sealed class NumericValueClientTest
    {
        [Test]
        public void ShouldDeserializeNumericValue()
        {
            var serialized = JsonSerializer.Serialize(EmployeeType.Values.Permanent);
            var deserialized = JsonSerializer.Deserialize<EmployeeTypeClient>(serialized);
            Assert.That(deserialized, Is.EqualTo(EmployeeTypeClient.Values.Permanent), "Should deserialize numeric value client");
        }
        public sealed class EmployeeTypeClient : NumericValue
        {
            public sealed class EmployeeTypesClient : NumericValues<EmployeeTypeClient>
            {
                internal EmployeeTypesClient()
                {
                    None = Add(new EmployeeTypeClient(0, "None"));
                    Temp = Add(new EmployeeTypeClient(10, "Temp"));
                    Permanent = Add(new EmployeeTypeClient(15, "Permanent"));
                }

                public EmployeeTypeClient None
                {
                    get;
                }

                public EmployeeTypeClient Temp
                {
                    get;
                }

                public EmployeeTypeClient Permanent
                {
                    get;
                }
            }

            public static readonly EmployeeTypesClient Values = new EmployeeTypesClient();

            public EmployeeTypeClient(int value, string displayText) : base(value, displayText)
            {
            }
        }
    }
}
