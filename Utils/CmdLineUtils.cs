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
        /// <summary>
        /// Execute's a git command with arguments in a windowless process.
        /// Can provide a callback which will be run after the process is complete.
        /// Returns a tuple where the first element is the std error output and the second element is the std output.
        /// Example call would have arguments ("status") or ("clone https://github.com/AlanWills/Repo.git")
        /// </summary>
        /// <param name="commandAndArgs"></param>
        /// <param name="onCommandCompleteCallback"></param>
        /// <returns></returns>
        public static Tuple<string, string> PerformGitCommand(string commandAndArgs, EventHandler onCommandCompleteCallback = null)
        {
            ProcessStartInfo gitInfo = CreateCmdLineProcessStartInfo(commandAndArgs);
            gitInfo.FileName = @"C:\Program Files (x86)\Git\cmd\git.exe";

            Process gitProcess = new Process();
            gitProcess.StartInfo = gitInfo;
            
            if (onCommandCompleteCallback != null)
            {
                gitProcess.Disposed += onCommandCompleteCallback;
            }

            gitProcess.Start();

            string stdError = gitProcess.StandardError.ReadToEnd();
            string stdOut = gitProcess.StandardOutput.ReadToEnd();

            return new Tuple<string, string>(stdError, stdOut);
        }

        /// <summary>
        /// Executes a windows command prompt command in a windowless process.
        /// Can provide a callback which will be run after the process is complete.
        /// Returns a tuple where the first element is the std error output and the second element is the std output.
        /// </summary>
        /// <param name="commandAndArgs"></param>
        /// <param name="onCommandCompleteCallback"></param>
        /// <returns></returns>
        public static Tuple<string, string> PerformCommand(string commandAndArgs, EventHandler onCommandCompleteCallback = null)
        {
            ProcessStartInfo cmdInfo = CreateCmdLineProcessStartInfo(commandAndArgs);

            Process process = new Process();
            process.StartInfo = cmdInfo;

            if (onCommandCompleteCallback != null)
            {
                process.Disposed += onCommandCompleteCallback;
            }

            process.Start();

            string stdError = process.StandardError.ReadToEnd();
            string stdOut = process.StandardOutput.ReadToEnd();

            return new Tuple<string, string>(stdError, stdOut);
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
    }
}
