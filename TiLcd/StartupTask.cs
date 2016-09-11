using Windows.ApplicationModel.Background;

namespace TiLcdTest
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var lcd = new TiLcd(18, 23, 4, 25, 17, 27, 22, 5, 6, 13, 19, 26);

            lcd.Init();
            
            // Do stuff

            while (true) ;
        }
    }
}
