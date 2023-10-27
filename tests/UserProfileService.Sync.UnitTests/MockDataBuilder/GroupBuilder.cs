using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;

namespace UserProfileService.Sync.UnitTests.MockDataBuilder
{
    /// <summary>
    ///     A class builder for <see cref="Group" /> and <see cref="GroupBasic" /> objects.
    /// </summary>
    public class GroupBuilder : AbstractMockBuilder<GroupBuilder, Group>,
        IBasicBuilder<GroupBasic>,
        ISyncModelBuilder<GroupSync>
    {
        public GroupBuilder()
        {
            Mockedobject = new Group();
        }

        /// <inheritdoc />
        public override GroupBuilder GenerateSampleData()
        {
            Mockedobject = MockDataGenerator.GenerateGroupInstances().FirstOrDefault();

            return this;
        }

        /// <inheritdoc />
        public GroupBasic BuildBasic()
        {
            var mapper = new Mapper(new MapperConfiguration(cfg => { cfg.CreateMap<Group, GroupBasic>(); }));

            return mapper.Map<Group, GroupBasic>(Mockedobject);
        }

        /// <inheritdoc />
        public GroupSync BuildSyncModel()
        {
            var mapper = new Mapper(
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<KeyProperties, ExternalIdentifier>().ReverseMap();

                        cfg.CreateMap<Group, GroupSync>()
                            .ForMember(
                                t => t.ExternalIds,
                                t => t.MapFrom(
                                    (group, groupSync, i, context) =>
                                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(
                                            group
                                                .ExternalIds)));
                    }));

            return mapper.Map<Group, GroupSync>(Mockedobject);
        }

        /// <summary>
        ///     Add a <see cref="Member" /> to the group, that is being created.
        /// </summary>
        /// <param name="member">
        ///     <see cref="Member" />
        /// </param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithMember(Member member)
        {
            (Mockedobject.Members ??= new List<Member>()).Add(member);

            return this;
        }

        /// <summary>
        ///     Add a list of <see cref="Member" /> to the group, that is being created.
        /// </summary>
        /// <param name="members">
        ///     <see cref="Member" />
        /// </param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithMembers(IEnumerable<Member> members)
        {
            (Mockedobject.Members ??= new List<Member>()).Concat(members);

            return this;
        }

        /// <summary>
        ///     Add a parent group where the group is a member of..
        /// </summary>
        /// <param name="member">
        ///     <see cref="Member" />
        /// </param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithParent(Member group)
        {
            (Mockedobject.MemberOf ??= new List<Member>()).Add(group);

            return this;
        }

        /// <summary>
        ///     Add groups where the group is a member of.
        /// </summary>
        /// <param name="members">
        ///     <see cref="Member" />
        /// </param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithParents(IEnumerable<Member> groups)
        {
            (Mockedobject.MemberOf ??= new List<Member>()).Concat(groups);

            return this;
        }

        /// <summary>
        ///     Create a group with the specified id
        /// </summary>
        /// <param name="id">group Id</param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithId(string id)
        {
            Mockedobject.Id = id;

            return this;
        }

        /// <summary>
        ///     Create a group with the given name
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithName(string name)
        {
            Mockedobject.Name = name;

            return this;
        }

        /// <summary>
        ///     Create a group with the specified external id
        /// </summary>
        /// <param name="source">external id source</param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithExternalId(string source)
        {
            Mockedobject.ExternalIds = new List<ExternalIdentifier>
            {
                new ExternalIdentifier(Guid.NewGuid().ToString(), source)
            };

            return this;
        }

        /// <summary>
        ///     Create a group with the specified external id
        /// </summary>
        /// <param name="id">external id</param>
        /// <param name="source">external id source</param>
        /// <returns>
        ///     <see cref="GroupBuilder" />
        /// </returns>
        public GroupBuilder WithExternalId(string id, string source)
        {
            Mockedobject.ExternalIds = new List<ExternalIdentifier>
            {
                new ExternalIdentifier(id, source)
            };

            return this;
        }
    }
}
