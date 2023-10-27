using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    /// <summary>
    ///     A class builder for <see cref="FunctionView" /> and <see cref="FunctionBasic" /> objects.
    /// </summary>
    public class FunctionBuilder : AbstractMockBuilder<FunctionBuilder, FunctionView>, IBasicBuilder<FunctionBasic>
    {
        public FunctionBuilder()
        {
            Mockedobject = new FunctionView();
        }

        /// <inheritdoc />
        public override FunctionBuilder GenerateSampleData()
        {
            Mockedobject = MockDataGenerator.GenerateFunctionViewInstances().FirstOrDefault();

            return this;
        }

        /// <summary>
        ///     Create a function with the given id
        /// </summary>
        /// <param name="id">function id</param>
        /// <returns
        /// <see cref="FunctionBuilder" />
        public FunctionBuilder WithId(string id)
        {
            Mockedobject.Id = id;

            return this;
        }

        /// <summary>
        ///     Create a function with the given name
        /// </summary>
        /// <param name="name">function name</param>
        /// <returns>
        ///     <see cref="FunctionBuilder" />
        /// </returns>
        public FunctionBuilder WithName(string name)
        {
            Mockedobject.Name = name;

            return this;
        }

        /// <summary>
        ///     Create a function with the specified role
        /// </summary>
        /// <param name="role">a role attached to the function</param>
        /// <returns>
        ///     <see cref="FunctionBuilder" />
        /// </returns>
        public FunctionBuilder WithRole(RoleBasic role)
        {
            Mockedobject.Role = role;

            return this;
        }

        /// <summary>
        ///     Create a function with the given tagFilter
        /// </summary>
        /// <param name="tagFilter">Tag filter</param>
        /// <returns>
        ///     <see cref="FunctionBuilder" />
        /// </returns>
        public FunctionBuilder WithTagFilter(Tag tagFilter)
        {
            // TODO: Problems because of model change
            //(mockedobject.TagFilters ??= new List<Tag>()).Add(tagFilter);
            return this;
        }

        /// <summary>
        ///     Create a function with a collection of tag filters
        /// </summary>
        /// <param name="tagFilters">collection of tag filters</param>
        /// <returns>
        ///     <see cref="FunctionBuilder" />
        /// </returns>
        public FunctionBuilder WithTagFilters(IEnumerable<Tag> tagFilters)
        {
            // TODO: Problems because of model change
            // mockedobject.TagFilters = (mockedobject.TagFilters ??= new List<Tag>()).Concat(tagFilters).ToList();
            return this;
        }

        /// <inheritdoc />
        public FunctionBasic BuildBasic()
        {
            var mapper = new Mapper(new MapperConfiguration(cfg => { cfg.CreateMap<FunctionView, FunctionBasic>(); }));

            return mapper.Map<FunctionView, FunctionBasic>(Mockedobject);
        }
    }
}
