using AutomationFramework.Modules.UnitTests.ApplicationProcessModule.Modules;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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

        private static IStageBuilder GetStageBuilder<TModule>() where TModule : IModule =>
            new StageBuilder<TModule>(null, RunInfo<int>.Empty, StagePath.Empty);

        private static DirectoryInfo GetProgramDirectory() =>
            new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        private static FileInfo GetTestConsoleApp() =>
            new FileInfo(Path.Combine(
            GetProgramDirectory().
            Parent.Parent.Parent.Parent
            .CreateSubdirectory("TestConsoleApp")
            .CreateSubdirectory("bin")
            .CreateSubdirectory("Debug")
            .CreateSubdirectory("net6.0").FullName,
            "TestConsoleApp.exe"));

        private static FileInfo GetTestDotNetFrameworkConsoleApp() =>
            new FileInfo(Path.Combine(
            GetProgramDirectory().
            Parent.Parent.Parent.Parent
            .CreateSubdirectory("TestDotNetFrameworkConsoleApp")
            .CreateSubdirectory("bin")
            .CreateSubdirectory("Debug").FullName,
            "TestDotNetFrameworkConsoleApp.exe"));

        private static FileInfo Get7Zip() =>
            new FileInfo(@"C:\Program Files\7-Zip\7z.exe");

        private static DirectoryInfo GetFilesToZipDirectory() =>
            GetProgramDirectory().CreateSubdirectory("ApplicationProcessModule").CreateSubdirectory("FilesToZip");

        [Fact]
        public async Task TestConsoleApp()
        {
            TestProcessModule module = new(GetStageBuilder<TestProcessModule>())
            {
                ApplicationPath = GetTestConsoleApp().FullName,
                Arguments = "10",
            };
            module.OnLog += Module_OnLog;
            await module.Run();
        }

        [Fact]
        public async Task TestDotNetFrameworkConsoleApp()
        {
            TestProcessModule module = new(GetStageBuilder<TestProcessModule>())
            {
                ApplicationPath = GetTestDotNetFrameworkConsoleApp().FullName,
                Arguments = "10",
            };
            module.OnLog += Module_OnLog;
            await module.Run();
        }

        [Fact]
        public async Task TestZipFiles()
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
            await module.Run();
            File.Move(outputFilePath, renamedZipPath);
        }

        private void Module_OnLog(IModule module, LogLevels level, object message) =>
            Log.Information(message.ToString());

        /// <summary>
        /// To keep track of the WaitForExitAsync bug. Will sometimes fail because it's very timing dependant.
        /// </summary>
        /// <returns>Task</returns>
        /*[Fact]
        public async Task WaitForExitAsync()
        {
            var logs = new List<string>();
            var psi = new ProcessStartInfo("cmd", "/C echo test")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            using var process = new Process
            {
                StartInfo = psi
            };
            process.OutputDataReceived += (sender, e) => { if (e.Data != null) logs.Add(e.Data); };
            process.Start();

            // Give time for the process (cmd) to terminate
            await Task.Delay(1000);

            process.BeginOutputReadLine();

            await process.WaitForExitAsync();
            Assert.Empty(logs); // The collection is empty, but it should contain 1 item

            process.WaitForExit();
            Assert.Equal(new[] { "test" }, logs); // ok because WaitForExit waits for redirected streams
        }*/
    }
}
