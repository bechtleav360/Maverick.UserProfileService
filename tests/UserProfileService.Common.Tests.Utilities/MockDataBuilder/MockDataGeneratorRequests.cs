using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    public static class MockDataGeneratorRequests
    {
        public static CreateUserRequest CreateUser()
        {
            return CreateUser(1).First();
        }

        public static ICollection<CreateUserRequest> CreateUser(int number)
        {
            return new Faker<CreateUserRequest>()
                .RuleFor(
                    u => u.FirstName,
                    faker => faker.Name.FirstName())
                .RuleFor(
                    u => u.LastName,
                    faker => faker.Name.LastName())
                .RuleFor(
                    u => u.Name,
                    (faker, u) => $"{u.LastName},{u.FirstName}")
                .RuleFor(
                    u => u.DisplayName,
                    (_, u) => u.Name)
                .RuleFor(
                    u => u.Email,
                    (faker, u) => $"{u.FirstName}.{u.LastName}@bechtle.com")
                .RuleFor(u => u.UserName, (faker, u) => $"{u.FirstName}.{u.LastName}")
                .RuleFor(
                    u => u.UserStatus,
                    faker => "normal")
                .RuleFor(
                    u => u.ExternalIds,
                    faker => faker.Make(
                        1,
                        () => new ExternalIdentifier(Guid.NewGuid().ToString(), faker.Company.CompanyName())))
                .Generate(number);
        }

        public static CreateGroupRequest CreateGroup(ICollection<ConditionAssignment> members = null)
        {
            return CreateGroup(1, members).First();
        }

        public static ICollection<CreateGroupRequest> CreateGroup(
            int number,
            ICollection<ConditionAssignment> members = null)
        {
            return new Faker<CreateGroupRequest>()
                .RuleFor(g => g.Name, faker => faker.Name.JobType() + Guid.NewGuid())
                .RuleFor(g => g.DisplayName, faker => faker.Name.JobTitle() + Guid.NewGuid())
                .RuleFor(
                    g => g.ExternalIds,
                    (faker, user)
                        => new List<ExternalIdentifier>
                        {
                            new ExternalIdentifier(Guid.NewGuid().ToString(), "test")
                        })
                .RuleFor(g => g.Members, faker => members ?? new List<ConditionAssignment>())
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool())
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 10))
                .Generate(number);
        }

        public static CreateOrganizationRequest CreateOrganization()
        {
            return CreateOrganization(1).First();
        }

        public static ICollection<CreateOrganizationRequest> CreateOrganization(int number)
        {
            return new Faker<CreateOrganizationRequest>()
                .RuleFor(g => g.Name, faker => faker.Name.JobType() + Guid.NewGuid())
                .RuleFor(g => g.DisplayName, faker => faker.Name.JobTitle() + Guid.NewGuid())
                .RuleFor(
                    g => g.ExternalIds,
                    (faker, user)
                        => new List<ExternalIdentifier>
                        {
                            new ExternalIdentifier(Guid.NewGuid().ToString(), "test")
                        })
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool())
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 10))
                .Generate(number);
        }

        public static CreateRoleRequest CreateRole()
        {
            return CreateRole(1).First();
        }

        public static ICollection<CreateRoleRequest> CreateRole(int number)
        {
            return new Faker<CreateRoleRequest>()
                .RuleFor(r => r.Name, faker => faker.Name.JobArea() + Guid.NewGuid())
                .RuleFor(r => r.IsSystem, faker => faker.Random.Bool(0.3F))
                .RuleFor(r => r.Description, faker => faker.Lorem.Slug(5))
                .RuleFor(r => r.Permissions, faker => faker.Lorem.Words(5).Select(t => t + Guid.NewGuid()).ToList())
                .RuleFor(
                    r => r.DeniedPermissions,
                    faker => faker.Lorem.Words(5).Select(t => t + Guid.NewGuid()).ToList())
                .Generate(number);
        }

        public static CreateFunctionRequest CreateFunction(string roleId, string organizationId)
        {
            return new Faker<CreateFunctionRequest>()
                .RuleFor(f => f.Name, faker => faker.Name.JobArea() + Guid.NewGuid())
                .RuleFor(f => f.RoleId, faker => roleId)
                .RuleFor(f => f.OrganizationId, faker => organizationId)
                .RuleFor(
                    g => g.ExternalIds,
                    (faker, user)
                        => new List<ExternalIdentifier>
                        {
                            new ExternalIdentifier(Guid.NewGuid().ToString(), "test")
                        })
                .Generate();
        }
    }
}
