using AutomationFramework.Modules.UnitTests.ApplicationProcessModule.Modules;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace AutomationFramework.Modules.UnitTests.ApplicationProcessModule
{
    public class ApplicationProcessModuleTests
    {
        public ApplicationProcessModuleTests(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Xunit(output)
                .CreateLogger();
        }

        private IStageBuilder GetStageBuilder<TModule>() where TModule : IModule =>
            new StageBuilder<TModule>(null, RunInfo<int>.Empty, StagePath.Empty);

        private DirectoryInfo GetProgramDirectory() =>
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        private FileInfo GetTestConsoleApp() =>
            new FileInfo(Path.Combine(
            GetProgramDirectory().
            Parent.Parent.Parent.Parent
            .CreateSubdirectory("TestConsoleApp")
            .CreateSubdirectory("bin")
            .CreateSubdirectory("Debug")
            .CreateSubdirectory("net5.0").FullName,
            "TestConsoleApp.exe"));

        private FileInfo Get7Zip() =>
            new FileInfo(@"C:\Program Files\7-Zip\7z.exe");

        private DirectoryInfo GetFilesToZipDirectory() =>
            GetProgramDirectory().CreateSubdirectory("ApplicationProcessModule").CreateSubdirectory("FilesToZip");

        [Fact]
        public void TestConsoleApp()
        {
            TestProcessModule module = new(GetStageBuilder<TestProcessModule>())
            {
                ApplicationPath = GetTestConsoleApp().FullName,
                Arguments = "10",
            };
            module.OnLog += Module_OnLog;
            module.Run();
        }

        [Fact]
        public void TestZipFiles()
        {
            var directoryPath = GetFilesToZipDirectory().FullName;
            var outputFilePath = $"{directoryPath}\\zipped.zip";
            var renamedZipPath = $"{directoryPath}\\zipRenamed.zip";
            File.Delete(renamedZipPath);
            TestProcessModule module = new(GetStageBuilder<TestProcessModule>())
            {
                ApplicationPath = Get7Zip().FullName,
                Arguments = $"a \"{outputFilePath}\" \"{directoryPath}\\*\"",
            };
            module.OnLog += Module_OnLog;
            module.Run();
            File.Move(outputFilePath, renamedZipPath);
        }

        private void Module_OnLog(IModule module, LogLevels level, object message) =>
            Log.Information(message.ToString());
    }
}
