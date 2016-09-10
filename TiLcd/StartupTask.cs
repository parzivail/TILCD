using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace TiLcdTest
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var lcd = new TiLcd(18, 23, 4, 25, 17, 27, 22, 5, 6, 13, 19, 26);

            lcd.Init();

            float f = 0;
            while (true)
            {
                lcd.Clear();
                
                lcd.DrawPartialCircle(20, 32, 12, f, true);

                lcd.DrawPartialCircle(60, 32, 12, 1 - f, true);

                lcd.RefreshFromBuffer();

                f += 0.01f;

                if (f > 1)
                    f = 0;

                100.Delay();
            }
        }

        private static string GetTime()
        {
            return DateTime.Now.ToString("hh:mm:ss tt");
        }
    }
}
