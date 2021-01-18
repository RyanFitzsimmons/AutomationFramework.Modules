using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.Modules.UnitTests.FindFilesModule
{
    public class FindFilesModuleTests
    {
        public FindFilesModuleTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        private static IStageBuilder GetStageBuilder<TModule>() where TModule : IModule =>
            new StageBuilder<TModule>(null, RunInfo<int>.Empty, StagePath.Empty);

        private static DirectoryInfo GetProgramDirectory() =>
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        private static DirectoryInfo GetModuleTestDirectory() =>
            GetProgramDirectory().CreateSubdirectory("FindFilesModule");

        private static DirectoryInfo GetFilesToFindDirectory() =>
            GetModuleTestDirectory().CreateSubdirectory("FilesToFind");

        [Fact]
        public async Task TestRecursive()
        {
            FilePathsResult result = null;
            FindFilesModule<FilePathsResult> module = new(GetStageBuilder<FindFilesModule<FilePathsResult>>())
            {
                SourceDirectoryPath = GetFilesToFindDirectory().FullName,
                SearchPattern = "*",
                Recursive = true,
            };
            module.OnLog += Module_OnLog;
            module.OnResult += (m, r) => result = r;
            await module.Run();

            Assert.Equal(4, result.FilePaths.Length);
        }

        private void Module_OnLog(IModule module, LogLevels level, object message) =>
            Log.Information(message.ToString());
    }
}
