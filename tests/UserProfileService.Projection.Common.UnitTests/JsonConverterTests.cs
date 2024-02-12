using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UserProfileService.Projection.Common.Converters;
using UserProfileService.Projection.Common.UnitTests.Helpers;
using Xunit;

namespace UserProfileService.Projection.Common.UnitTests
{
    public class JsonConverterTests
    {
        private static void AssertEventsAreEquivalent(
            PropertiesChanged originalEvent,
            PropertiesChanged deserializedEvent,
            string[] wrongProperties)
        {
            Assert.NotNull(deserializedEvent);
            Assert.NotNull(deserializedEvent.Properties);

            deserializedEvent.Should()
                .BeEquivalentTo(
                    originalEvent,
                    options => options
                        .Excluding(e => e.Properties));

            // to be sure, the keys of the reference dictionary are compared case sensitive
            deserializedEvent.Properties
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value,
                    StringComparer.OrdinalIgnoreCase)
                .Should()
                .BeEquivalentTo(
                    originalEvent.Properties
                        .Where(
                            kv => wrongProperties == null
                                || !wrongProperties.Contains(kv.Key))
                        .ToDictionary(
                            kv => kv.Key,
                            kv => kv.Value,
                            StringComparer.OrdinalIgnoreCase));
        }

        private static PropertiesChanged CreateEvent(
            ObjectType objectType,
            Dictionary<string, object> properties)
        {
            return new PropertiesChanged
            {
                Id = "Entity-123-456-789",
                ObjectType = objectType,
                Properties = properties,
                EventId = "Event-1-2-3",
                MetaData =
                {
                    CorrelationId = "correlation#1",
                    VersionInformation = 1,
                    Timestamp = new DateTime(
                        2030,
                        10,
                        31,
                        17,
                        50,
                        30)
                }
            };
        }

        [Theory]
        [MemberData(
            nameof(ConverterTestArguments.PropertiesChangedEventShouldWork),
            MemberType = typeof(ConverterTestArguments))]
        public void Convert_properties_changed_event_should_work(
            ObjectType entityType,
            Dictionary<string, object> properties,
            string[] wrongProperties)
        {
            // arrange
            PropertiesChanged referenceEvent = CreateEvent(entityType, properties);
            string jsonText = JsonConvert.SerializeObject(referenceEvent);

            // act
            var deserializedEvent =
                JsonConvert.DeserializeObject<PropertiesChanged>(
                    jsonText,
                    new PropertiesChangedEventJsonConverter(),
                    new StringEnumConverter());

            // assert
            AssertEventsAreEquivalent(
                referenceEvent,
                deserializedEvent,
                wrongProperties);
        }

        [Theory]
        [MemberData(
            nameof(ConverterTestArguments.PropertiesChangedEventShouldFail),
            MemberType = typeof(ConverterTestArguments))]
        public void Convert_properties_changed_event_should_fail(
            ObjectType entityType,
            Dictionary<string, object> properties,
            Type exceptionType)
        {
            // arrange
            PropertiesChanged referenceEvent = CreateEvent(entityType, properties);
            string jsonText = JsonConvert.SerializeObject(referenceEvent);

            // act & assert
            Assert.Throws(
                exceptionType,
                () =>
                    JsonConvert.DeserializeObject<PropertiesChanged>(
                        jsonText,
                        new PropertiesChangedEventJsonConverter(),
                        new StringEnumConverter()));
        }

        [Fact]
        public void Convert_properties_changed_event_missing_object_type_should_fail()
        {
            // arrange
            PropertiesChanged referenceEvent = CreateEvent(ObjectType.Group, new Dictionary<string, object>());
            JObject jObj = JObject.FromObject(referenceEvent);
            jObj.Remove(nameof(PropertiesChanged.ObjectType));
            var jsonText = jObj.ToString(Formatting.Indented);

            // act & assert
            Assert.Throws<JsonException>(
                () =>
                    JsonConvert.DeserializeObject<PropertiesChanged>(
                        jsonText,
                        new PropertiesChangedEventJsonConverter(),
                        new StringEnumConverter()));
        }
    }
}
