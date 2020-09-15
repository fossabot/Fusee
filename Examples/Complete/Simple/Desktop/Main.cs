using Fusee.Base.Core;
using System;
using System.Threading;

namespace Fusee.Examples.Simple.Desktop
{
    public class Simple
    {
        public static void Main()
        {
            Lib.ExecFusAppInNewThread();

            var isInit = Lib.IsAppInitialized();

            while(!isInit)
                isInit = Lib.IsAppInitialized();

            Thread.Sleep(2000);

            Lib.SetRenderPause(true);

            Thread.Sleep(2000);

            Lib.SetRenderPause(false);

            Thread.Sleep(2000);

            Lib.CloseGameWindow();

            var test = Lib.FusTask.IsCompleted;

            Console.ReadKey();

        }
    }
}