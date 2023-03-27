using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;


/* Utility to send close-command (ctrl-c) to a child console application from another (main) console application 
 * The actual kill signal needs to be sent from a second child console app (which is this one).
 * Main application starts this utility program to send the kill-signal to its child application
 * 
 * Based on the solution described here:
 * https://stackoverflow.com/a/29274238/2373715
 */

namespace ProcCtrlC
{

    internal class Program
    {
        internal const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(uint CtrlType);


        static int Main(string[] args)
        {
            //Argument is process id of the (console) application we want to kill
            try
            {
                CtrlC(uint.Parse(args[0]));
            }
            finally
            {
            }
            return 0;
        }



        internal static bool CtrlC(uint pid)
        {
            //System.Diagnostics.Debugger.Launch();


            //Needs to free current console, or else Attach will not work
            FreeConsole();
            //Attaching to target applications console (which we will send killsignal to) by process id
            if (AttachConsole((uint)pid))
            {
                SetConsoleCtrlHandler(null, true);
                try
                {
                    //Send Ctrl-C
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                        return false;
                    FreeConsole();

                    Thread.Sleep(2000);
                }
                finally
                {
                    SetConsoleCtrlHandler(null, false);
                }
                return true;
            }
            return false;
        }
    }
}
