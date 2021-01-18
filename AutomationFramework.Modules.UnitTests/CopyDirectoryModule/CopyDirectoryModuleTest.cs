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

namespace AutomationFramework.Modules.UnitTests.CopyDirectoryModule
{
    public class CopyDirectoryModuleTest
    {
        public CopyDirectoryModuleTest(ITestOutputHelper output)
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
            GetProgramDirectory().CreateSubdirectory("CopyDirectoryModule");

        private static DirectoryInfo GetDirectoryToCopy() =>
            GetModuleTestDirectory().CreateSubdirectory("DirectoryToCopy");

        private static DirectoryInfo GetDestinationDirectory() =>
            GetModuleTestDirectory().CreateSubdirectory("Destination");

        [Fact]
        public async Task TestRecursiveOverwrite()
        {
            GetDestinationDirectory().Delete(true); // Clean directory
            CopyDirectoryModule<FilePathsResult> module = new(GetStageBuilder<CopyDirectoryModule<FilePathsResult>>())
            {
                SourceDirectoryPath = GetDirectoryToCopy().FullName,
                DestinationDirectoryPath = GetDestinationDirectory().FullName,
                Recursive = true,
                Overwrite = true,                
            };
            module.OnLog += Module_OnLog;
            await module.Run();
            await module.Run(); // Runs a second time to test overwrite

            Assert.True(File.Exists(Path.Combine(GetDestinationDirectory().FullName, "TestFile1.txt")));
            Assert.True(File.Exists(Path.Combine(GetDestinationDirectory().FullName, "TestFile2.txt")));
            Assert.True(File.Exists(Path.Combine(GetDestinationDirectory().FullName, "SubDirectory", "TestFile3.txt")));
            Assert.True(File.Exists(Path.Combine(GetDestinationDirectory().FullName, "SubDirectory", "SubSubDirectory", "TestFile4.txt")));
        }

        private void Module_OnLog(IModule module, LogLevels level, object message) =>
            Log.Information(message.ToString());
    }
}
