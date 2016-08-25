using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;
using Utils;

namespace BuildServer
{
    public class Server : BaseServer
    {
        private const string UnitTestFrameworkRepoName = "MonoGameUnitTestFramework";

        protected override void ProcessMessage(byte[] data)
        {
            base.ProcessMessage(data);

            if (data.ConvertToString() == "Request Build")
            {
                ClientComms.Send("Build request confirmed");

                // Check out the monogame unit test framework
                CmdLineUtils.PerformCommand(CmdLineUtils.Git, "clone https://github.com/AlanWills/" + UnitTestFrameworkRepoName + ".git");
                Console.WriteLine("Checkout of test framework completed");

                TestProject("CelesteEngine", "CelesteEngineTestProject", Read2DEngineLogAndSendMessage);

                // Delete the monogame unit test framework
                DeleteGitRepo(Path.Combine(Directory.GetCurrentDirectory(), UnitTestFrameworkRepoName));
            }
        }

        /// <summary>
        /// Clones the repository, runs the tests and then when the process has finished, reads the log file and emails the results
        /// </summary>
        /// <param name="projectDirectoryPath"></param>
        /// <param name="projectExeName"></param>
        private void TestProject(string projectGithubRepoName, string testProjectName, EventHandler logAndMessageEvent)
        {
            CmdLineUtils.PerformCommand(CmdLineUtils.Git, "clone https://github.com/AlanWills/" + projectGithubRepoName + ".git " + projectGithubRepoName);
            Console.WriteLine("Checkout of " + projectGithubRepoName + " completed");

            string pathToSolution = Path.Combine(projectGithubRepoName, testProjectName) + ".sln";
            CmdLineUtils.PerformCommand(CmdLineUtils.MSBuild, pathToSolution);

            // Start the test project and wait for it to close
            string processPath = Path.Combine(Directory.GetCurrentDirectory(), projectGithubRepoName, projectGithubRepoName, @"bin\Windows\x86\Debug", testProjectName + ".exe");
            Process proc = Process.Start(processPath);
            proc.WaitForExit();
            proc.Close();

            // Send the results
            logAndMessageEvent.Invoke(this, EventArgs.Empty);

            // Clean up all of the files we have checked out
            DeleteGitRepo(Path.Combine(Directory.GetCurrentDirectory(), projectGithubRepoName));
        }

        /// <summary>
        /// Reads the log file for the Celeste Engine and either sends back the result or emails the result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Read2DEngineLogAndSendMessage(object sender, EventArgs e)
        {
            StringBuilder fileContents = new StringBuilder();
            foreach (string line in File.ReadLines(Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "TestResults.txt")))
            {
                fileContents.AppendLine(line);
            }

            Message(fileContents);
        }

        /// <summary>
        /// Either sends back via comms or emails the inputted string in the string builder to me if there is no connection
        /// </summary>
        /// <param name="testRunInformation"></param>
        private void Message(StringBuilder testRunInformation)
        {
            DateTime buildCompleteTime = DateTime.Now;

            testRunInformation.AppendLine();
            testRunInformation.Append("Build Request completed at " + buildCompleteTime.ToShortTimeString());

            try
            {
                ClientComms.Send(testRunInformation.ToString());
            }
            catch
            {
                MailMessage mail = new MailMessage("alawills@googlemail.com", "alawills@googlemail.com");
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

                mail.Subject = "Build Request";
                mail.Body = testRunInformation.ToString();

                client.Port = 587;
                client.Credentials = new System.Net.NetworkCredential("alawills", "favouriteprimes111929");
                client.EnableSsl = true;

                client.Send(mail);
            }

            Console.WriteLine("Testing run complete");
        }

        /// <summary>
        /// Git files require special permissions to delete, so this function just wraps up all of that functionality
        /// </summary>
        /// <param name="folderFullPath"></param>
        private void DeleteGitRepo(string folderFullPath)
        {
            DirectoryInfo directory = new DirectoryInfo(folderFullPath);

            // Git files are funny and we have to change the access level
            foreach (FileInfo file in new DirectoryInfo(Path.Combine(directory.FullName, ".git")).GetFiles(".", SearchOption.AllDirectories))
            {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
            }

            Directory.Delete(directory.FullName, true);
        }
    }
}
