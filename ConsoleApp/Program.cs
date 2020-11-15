using System;
using System.Linq;

namespace ConsoleApp
{
    public class Program
    {
        public string A { get; set; }
        public int B { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Types in this assembly:");
            foreach (Type t in typeof(Program).Assembly.GetTypes())
            {
                Console.WriteLine(t.FullName);
            }

            var ps = CompiledReflection.GetPropertyNames<Program>();

            foreach (var p in ps)
            {
                Console.WriteLine(p);
            }

            ps = CompiledReflection.GetPropertyNames<TestClass>();

            foreach (var p in ps)
            {
                Console.WriteLine(p);
            }

            var pis = CompiledReflection.GetPropertyInfo<TestClass>();

            var tc = new TestClass { MyProperty = 42, MyProperty2 = "23" };

            foreach (var pi in pis)
            {
                Console.WriteLine($"{pi.TypeName} {pi.Name} {pi.GetValue(tc)}");
            }
        }
    }

    public class TestClass
    {
        public int MyProperty { get; set; }
        public string MyProperty2 { get; set; }
    }
}
