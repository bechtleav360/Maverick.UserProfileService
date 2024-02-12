using System;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Extensions;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using Xunit;

namespace UserProfileService.Projection.Common.UnitTests
{
    public class StreamedEventHeaderExtensionTests
    {
        private static IMapper GetMapper(DateTimeOffset processedOn)
        {
            return new Mapper(
                new MapperConfiguration(
                    c =>
                    {
                        c.CreateMap<StreamedEventHeader, ProjectionState>()
                            .ForMember(
                                state => state.EventId,
                                config =>
                                    config.MapFrom(header => header.EventId.ToString()))
                            .ForMember(
                                state => state.EventName,
                                config =>
                                    config.MapFrom(header => header.EventType))
                            .ForMember(
                                state => state.EventNumberVersion,
                                config =>
                                    config.MapFrom(
                                        (header, state) => state.EventNumberVersion = header.EventNumberVersion))
                            .ForMember(
                                state => state.StreamName,
                                config =>
                                    config.MapFrom(header => header.EventStreamId))
                            .ForMember(
                                state => state.ProcessedOn,
                                config =>
                                    config.MapFrom((_, state) => state.ProcessedOn = processedOn));
                    }));
        }

        [Fact]
        public void Convert_null_header_to_projection_state_should_work()
        {
            // act
            var result = (null as StreamedEventHeader).ToProjectionState();

            // assert
            Assert.Null(result);
        }

        [Fact]
        public void Convert_empty_header_to_projection_state_should_work()
        {
            // arrange
            var input = new StreamedEventHeader();
            var referenceState = GetMapper(DateTimeOffset.UtcNow).Map<ProjectionState>(input);

            // act
            var result = input.ToProjectionState();

            // assert
            result.Should()
                .BeEquivalentTo(
                    referenceState,
                    c => c.Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
                        .WhenTypeIs<DateTimeOffset>());
        }

        [Fact]
        public void Convert_header_to_projection_state_should_work()
        {
            // arrange
            var input = new StreamedEventHeader
            {
                EventStreamId = "whatever",
                EventId = Guid.Parse("dd29705a-f10f-41cf-8d70-b88822cc2051"),
                Created = DateTime.Parse("2011-09-23T13:54:32.691+01:00"),
                EventNumberVersion = 9819213,
                EventType = "method tested",
                StreamId = "whatever_related"
            };

            var referenceState = GetMapper(DateTimeOffset.Parse("2030-11-21T17:31:46.871")).Map<ProjectionState>(input);

            // act
            var result = input.ToProjectionState(
                processedOn:
                DateTimeOffset.Parse("2030-11-21T17:31:46.871"));

            // assert
            result.Should().BeEquivalentTo(referenceState);
        }

        [Fact]
        public void Convert_header_to_projection_state_including_error_information_should_work()
        {
            // arrange
            var input = new StreamedEventHeader
            {
                EventStreamId = "whatever",
                EventId = Guid.Parse("dd29705a-f10f-41cf-8d70-b88822cc2051"),
                Created = DateTime.Parse("2011-09-23T13:54:32.691+01:00"),
                EventNumberVersion = 9819213,
                EventType = "method tested",
                StreamId = "whatever_related"
            };

            var exception = new FormatException("Cannot do that in this case.");
            var referenceState = GetMapper(DateTimeOffset.Parse("2030-11-21T17:31:46.871")).Map<ProjectionState>(input);
            referenceState.ErrorMessage = exception.Message;
            referenceState.ErrorOccurred = true;
            referenceState.StackTraceMessage = exception.ToString();

            // act
            var result = input.ToProjectionState(
                exception,
                DateTimeOffset.Parse("2030-11-21T17:31:46.871"));

            // assert
            result.Should().BeEquivalentTo(referenceState);
        }
    }
}
