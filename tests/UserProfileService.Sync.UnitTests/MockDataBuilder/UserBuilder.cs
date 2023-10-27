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
    ///     A class builder for <see cref="User" /> and <see cref="UserBasic" /> objects.
    /// </summary>
    public class UserBuilder : AbstractMockBuilder<UserBuilder, User>,
        IBasicBuilder<UserBasic>,
        ISyncModelBuilder<UserSync>
    {
        /// <summary>
        ///     Default constructor to initialize a <see cref="UserBuilder" />
        /// </summary>
        public UserBuilder()
        {
            Mockedobject = new User();
        }

        /// <inheritdoc />
        public override UserBuilder GenerateSampleData()
        {
            Mockedobject = MockDataGenerator.GenerateUserInstances().FirstOrDefault();

            return this;
        }

        /// <inheritdoc />
        public UserBasic BuildBasic()
        {
            var mapper = new Mapper(new MapperConfiguration(cfg => { cfg.CreateMap<User, UserBasic>(); }));

            return mapper.Map<User, UserBasic>(Mockedobject);
        }

        /// <inheritdoc />
        public UserSync BuildSyncModel()
        {
            var mapper = new Mapper(
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<KeyProperties, ExternalIdentifier>().ReverseMap();

                        cfg.CreateMap<User, UserSync>()
                            .ForMember(
                                t => t.ExternalIds,
                                t => t.MapFrom(
                                    (group, groupSync, i, context) =>
                                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(
                                            group
                                                .ExternalIds)));
                    }));

            return mapper.Map<User, UserSync>(Mockedobject);
        }

        /// <summary>
        ///     set the id of the user, that is being built.
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>
        ///     <see cref="UserBuilder" />
        /// </returns>
        public UserBuilder WithId(string id)
        {
            Mockedobject.Id = id;

            return this;
        }

        /// <summary>
        ///     set the name of the user, that is being built.
        /// </summary>
        /// <param name="name">name of the user</param>
        /// <returns>
        ///     <see cref="UserBuilder" />
        /// </returns>
        public UserBuilder WithName(string name)
        {
            Mockedobject.Name = name;

            return this;
        }

        /// <summary>
        ///     set the firstname of the user, that is being built.
        /// </summary>
        /// <param name="firstName">firstname of the user</param>
        /// <returns>
        ///     <see cref="UserBuilder" />
        /// </returns>
        public UserBuilder WithFirstName(string firstName)
        {
            Mockedobject.FirstName = firstName;

            return this;
        }

        /// <summary>
        ///     set the lastname of the user, that is being built.
        /// </summary>
        /// <param name="lastName">firstname of the user</param>
        /// <returns>
        ///     <see cref="UserBuilder" />
        /// </returns>
        public UserBuilder WithLastName(string lastName)
        {
            Mockedobject.LastName = lastName;

            return this;
        }

        /// <summary>
        ///     Create a user with an assignment <see cref="User.MemberOf" />
        /// </summary>
        /// <param name="member">
        ///     <see cref="Member" />
        /// </param>
        /// <returns>
        ///     <see cref="UserBuilder" />
        /// </returns>
        public UserBuilder WithMemberAssignment(Member member)
        {
            (Mockedobject.MemberOf ??= new List<Member>()).Add(member);

            return this;
        }

        /// <summary>
        ///     Create a user with a collection of  assignments <see cref="User.MemberOf" />
        /// </summary>
        /// <param name="members">A collection of <see cref="Member" /></param>
        /// <returns>
        ///     <see cref="UserBuilder" />
        /// </returns>
        public UserBuilder WithMemberAssignments(IEnumerable<Member> members)
        {
            (Mockedobject.MemberOf ??= new List<Member>()).Concat(members);

            return this;
        }
    }
}
