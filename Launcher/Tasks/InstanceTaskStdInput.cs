using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeanBatchLauncher.Launcher.Program;
using static CommandLineEncoder.Utils;
using System.IO;
using System.Diagnostics;

namespace LeanBatchLauncher.Launcher.Tasks
{
    internal class InstanceTaskStdInput
    {
        internal static void Start(Configuration userConfiguration, InstanceContext context)
        {
            // Use ProcessStartInfo class.
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                //FileName = Path.Combine("c:\\Projects\\QuantConnect\\Lean\\Launcher\\bin\\Debug\\", "Instance.exe"),
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Instance.exe"),
                //WorkingDirectory = "c:\\Projects\\QuantConnect\\Lean\\Launcher\\bin\\Debug\\",
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                RedirectStandardInput= true,
            };

            // Create arguments
            var builder = new StringBuilder();
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(userConfiguration.LibraryPath)));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(userConfiguration.ApiJobUserId)));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(userConfiguration.ApiAccessToken)));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(context.StartDate.ToString("yyyy-MM-dd"))));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(context.EndDate.ToString("yyyy-MM-dd"))));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(context.AlphaModelName)));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(context.Symbol)));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(context.MinuteResolution.ToString())));
            builder.Append(String.Format("\"{0}\" ", EncodeArgText(context.ParametersSerialized)));

            // Append to StartInfo
            startInfo.Arguments = builder.ToString();

            // Start the process with the info we specified
            using (Process exeProcess = Process.Start(startInfo))
            {
                StreamWriter myStreamWriter = exeProcess.StandardInput;
                
                myStreamWriter.Write("{\r\n  \"Name\": \"test\",\r\n  \"MaxOrders\": 1,\r\n  \"Period\": \"2.00:10:00\",\r\n  \"MLSessionId\": 499,\r\n  \"MLIterationId\": 3078,\r\n  \"MLCluster\": 222,\r\n  \"ConcreteOrderDefinitions\": {\r\n    \"ProfitLoss\": {\r\n      \"ProfitPct\": 0.05,\r\n      \"SLPct\": -0.05,\r\n      \"TimeInForceTimeSpanEntry\": null,\r\n      \"TimeInForceTimeSpanProfit\": null,\r\n      \"FixedAmountToBuy\": 40000\r\n    }\r\n  }\r\n}");
                myStreamWriter.Close();

                // Proceeed when process is finished
                exeProcess.WaitForExit();
            }
        }
    }
}
