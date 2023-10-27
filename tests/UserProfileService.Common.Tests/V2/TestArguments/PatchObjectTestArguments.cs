using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UserProfileService.Common.Tests.Helpers;
using UserProfileService.Common.Tests.Utilities;
using UserProfileService.Common.Tests.Utilities.TestModels;

namespace UserProfileService.Common.Tests.V2.TestArguments
{
    public static class PatchObjectTestArguments
    {
        public static (Employee beforeUpdate, Employee afterUpdate, IReadOnlyDictionary<string, object> changeSet)
            ChangingNullableValueTypes()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().FirstOrDefault(e => e.BirthDate.HasValue),
                e => new Employee(e));

            // nullable checks
            builder
                .AddChange(
                    p => p.Quality,
                    48.91)
                .AddChange(
                    p => p.BirthDate,
                    newValue: null)
                .AddChange(
                    p => p.LikesJob,
                    true);

            return (builder.OriginalEntity, builder.ModifiedEntity, builder.ChangeSet);
        }

        public static (Employee beforeUpdate, Employee afterUpdate, IReadOnlyDictionary<string, object> changeSet)
            DifferentPropertyNameSpelling()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().ElementAt(1),
                e => new Employee(e));

            builder
                // check case-insensitive property name matching
                .AddChange(
                    "firstname",
                    "myCoolTest");

            return (builder.OriginalEntity, builder.ModifiedEntity, builder.ChangeSet);
        }

        public static (Employee beforeUpdate, Employee afterUpdate, IReadOnlyDictionary<string, object> changeSet)
            SetArrayToIListPropertyType()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().ElementAt(1),
                e => new Employee(e));

            builder
                // try to set an IList property with an array
                .AddChange(
                    e =>
                        e.Skills,
                    new[] { "GodMode" });

            return (builder.OriginalEntity, builder.ModifiedEntity, builder.ChangeSet);
        }

        public static (Employee beforeUpdate, IReadOnlyDictionary<string, object> changeSet)
            SetDoubleAsStringToDouble()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().FirstOrDefault(e => e.BirthDate.HasValue));

            builder
                .AddInvalidChange(
                    p => p.Priority,
                    "19.84");

            return (builder.OriginalEntity, builder.ChangeSet);
        }

        public static (Employee beforeUpdate, IReadOnlyDictionary<string, object> changeSet)
            SetNullToValueType()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().FirstOrDefault(e => e.BirthDate.HasValue));

            builder
                .AddInvalidChange(
                    p => p.StartedAt,
                    null);

            return (builder.OriginalEntity, builder.ChangeSet);
        }

        public static (Employee beforeUpdate, IReadOnlyDictionary<string, object> changeSet)
            SetStringToDouble()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().FirstOrDefault(e => e.BirthDate.HasValue));

            builder
                .AddInvalidChange(
                    p => p.Priority,
                    "whatever");

            return (builder.OriginalEntity, builder.ChangeSet);
        }

        public static (Employee beforeUpdate, Employee afterUpdate, IReadOnlyDictionary<string, object> changeSet)
            SimpleOverwritePropertyValues()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().First(),
                e => new Employee(e));

            builder
                .AddChange(
                    e => e.FirstName,
                    e => $"{e.FirstName}_test")
                .AddChange(
                    e =>
                        e.IsManager,
                    e => !e.IsManager)
                .AddChange(
                    e =>
                        e.StartedAt,
                    DateTime.Today)
                .AddChange(
                    e =>
                        e.Skills,
                    new List<string>
                    {
                        "Rust development"
                    })
                .AddChange(
                    e =>
                        e.Wallets,
                    new[] { "1PVsZB2SNL8MKTCqTTEmJtYcqjz3UyoHsX-1" })
                .AddChange(
                    e =>
                        e.PreferredCars,
                    new ReadOnlyCollection<Employee.Cars>(
                        new List<Employee.Cars>
                        {
                            new Employee.Cars
                            {
                                Make = "AVS",
                                Model = "Maverick UPS",
                                Year = 2021
                            }
                        }))
                // set double values
                .AddChange(
                    e =>
                        e.Priority,
                    19.84);

            return (builder.OriginalEntity, builder.ModifiedEntity, builder.ChangeSet);
        }

        public static (Employee beforeUpdate, Employee afterUpdate, IReadOnlyDictionary<string, object> changeSet)
            ValidAndInvalidParameterChangesTogether()
        {
            EntityModificationBuilder<Employee> builder = EntityModificationBuilder.Create(
                SampleDataHelper.GetEmployees().ElementAt(1),
                e => new Employee(e));

            builder
                // at least one valid property change should be contained
                .AddChange(
                    p => p.LastName,
                    "myCoolTest")
                // try to add non-existing property to a set of valid changes
                .AddChange("notExistingProperty", "Whatever");

            return (builder.OriginalEntity, builder.ModifiedEntity, builder.ChangeSet);
        }
    }
}
