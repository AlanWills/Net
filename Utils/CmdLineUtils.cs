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
        /// Provide a callback which will be run after the process is complete.
        /// Returns a tuple where the first element is the std error output and the second element is the std output.
        /// Example call would have arguments ("status") or ("clone https://github.com/AlanWills/Repo.git")
        /// </summary>
        /// <param name="commandAndArgs"></param>
        /// <param name="onCommandCompleteCallback"></param>
        /// <returns></returns>
        public static Tuple<string, string> PerformGitCommand(string commandAndArgs, EventHandler onCommandCompleteCallback = null)
        {
            ProcessStartInfo gitInfo = new ProcessStartInfo();
            gitInfo.CreateNoWindow = true;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            gitInfo.UseShellExecute = false;
            gitInfo.FileName = @"C:\Program Files\Git\bin\git.exe";
            gitInfo.Arguments = commandAndArgs;
            gitInfo.WorkingDirectory = Directory.GetCurrentDirectory();

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
    }
}
