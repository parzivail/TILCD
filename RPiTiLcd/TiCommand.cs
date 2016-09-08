using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiTiLcd
{
    internal class TiCommand
    {
        public static byte WordLengthSet = 0,
            DisplayStateSet = 2,
            CounterModeSet = 4,
            OpAmpControl2Set = 8,
            OpAmpControl1Set = 16,
            YAddressSet = 32,
            ZAddressSet = 64,
            XAddressSet = 128,
            ContrastSet = 192;
    }
}
