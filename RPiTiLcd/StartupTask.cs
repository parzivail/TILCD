using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;

namespace RPiTiLcd
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var lcd = new TiLcd(40, 41, 42, 50, 30, 31, 32, 33, 34, 35, 36, 37);
        }
    }
}
