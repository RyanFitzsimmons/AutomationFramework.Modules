using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AutomationFramework.Modules
{
    public class ReadLinesModule<TResult> : Module<TResult> where TResult : ReadLinesResult
    {
        public ReadLinesModule(IStageBuilder builder) 
            : base(builder) { }

        public string FilePath { get; init; }
        public Encoding Encoding { get; init; } = Encoding.Default;

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            result.Lines = File.ReadLines(FilePath, Encoding).ToArray();
            return result;
        }
    }
}
