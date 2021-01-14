﻿using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutomationFramework.Modules
{
    public class MoveDirectoryModule<TResult> : Module<TResult> where TResult : FilePathsResult
    {
        public MoveDirectoryModule(IStageBuilder builder) 
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

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            if (!IsValid) throw new Exception("Invalid Module Setup");
            var destinationPaths = new List<string>();
            var files = SourceDirectory.GetFiles("*", (Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            foreach (var file in files)
            {
                try
                {
                    CheckForCancellation();
                    var relativePath = file.Directory.FullName.Remove(0, SourceDirectory.FullName.Length);
                    var destinationDirectory = DestinationDirectory.FullName + relativePath;
                    if (!Directory.Exists(destinationDirectory))
                        GetRetryPolicy().Execute(() => Directory.CreateDirectory(destinationDirectory));
                    var fileDestination = Path.Combine(destinationDirectory, file.Name);

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

            try
            {
                GetRetryPolicy().Execute(() =>
                {
                    Log(LogLevels.Information, $"Deleting directory \"{SourceDirectory.FullName}\"");
                    SourceDirectory.Delete(Recursive);
                });
            }
            catch
            {
                /// Logging the name of the directory which failed to delete. The exception details
                /// will be logged by the kernel
                Log(LogLevels.Error, $"Failed to delete directory \"{SourceDirectory.FullName}\"");
                throw;
            }

            result.FilePaths = destinationPaths.ToArray();
            return result;
        }

        private RetryPolicy GetRetryPolicy() =>
            Policy
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
