using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class CopyFilesModule<TResult> : Module<TResult> where TResult : FilePathsResult
    {
        public CopyFilesModule(IStageBuilder builder) : base(builder)
        {
        }

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

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            if (!IsValid) throw new Exception("Invalid Module Setup");

            var destinationPaths = new List<string>();
            var files = (SourceFilePaths ?? Array.Empty<string>()).Select(x => new FileInfo(x));
            foreach (var file in files)
            {
                try
                {
                    CheckForCancellation();
                    if (!DestinationDirectory.Exists)
                        GetRetryPolicy().Execute(() => DestinationDirectory.Create());
                    var fileDestination = Path.Combine(DestinationDirectory.FullName, file.Name);

                    GetRetryPolicy().Execute(() =>
                    {
                        Log(LogLevels.Information, $"Copying file \"{file.FullName}\" to \"{fileDestination}\"");
                        file.CopyTo(fileDestination, Overwrite);
                        destinationPaths.Add(fileDestination);
                    });
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

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<IOException>()
                .WaitAndRetry(
                RetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (e, t, i, c) =>
                {
                    Log(LogLevels.Warning, e);
                    CheckForCancellation();
                    Log(LogLevels.Warning, $"{i} Retrying in {t.TotalSeconds} seconds");
                });
        }
    }
}
