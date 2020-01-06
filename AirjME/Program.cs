using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace AirjME
{
    class Program
    {
        static void Main(string[] args)
        {
            var processList = System.Diagnostics.Process.GetProcessesByName("WowClassic");
            Console.WriteLine("Airj's Anti AFK starting...");
            foreach (var process in processList)
            {
                var wowProcess = new WowProcess(process);
                wowProcess.Open();
                wowProcess.AllowCreateRemoteThread(true);
                Thread.Sleep(5);
                while (wowProcess.IsInGame())
                {
                    // wowProcess.GetPlayerPosition();
                    var time = wowProcess.CallFunction(0x2a53d0);
                    wowProcess.IterateObject();
                    //Thread.Sleep(100);
                }
                Thread.Sleep(5);
                wowProcess.AllowCreateRemoteThread(false);
                wowProcess.Close();
            }
            Console.WriteLine("Airj's Anti AFK finished. press any key to exit");
            Console.ReadKey();
        }
    }
}
