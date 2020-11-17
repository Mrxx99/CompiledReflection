namespace SourceGenerator.GenerationTests
{
    public class TestClass1
    {
        public int MyProperty { get; init; }
        public string MyProperty2 { get; set; }
        internal string MyProperty2b { get; set; }
        public TestClass1 MyProperty3 { get; }
        public TestClass2 MyProperty4 { get; private set; }
        public double MyProperty5 { private get; set; }
    }
}
