using System;

namespace AutomationFramework.Modules.Attributes
{
    public class CmdArgumentOrderAttribute : Attribute
    {
        public int OrderBy { get; set; }
    }
}
