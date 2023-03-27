using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


/*https://stackoverflow.com/a/29274238/2373715*/

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


        static void Main(string[] args)
        {
            CtrlC(uint.Parse(args[0]));
        }



        internal static bool CtrlC(uint pid)
        {
            //System.Diagnostics.Debugger.Launch();
            FreeConsole();
            if (AttachConsole((uint)pid))
            {
                SetConsoleCtrlHandler(null, true);
                try
                {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                        return false;
                }
                finally
                {
                    SetConsoleCtrlHandler(null, false);
                    FreeConsole();
                }
                return true;
            }
            return false;
        }
    }
}
