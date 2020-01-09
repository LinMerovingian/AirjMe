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
            // Console.WriteLine("Airj's Anti AFK starting...");
            foreach (var process in processList)
            {
                new Thread(() =>
                {
                    var wowProcess = new WowProcess(process);
                    wowProcess.Open();
                    wowProcess.OffsetHolder.InitializeOffsets();
                    wowProcess.AllowCreateRemoteThread(true);

                    Thread.Sleep(5);
                    if (wowProcess.IsInGame())
                    {
                        Console.WriteLine($"Starting for process {process.Id}, {wowProcess.GetPlayerName()}");
                    }

                    // wowProcess.ResumeAllThread();
                    while (true)
                    {
                        if (wowProcess.IsInGame())
                        {
                            wowProcess.SuspendAllThread();
                            var tainted = wowProcess.GetTaintedValue();
                            Console.WriteLine($"Tainted = {tainted}");
                            if (tainted != 0)
                            {
                                wowProcess.SetTaintedValue(0);
                            }
                            wowProcess.Test5();
                            if (tainted != 0)
                            {
                                wowProcess.SetTaintedValue(tainted);
                            }
                            wowProcess.ResumeAllThread();
                            Thread.Sleep(20);
                        }
                    }

                    Thread.Sleep(5);
                    wowProcess.AllowCreateRemoteThread(false);
                    wowProcess.Close();
                }).Start();
            }

            // Console.WriteLine("Airj's Anti AFK finished. press any key to exit");
            Console.ReadKey();
        }
    }
}