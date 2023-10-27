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
    ///     A class builder for <see cref="RoleObject" /> and <see cref="RoleBasic" /> objects.
    /// </summary>
    public class RoleBuilder : AbstractMockBuilder<RoleBuilder, RoleView>,
        IBasicBuilder<RoleBasic>,
        ISyncModelBuilder<RoleSync>
    {
        /// <summary>
        ///     Default constructor of <see cref="RoleBuilder" />
        /// </summary>
        public RoleBuilder()
        {
            Mockedobject = new RoleView();
        }

        /// <inheritdoc />
        public RoleBasic BuildBasic()
        {
            var mapper = new Mapper(new MapperConfiguration(cfg => { cfg.CreateMap<RoleView, RoleBasic>(); }));

            return mapper.Map<RoleView, RoleBasic>(Mockedobject);
        }

        /// <inheritdoc />
        public RoleSync BuildSyncModel()
        {
            var mapper = new Mapper(
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<KeyProperties, ExternalIdentifier>().ReverseMap();

                        cfg.CreateMap<RoleView, RoleSync>()
                            .ForMember(
                                t => t.ExternalIds,
                                t => t.MapFrom(
                                    (group, groupSync, i, context) =>
                                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(
                                            group
                                                .ExternalIds)));
                    }));

            return mapper.Map<RoleView, RoleSync>(Mockedobject);
        }

        /// <inheritdoc />
        public override RoleBuilder GenerateSampleData()
        {
            Mockedobject = MockDataGenerator.GenerateRoleViewInstances().FirstOrDefault();

            return this;
        }

        /// <summary>
        ///     set the name of the role, that is being built
        /// </summary>
        /// <param name="name">Name of the role</param>
        /// <returns>
        ///     <see cref="RoleBuilder" />
        /// </returns>
        public RoleBuilder WithName(string name)
        {
            Mockedobject.Name = name;

            return this;
        }

        /// <summary>
        ///     set the Id of the role, that is being built
        /// </summary>
        /// <param name="id">Id of the role</param>
        /// <returns>
        ///     <see cref="RoleBuilder" />
        /// </returns>
        public RoleBuilder WithId(string id)
        {
            Mockedobject.Id = id;

            return this;
        }
        
        /// <summary>
        ///     Add permission to the role, that is being built
        /// </summary>
        /// <param name="permission">permission to the role</param>
        /// <returns>
        ///     <see cref="RoleBuilder" />
        /// </returns>
        public RoleBuilder WithPermission(string permission)
        {
            (Mockedobject.Permissions ??= new List<string>()).Add(permission);

            return this;
        }

        /// <summary>
        ///     Add permissions to the role, that is being built
        /// </summary>
        /// <param name="permissions">Permissions of the role</param>
        /// <returns>
        ///     <see cref="RoleBuilder" />
        /// </returns>
        public RoleBuilder WithPermissions(IEnumerable<string> permissions)
        {
            (Mockedobject.Permissions ??= new List<string>()).Concat(permissions);

            return this;
        }
    }
}
