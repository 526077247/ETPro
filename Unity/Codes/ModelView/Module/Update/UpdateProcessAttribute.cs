using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UpdateProcessAttribute : BaseAttribute
    {
        public int UpdateStep;
        public UpdateProcessAttribute(UpdateTaskStep updateStep)
        {
            this.UpdateStep = (int)updateStep;
        }
    }
}
