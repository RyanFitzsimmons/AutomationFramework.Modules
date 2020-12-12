using System;

namespace AutomationFramework.Modules.Attributes
{
    public class CmdArgumentIncludeAttribute : Attribute
    {
        public bool ForceQuotes { get; set; }
    }
}
