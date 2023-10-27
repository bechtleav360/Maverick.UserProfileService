using System;
using System.Collections.Generic;
using UserProfileService.Common.Tests.Utilities.TestModels;
using UserProfileService.Projection.Common.Extensions;
using Xunit;

namespace UserProfileService.Projection.Common.Tests
{
    public class TypeExtensionTests
    {
        [Fact]
        public void Get_instance_types_non_generic_interface_type_shall_fail()
        {
            Assert.Throws<ArgumentException>(() => typeof(ITestInterface).GetInstanceTypesForDependencyInjection());
        }

        [Fact]
        public void Get_instance_types_null_as_interface_type_shall_fail()
        {
            Assert.Throws<ArgumentNullException>(() => ((Type)null).GetInstanceTypesForDependencyInjection());
        }

        [Fact]
        public void Get_instance_types_shall_work()
        {
            // act
            List<(Type instanceType, Type serviceType)> list =
                typeof(ITestInterface<>).GetInstanceTypesForDependencyInjection();

            // assert
            Assert.Single(list);

            Assert.Contains(
                list,
                tuple => tuple.instanceType == typeof(TestClass)
                    && tuple.serviceType == typeof(ITestInterface<object>));
        }

        [Fact]
        public void Get_instance_types_in_different_assemblies_shall_work()
        {
            // act
            List<(Type instanceType, Type serviceType)> list =
                typeof(IPayloadObject<>).GetInstanceTypesForDependencyInjection(GetType().Assembly);

            // assert
            Assert.Equal(2, list.Count);

            Assert.Contains(
                list,
                tuple => tuple.instanceType == typeof(AnotherClass)
                    && tuple.serviceType == typeof(IPayloadObject<Employee>));

            Assert.Contains(
                list,
                tuple => tuple.instanceType == typeof(CoolClass)
                    && tuple.serviceType == typeof(IPayloadObject<object>));
        }

        internal interface ITestInterface
        {
        }

        internal interface ITestInterface<TElement>
        {
        }

        internal class AnotherClass : IPayloadObject<Employee>
        {
            public Employee Payload { get; set; }
        }

        internal class CoolClass : IPayloadObject<object>
        {
            public object Payload { get; set; }
        }

        internal class TestClass : ITestInterface<object>
        {
        }
    }
}
