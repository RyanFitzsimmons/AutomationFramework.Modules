using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public static class IOAsyncExtensions
    {
        public static async Task CopyToAsync(this FileInfo file, string destination, bool overwrite, CancellationToken token)
        {
            FileMode mode = FileMode.CreateNew;
            if (overwrite) mode = FileMode.Create;
            using FileStream SourceStream = File.Open(file.FullName, mode);
            using FileStream DestinationStream = File.Create(destination);
            await SourceStream.CopyToAsync(DestinationStream, token);
        }
    }
}
