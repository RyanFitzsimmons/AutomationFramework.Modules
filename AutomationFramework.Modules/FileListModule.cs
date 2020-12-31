﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.Modules
{
    public class FileListModule<TDataLayer, TResult> : Module<TDataLayer, TResult> 
        where TDataLayer : IModuleDataLayer
        where TResult : FileListModuleResult
    {
        public FileListModule(IRunInfo runInfo, StagePath stagePath, IMetaData metaData) : base(runInfo, stagePath, metaData)
        {
        }

        public override string Name { get; init; } = "File List";

        public FileInfo[] Files { get; set; }

        public bool IncludeDirectoryPath { get; set; }

        public string FileName { get; set; }

        public DirectoryInfo DestinationDirectory { get; set; }

        protected override TResult DoWork()
        {
            var result = Activator.CreateInstance<TResult>();
            var filePath = Path.Combine(DestinationDirectory.FullName, FileName);
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.Default))
            {
                foreach (var file in Files)
                {
                    if (IncludeDirectoryPath) sw.WriteLine(file.FullName);
                    else sw.WriteLine(file.Name);
                }
            }

            result.FilePath = filePath;
            return result;
        }
    }
}
