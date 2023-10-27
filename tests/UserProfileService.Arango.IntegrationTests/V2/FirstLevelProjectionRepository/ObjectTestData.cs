using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    public class ObjectTestData
    {
        public string Name { get; }
        public IList<object> Data { get; }
        public Type EntityType { get; }
        public string KeyPropertyName { get; }
        public TestType TestScope { get; }

        public ObjectTestData(Type testCase)
        {
            Name = testCase.Name;

            var annotation = testCase.GetCustomAttribute<SeedGeneralDataAttribute>();

            if (annotation == null)
            {
                return;
            }

            EntityType = annotation.EntityType;
            TestScope = annotation.TestScope;
            KeyPropertyName = annotation.KeyPropertyName;

            List<MemberInfo> memberInfo = testCase.GetNestedTypes()
                .SelectMany(
                    nestedType =>
                        nestedType.GetMembers(
                            BindingFlags.Static
                            | BindingFlags.Public
                            | BindingFlags.FlattenHierarchy))
                .Concat(
                    testCase.GetMembers(
                        BindingFlags.Static
                        | BindingFlags.Public
                        | BindingFlags.FlattenHierarchy))
                .Where(
                    m => (m.MemberType == MemberTypes.Field)
                        | (m.MemberType == MemberTypes.Property))
                .ToList();

            Data = new List<object>();

            foreach (MemberInfo info in memberInfo)
            {
                if (info is FieldInfo fInfo)
                {
                    ProcessField(fInfo, annotation);
                }
                else if (info is PropertyInfo pInfo)
                {
                    ProcessProperty(pInfo, annotation);
                }
            }
        }

        private void ProcessProperty(PropertyInfo info, SeedGeneralDataAttribute annotation)
        {
            if (typeof(IList<>).MakeGenericType(annotation.EntityType).IsAssignableFrom(info.PropertyType))
            {
                // static class properties
                object val = info.GetValue(null);

                if (!(val is IEnumerable rawList))
                {
                    return;
                }

                IEnumerator enumerator = rawList.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    Data.Add(enumerator.Current);
                }
            }

            if (info.PropertyType == annotation.EntityType)
            {
                Data.Add(info.GetRawConstantValue());
            }
        }

        private void ProcessField(FieldInfo info, SeedGeneralDataAttribute annotation)
        {
            if (typeof(IList<>).MakeGenericType(annotation.EntityType).IsAssignableFrom(info.FieldType))
            {
                if (!(info.GetRawConstantValue() is IEnumerable rawList))
                {
                    return;
                }

                IEnumerator enumerator = rawList.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    Data.Add(enumerator.Current);
                }
            }

            if (info.FieldType == annotation.EntityType)
            {
                Data.Add(info.GetRawConstantValue());
            }
        }
    }
}
