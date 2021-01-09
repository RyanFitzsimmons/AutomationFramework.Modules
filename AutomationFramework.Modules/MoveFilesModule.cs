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
    public class MoveFilesModule<TResult> : Module<TResult> where TResult : FilePathsResult
    {
        public MoveFilesModule(IStageBuilder builder) : base(builder)
        {
        }

        public string[] SourceFilePaths { get; init; }
        public string DestinationDirectoryPath { get; init; }
        public bool Overwrite { get; init; }
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
                    if (!DestinationDirectory.Exists)
                        GetRetryPolicy().Execute(() => DestinationDirectory.Create());
                    var fileDestination = Path.Combine(DestinationDirectory.FullName, file.Name);

                    GetRetryPolicy().Execute(() =>
                    {
                        Log(LogLevels.Information, $"Copying file \"{file.FullName}\" to \"{fileDestination}\"");
                        file.CopyTo(fileDestination, Overwrite);
                        destinationPaths.Add(fileDestination);
                    });

                    GetRetryPolicy().Execute(() =>
                    {
                        Log(LogLevels.Information, $"Deleting file \"{file.FullName}\"");
                        file.Delete();
                    });
                }
                catch
                {
                    /// Logging the name of the file which failed to copy. The exception details
                    /// will be logged by the kernel
                    Log(LogLevels.Error, $"Failed to move file \"{file.FullName}\"");
                    throw;
                }
            }

            result.FilePaths = destinationPaths.ToArray();
            return result;
        }

        private static RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<IOException>()
                .WaitAndRetry(25,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
