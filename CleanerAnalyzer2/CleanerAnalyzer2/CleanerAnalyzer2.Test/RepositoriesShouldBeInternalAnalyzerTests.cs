using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = CleanerAnalyzer2.Test.Verifiers.CSharpCodeFixVerifier<
    CleanerAnalyzer2.RepositoriesShouldBeInternalAnalyzer,
    CleanerAnalyzer2.RepositoriesShouldBeInternalCodeFixProvider>;

namespace CleanerAnalyzer2.Test
{
    [TestClass]
    public class RepositoriesShouldBeInternalAnalyzerTests
    {
        // No diagnostic expected - already internal
        [TestMethod]
        public async Task InternalRepository_NoDiagnostic()
        {
            var test = @"
                namespace TestNamespace
                {
                    internal class UserRepository
                    {   
                    }
                }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        // Should ignore interfaces even if named Repository
        [TestMethod]
        public async Task RepositoryInterface_NoDiagnostic()
        {
            var test = @"
                namespace TestNamespace
                {
                    public interface IUserRepository
                    {   
                    }
                }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
} 