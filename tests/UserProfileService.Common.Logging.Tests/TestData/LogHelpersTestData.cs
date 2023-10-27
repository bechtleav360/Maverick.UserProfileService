using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json;
using UserProfileService.Common.Tests.Utilities;
using UserProfileService.Common.Tests.Utilities.TestModels;

namespace UserProfileService.Common.Logging.Tests.TestData
{
    public class LogHelpersTestData
    {
        public class LogStringTests : IEnumerable<object[]>
        {
            private static IEnumerable<object[]> GetData()
            {
                var cts = new CancellationTokenSource();
                cts.Cancel();

                IList<Employee> employees = SampleDataHelper.GetEmployees();

                yield return new object[] { null, string.Empty };
                yield return new object[] { CoolEnumeration.Crazy, nameof(CoolEnumeration.Crazy) };
                yield return new object[] { CoolEnumeration.Crazy | CoolEnumeration.Big, "Big, Crazy" };
                yield return new object[] { -0.9712, "-0.9712" };
                yield return new object[] { 12817, "12817" };
                yield return new object[] { 981.1290F, "981.129" };
                yield return new object[] { 9183621983L, "9183621983" };

                yield return new object[]
                {
                    new DateTimeOffset(
                        2022,
                        12,
                        24,
                        14,
                        53,
                        18,
                        TimeSpan.FromHours(1)),
                    "2022-12-24 13:53:18Z"
                };

                yield return new object[]
                {
                    new DateTime(
                        2038,
                        11,
                        30,
                        19,
                        31,
                        1,
                        DateTimeKind.Utc),
                    "2038-11-30 19:31:01Z"
                };

                yield return new object[] { true, "True" };
                yield return new object[] { "some string (#coolString)", "some string (#coolString)" };

                yield return new object[]
                {
                    cts.Token,
                    $"{nameof(CancellationToken)}.{nameof(CancellationToken.IsCancellationRequested)}: True"
                };

                yield return new object[] { new byte[350], "350 bytes" };

                yield return new object[]
                {
                    employees,
                    JsonConvert.SerializeObject(
                        employees,
                        Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            Culture = CultureInfo.InvariantCulture,
                            FloatFormatHandling = FloatFormatHandling.DefaultValue,
                            Converters =
                            {
                                new CompactStyleFloatingPointJsonConverter()
                            }
                        })
                };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                return GetData().GetEnumerator();
            }
        }

        public class AsArgumentListTests : IEnumerable<object[]>
        {
            private static readonly List<object[]> _Data = new List<object[]>
            {
                new object[] { null },
                new object[] { "test test" },
                new object[] { 123 },
                new object[] { true }
            };

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                return ((IEnumerable<object[]>)_Data).GetEnumerator();
            }
        }

        private class CompactStyleFloatingPointJsonConverter : JsonConverter<double>
        {
            public override void WriteJson(JsonWriter writer, double value, JsonSerializer serializer)
            {
                writer.WriteRawValue(value.ToString("G", CultureInfo.InvariantCulture));
            }

            public override double ReadJson(
                JsonReader reader,
                Type objectType,
                double existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }
    }
}
