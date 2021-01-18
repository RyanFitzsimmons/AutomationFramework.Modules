using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class CopyDirectoryModule<TResult> : Module<TResult> where TResult : FilePathsResult
    {
        public CopyDirectoryModule(IStageBuilder builder) 
            : base(builder) { }

        public string SourceDirectoryPath { get; init; }
        public string DestinationDirectoryPath { get; init; }
        public bool Recursive { get; init; }
        public bool Overwrite { get; init; }
        /// <summary>
        /// If an IO Exception occurs, this is the number of times
        /// it will retry before throwing the exception. 
        /// Default = 5
        /// </summary>
        public int RetryAttempts { get; init; } = 5;
        private DirectoryInfo SourceDirectory => new DirectoryInfo(SourceDirectoryPath);
        private DirectoryInfo DestinationDirectory => new DirectoryInfo(DestinationDirectoryPath);

        private bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SourceDirectoryPath))
                {
                    Log(LogLevels.Error, "Source directory path is empty");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(DestinationDirectoryPath))
                {
                    Log(LogLevels.Error, "Destination directory path is empty");
                    return false;
                }

                return true;
            }
        }

        protected override async Task<TResult> DoWork(CancellationToken token)
        {
            var result = Activator.CreateInstance<TResult>();
            if (!IsValid) throw new Exception("Invalid Module Setup");
            var destinationPaths = new List<string>();
            var files = SourceDirectory.GetFiles("*", (Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            foreach (var file in files)
            {
                try
                {
                    var relativePath = file.Directory.FullName.Remove(0, SourceDirectory.FullName.Length);
                    var destinationDirectory = DestinationDirectory.FullName + relativePath;
                    if (!Directory.Exists(destinationDirectory))
                        Directory.CreateDirectory(destinationDirectory);
                    var fileDestination = Path.Combine(destinationDirectory, file.Name);
                    await CopyFile(file, fileDestination, Overwrite, token);
                    destinationPaths.Add(fileDestination);
                }
                catch
                {
                    /// Logging the name of the file which failed to copy. The exception details
                    /// will be logged by the kernel
                    Log(LogLevels.Error, $"Failed to copy file \"{file.FullName}\"");
                    throw;
                }
            }

            result.FilePaths = destinationPaths.ToArray();
            return result;
        }

        private async Task CopyFile(FileInfo file, string destinationPath, bool overwrite, CancellationToken token) =>
            await GetAsyncRetryPolicy().ExecuteAsync(async () =>
            {
                Log(LogLevels.Information, $"Copying file \"{file.FullName}\" to \"{destinationPath}\"");
                await file.CopyToAsync(destinationPath, overwrite, token);
            });

        private AsyncRetryPolicy GetAsyncRetryPolicy() =>
            Policy
            .Handle<IOException>()
            .WaitAndRetryAsync(
            RetryAttempts,
            (i, t) => GetRetryTimeSpan(i),
            (e, t, i, c) => OnTryFailure(e, t, i));

        private static TimeSpan GetRetryTimeSpan(int attempt) =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt));

        private void OnTryFailure(Exception exception, TimeSpan nextAttemptIn, int attempt)
        {
            Log(LogLevels.Warning, exception);
            Log(LogLevels.Warning, $"{attempt} Retrying in {nextAttemptIn.TotalSeconds} seconds");
        }
    }
}
