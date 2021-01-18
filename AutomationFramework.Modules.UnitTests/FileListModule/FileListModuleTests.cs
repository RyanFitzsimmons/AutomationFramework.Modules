using Serilog;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.Modules.UnitTests.FileListModule
{
    public class FileListModuleTests
    {
        public FileListModuleTests(ITestOutputHelper output)
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
            GetProgramDirectory().CreateSubdirectory("FileListModule");

        private static DirectoryInfo GetFilesToListDirectory() =>
            GetModuleTestDirectory().CreateSubdirectory("FilesToList");

        private static DirectoryInfo GetDestinationDirectory() =>
            GetModuleTestDirectory().CreateSubdirectory("Destination");

        private static string FilePath => Path.Combine(GetDestinationDirectory().FullName, "FileList.txt");

        [Fact]
        public async Task TestOverwrite()
        {
            GetDestinationDirectory().Delete(true); // Clean directory
            FilePathResult result = null;
            FileListModule<FilePathResult> module = new(GetStageBuilder<FileListModule<FilePathResult>>())
            {
                FilePaths = GetFilesToListDirectory().GetFiles("*", SearchOption.AllDirectories).Select(x => x.FullName).ToArray(),
                FilePath = FilePath,
                IncludeDirectoryPath = true,
                Overwrite = true,
            };
            module.OnLog += Module_OnLog;
            module.OnResult += (m, r) => result = r;
            await module.Run();
            await module.Run(); // Runs a second time to test overwrite

            Assert.Equal(FilePath, result.FilePath);
            Assert.True(File.Exists(FilePath));
            string[] lines = await File.ReadAllLinesAsync(FilePath, Encoding.Default);
            Assert.Equal(4, lines.Length);
        }

        private void Module_OnLog(IModule module, LogLevels level, object message) =>
            Log.Information(message.ToString());
    }
}
