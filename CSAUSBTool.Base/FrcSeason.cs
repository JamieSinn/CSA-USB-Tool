using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CSAUSBTool.Base
{
    public class FRCSeason : FIRSTSeason
    {
        public FRCSeason(int year) : base(year, FIRSTProgram.FRC)
        {
        }
    }
}