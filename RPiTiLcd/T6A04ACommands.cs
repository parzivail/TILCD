using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiTiLcd
{
    public sealed class T6A04ACommands
    {
        byte WordLengthSet = 0,
        DisplayStateSet = 2,
        CounterModeSet = 4,
        OpAmpControl2Set = 8,
        OpAmpControl1Set = 16,
        YAddressSet = 32,
        ZAddressSet = 64,
        XAddressSet = 128,
        ContrastSet = 192;
    }

    [Flags]
    public enum DisplayState
    {
        Off = 0,
        On = 1
    }

    [Flags]
    public enum WordLength
    {
        SixBits = 0,
        EightBits = 1
    }

    [Flags]
    public enum CounterMode
    {
        XDown = 0,
        XUp = 1,
        YDown = 2,
        YUp = 3
    }
}
