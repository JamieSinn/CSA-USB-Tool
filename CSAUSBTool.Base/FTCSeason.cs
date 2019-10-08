using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAUSBTool.Base
{
    public class FTCSeason : FIRSTSeason
    {
        public FTCSeason(int year) : base(year, FIRSTProgram.FTC)
        {
        }
    }
}
