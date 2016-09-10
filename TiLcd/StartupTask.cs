using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using LcdDriver;

namespace TiLcdTest
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var lcd = new TiLcd(18, 23, 4, 25, 17, 27, 22, 5, 6, 13, 19, 26);

            lcd.Init(48);

            lcd.BeginDraw(TiLcd.BeginMode.Fill);
            lcd.AddPoint(19, 29);
            lcd.AddPoint(49, 16);
            lcd.AddPoint(48, 5);
            lcd.AddPoint(24, 5);
            lcd.AddPoint(7, 13);
            lcd.EndDraw();

            lcd.RefreshFromBuffer();

            while (true) ; // Let the Pi actually do stuff
        }
    }
}
