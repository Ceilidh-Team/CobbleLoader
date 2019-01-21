using ProjectCeilidh.CobbleLoader.Attributes;
using ProjectCeilidh.CobbleLoader.Tests;

namespace ProjectCeilidh.CobbleLoader.TestPlugin
{
    [CobbleLoader]
    public class TestImplementation : ITestInterface
    {
        public string GetValue() => "test";
    }
}