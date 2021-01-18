using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class CopyFilesModule<TResult> : Module<TResult> where TResult : FilePathsResult
    {
        public CopyFilesModule(IStageBuilder builder) 
            : base(builder) { }

        public string[] SourceFilePaths { get; init; }
        public string DestinationDirectoryPath { get; init; }
        public bool Overwrite { get; init; }
        /// <summary>
        /// If an IO Exception occurs, this is the number of times
        /// it will retry before throwing the exception. 
        /// Default = 5
        /// </summary>
        public int RetryAttempts { get; init; } = 5;
        private DirectoryInfo DestinationDirectory => new DirectoryInfo(DestinationDirectoryPath);

        private bool IsValid
        {
            get
            {
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
            var files = (SourceFilePaths ?? Array.Empty<string>()).Select(x => new FileInfo(x));
            foreach (var file in files)
            {
                try
                {
                    if (!DestinationDirectory.Exists)
                        DestinationDirectory.Create();
                    var fileDestination = Path.Combine(DestinationDirectory.FullName, file.Name);

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
