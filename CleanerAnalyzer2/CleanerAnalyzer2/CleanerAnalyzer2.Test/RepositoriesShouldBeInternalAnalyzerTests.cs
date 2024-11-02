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

        // Should flag public repository
        [TestMethod]
        public async Task PublicRepository_ShouldTriggerDiagnostic()
        {
            var test = @"
                namespace TestNamespace
                {
                    public class {|#0:UserRepository|}
                    {   
                    }
                }";

            var fixtest = @"
                namespace TestNamespace
                {
                    internal class UserRepository
                    {   
                    }
                }";

            var expected = VerifyCS.Diagnostic("RepositoriesShouldBeInternal")
                .WithLocation(0)
                .WithArguments("UserRepository");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        // Should flag private repository
        [TestMethod]
        public async Task PrivateRepository_ShouldTriggerDiagnostic()
        {
            var test = @"
                namespace TestNamespace
                {
                    private class {|#0:UserRepository|}
                    {   
                    }
                }";

            var fixtest = @"
                namespace TestNamespace
                {
                    internal class UserRepository
                    {   
                    }
                }";

            var expected = VerifyCS.Diagnostic("RepositoriesShouldBeInternal")
                .WithLocation(0)
                .WithArguments("UserRepository");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        // Should flag protected repository
        [TestMethod]
        public async Task ProtectedRepository_ShouldTriggerDiagnostic()
        {
            var test = @"
                namespace TestNamespace
                {
                    protected class {|#0:UserRepository|}
                    {   
                    }
                }";

            var fixtest = @"
                namespace TestNamespace
                {
                    internal class UserRepository
                    {   
                    }
                }";

            var expected = VerifyCS.Diagnostic("RepositoriesShouldBeInternal")
                .WithLocation(0)
                .WithArguments("UserRepository");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        // Should preserve other modifiers
        [TestMethod]
        public async Task PublicSealedRepository_ShouldPreserveSealed()
        {
            var test = @"
                namespace TestNamespace
                {
                    public sealed class {|#0:UserRepository|}
                    {   
                    }
                }";

            var fixtest = @"
                namespace TestNamespace
                {
                    internal sealed class UserRepository
                    {   
                    }
                }";

            var expected = VerifyCS.Diagnostic("RepositoriesShouldBeInternal")
                .WithLocation(0)
                .WithArguments("UserRepository");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        // Should fix multiple access modifiers
        [TestMethod]
        public async Task PrivateInternalRepository_ShouldFixToJustInternal()
        {
            var test = @"
                namespace TestNamespace
                {
                    private internal class {|#0:UserRepository|}
                    {   
                    }
                }";

            var fixtest = @"
                namespace TestNamespace
                {
                    internal class UserRepository
                    {   
                    }
                }";

            var expected = VerifyCS.Diagnostic("RepositoriesShouldBeInternal")
                .WithLocation(0)
                .WithArguments("UserRepository");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        // Should ignore non-repository classes
        [TestMethod]
        public async Task NonRepositoryClass_NoDiagnostic()
        {
            var test = @"
                namespace TestNamespace
                {
                    public class UserService
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