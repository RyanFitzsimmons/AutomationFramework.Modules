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

namespace AutomationFramework.Modules.UnitTests.ReadLinesModule
{
    public class ReadLinesModuleTests
    {
        public ReadLinesModuleTests(ITestOutputHelper output)
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
            GetProgramDirectory().CreateSubdirectory("ReadLinesModule");

        private static FileInfo FileToRead => new FileInfo(Path.Combine(GetModuleTestDirectory().FullName, "FileToRead.txt"));

        [Fact]
        public async Task TestOverwrite()
        {
            ReadLinesResult result = null;
            ReadLinesModule<ReadLinesResult> module = new(GetStageBuilder<ReadLinesModule<ReadLinesResult>>())
            {
                FilePath = FileToRead.FullName,
            };
            module.OnLog += Module_OnLog;
            module.OnResult += (m, r) => result = r;
            await module.Run();
            await module.Run(); // Runs a second time to test overwrite

            Assert.Equal(5, result.Lines.Length);
        }

        private void Module_OnLog(IModule module, LogLevels level, object message) =>
            Log.Information(message.ToString());
    }
}
