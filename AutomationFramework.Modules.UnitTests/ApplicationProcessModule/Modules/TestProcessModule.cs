using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationFramework.Modules.UnitTests.ApplicationProcessModule.Modules
{
    public class TestProcessModule : ApplicationProcessModule<ApplicationProcessResult>
    {
        public TestProcessModule(IStageBuilder builder) : base(builder)
        {
        }
    }
}
