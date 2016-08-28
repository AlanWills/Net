using System;
using System.Diagnostics;
using System.IO;

namespace Utils
{
    /// <summary>
    /// Utility class for performing git operations from within C#
    /// </summary>
    public static class CmdLineUtils
    {
        public static string Git = @"C:\Program Files\Git\bin\git.exe";
        public static string MSBuild = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe";

        /// <summary>
        /// Executes a windows command prompt command in a windowless process.
        /// Can provide a callback which will be run after the process is complete.
        /// Asynchronously prints out the standard error and standard output to the Console.
        /// </summary>
        /// <param name="commandAndArgs"></param>
        /// <param name="onCommandCompleteCallback"></param>
        /// <returns></returns>
        public static void PerformCommand(string fileName, string commandAndArgs, EventHandler onCommandCompleteCallback = null)
        {
            ProcessStartInfo cmdInfo = CreateCmdLineProcessStartInfo(commandAndArgs);
            cmdInfo.FileName = fileName;
            RunProcess(cmdInfo, onCommandCompleteCallback);
        }

        /// <summary>
        /// Creates the process start info for running a command in a windowless process in the current working directory with the inputted arguments.
        /// StdError and StdOutput are redirected since it is windowless.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static ProcessStartInfo CreateCmdLineProcessStartInfo(string arguments = "")
        {
            ProcessStartInfo cmdInfo = new ProcessStartInfo();
            cmdInfo.CreateNoWindow = true;
            cmdInfo.RedirectStandardError = true;
            cmdInfo.RedirectStandardOutput = true;
            cmdInfo.UseShellExecute = false;
            cmdInfo.Arguments = arguments;
            cmdInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            return cmdInfo;
        }

        /// <summary>
        /// Creates a process which outputs error and output asynchronously to the standard output and will block until complete.
        /// </summary>
        /// <param name="processInfo"></param>
        /// <param name="onCommandCompleteCallback"></param>
        private static void RunProcess(ProcessStartInfo processInfo, EventHandler onCommandCompleteCallback)
        {
            Process process = new Process();
            process.StartInfo = processInfo;
            process.ErrorDataReceived += PrintToCommandLine;
            process.OutputDataReceived += PrintToCommandLine;

            if (onCommandCompleteCallback != null)
            {
                process.Disposed += onCommandCompleteCallback;
            }

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
            process.Close();
        }

        private static void PrintToCommandLine(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
