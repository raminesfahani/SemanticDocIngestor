using SemanticDocIngestor.Extensions.Tools;
using Xunit.Abstractions;

namespace SemanticDocIngestor.Extensions.Tests
{
    public abstract class TestBase(ITestOutputHelper outputHelper)
    {
        protected static string EnvPath => ".env";
        protected ITestOutputHelper Output { get; } = outputHelper;

        protected static void LoadEnv()
        {
            DotEnv.Load(EnvPath);
        }
    }
}
