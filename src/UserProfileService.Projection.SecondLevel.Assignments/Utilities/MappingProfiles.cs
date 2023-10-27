using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using ResolvedModels = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace UserProfileService.Projection.SecondLevel.Assignments.Utilities;

internal class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ResolvedModels.Role, SecondLevelAssignmentContainer>();

        CreateMap<ResolvedModels.Role, ISecondLevelAssignmentContainer>()
            .ConstructUsing((source, context) => context.Mapper.Map<SecondLevelAssignmentContainer>(source));

        CreateMap<ResolvedModels.Group, SecondLevelAssignmentContainer>();

        CreateMap<ResolvedModels.Group, ISecondLevelAssignmentContainer>()
            .ConstructUsing((source, context) => context.Mapper.Map<SecondLevelAssignmentContainer>(source));

        CreateMap<ResolvedModels.Organization, SecondLevelAssignmentContainer>();

        CreateMap<ResolvedModels.Organization, ISecondLevelAssignmentContainer>()
            .ConstructUsing((source, context) => context.Mapper.Map<SecondLevelAssignmentContainer>(source));

        CreateMap<ResolvedModels.Function, SecondLevelAssignmentFunction>()
            .ForMember(
                f => f.Name,
                expression => expression.MapFrom(f => f.GenerateFunctionName()));

        CreateMap<ResolvedModels.Function, ISecondLevelAssignmentContainer>()
            .ConstructUsing((source, context) => context.Mapper.Map<SecondLevelAssignmentFunction>(source));

        CreateMap<RangeCondition, Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition>()
            .ReverseMap();

        CreateMap<ResolvedModels.IContainer, ObjectIdent>()
            .ForMember(o => o.Type, expression => expression.MapFrom(c => c.ContainerType))
            .ReverseMap();
    }
}
