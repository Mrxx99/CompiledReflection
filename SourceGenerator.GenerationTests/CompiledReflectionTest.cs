using System.Linq;
using System.Reflection;
using Xunit;

namespace SourceGenerator.GenerationTests
{
    public class CompiledReflectionTest
    {
        [Fact]
        public void GetPropertyNamesShouldReturnNameOfProperties()
        {
            var actualPropertyNames = CompiledReflection.GetPropertyNames<TestClass1>();

            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var expectedPropertyNames = typeof(TestClass1).GetProperties(bindingFlags).Select(p => p.Name);

            Assert.Equal(expectedPropertyNames, actualPropertyNames);
        }
    }
}
