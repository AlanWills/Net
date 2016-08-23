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
        private const string _2DEngineTestPath = @"C:\Users\Alan\Documents\Visual Studio 2015\Projects\2DEngine\2DEngineUnitTestGameProject";

        protected override void ProcessMessage(byte[] data)
        {
            base.ProcessMessage(data);

            if (data.ConvertToString() == "Request Build")
            {
                ClientComms.Send("Build request confirmed");

                TestProject("CelesteEngine", Read2DEngineLogAndSendMessage);
            }
        }

        /// <summary>
        /// Clones the repository, runs the tests and then when the process has finished, reads the log file and emails the results
        /// </summary>
        /// <param name="projectDirectoryPath"></param>
        /// <param name="projectExeName"></param>
        private void TestProject(string projectGithubRepoName, EventHandler logAndEmailEvent)
        {
            ProcessStartInfo gitInfo = new ProcessStartInfo();
            gitInfo.CreateNoWindow = true;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            gitInfo.UseShellExecute = false;
            gitInfo.FileName = @"C:\Program Files\Git\bin\git.exe";
            gitInfo.Arguments = "clone https://github.com/AlanWills/" + projectGithubRepoName + ".git " + projectGithubRepoName;
            gitInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            Process gitProcess = new Process();
            gitProcess.StartInfo = gitInfo;
            gitProcess.Disposed += logAndEmailEvent;
            gitProcess.Start();

            Console.WriteLine(gitProcess.StandardError.ReadToEnd());
            Console.WriteLine(gitProcess.StandardOutput.ReadToEnd());
            Console.WriteLine("Checkout completed");

            // Delete the repo - we should do this after testing but for now do it here to clean up
            DirectoryInfo directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), projectGithubRepoName));

            // Git files are funny and we have to change the access level
            foreach (FileInfo file in new DirectoryInfo(Path.Combine(directory.FullName, ".git")).GetFiles(".", SearchOption.AllDirectories))
            {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
            }

            Directory.Delete(directory.FullName, true);
        }

        /// <summary>
        /// Reads the log file for the Celeste Engine and either sends back the result or emails the result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Read2DEngineLogAndSendMessage(object sender, EventArgs e)
        {
            StringBuilder fileContents = new StringBuilder();
            foreach (string line in File.ReadLines(Path.Combine(_2DEngineTestPath, "TestResults", "TestResults.txt")))
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

        // Start using UnitTestFramework and move this to a helper function for invoking all MS unit tests

        //private void TestAssembly(string dllFullPath)
        //{
        //    DateTime buildRequestTime = DateTime.Now;

        //    Assembly assembly = Assembly.Load(dllFullPath);
        //    Celeste.Cel.ScriptDirectoryPath = @"C:\Users\Alan\Documents\Visual Studio 2015\Projects\Celeste\Celeste-master\Celeste\TestCeleste\Scripts";

        //    // Turn off asserts
        //    Trace.Listeners.Clear();

        //    List<string> passedTests = new List<string>();
        //    List<string> failedTests = new List<string>();

        //    foreach (Type type in assembly.GetExportedTypes())
        //    {
        //        if (type.IsAbstract) { continue; }

        //        List<MethodInfo> methods = new List<MethodInfo>(type.GetMethods());

        //        MethodInfo classInitialiseFunction = methods.Find(x => x.GetCustomAttribute<ClassInitializeAttribute>() != null);
        //        MethodInfo testInitialiseFunction = methods.Find(x => x.GetCustomAttribute<TestInitializeAttribute>() != null);
        //        MethodInfo testCleanupFunction = methods.Find(x => x.GetCustomAttribute<TestCleanupAttribute>() != null);
        //        MethodInfo classCleanupFunction = methods.Find(x => x.GetCustomAttribute<ClassCleanupAttribute>() != null);

        //        object testedClass = Activator.CreateInstance(type);

        //        if (type.GetCustomAttribute<TestClassAttribute>() != null)
        //        {
        //            if (classInitialiseFunction != null)
        //            {
        //                try
        //                {
        //                    classInitialiseFunction.Invoke(testedClass, null);
        //                }
        //                catch { continue; }
        //            }

        //            foreach (MethodInfo method in methods)
        //            {
        //                if (method.GetCustomAttribute<TestMethodAttribute>() != null)
        //                {
        //                    if (testInitialiseFunction != null)
        //                    {
        //                        try
        //                        {
        //                            testInitialiseFunction.Invoke(testedClass, null);
        //                        }
        //                        catch { continue; }
        //                    }

        //                    try
        //                    {
        //                        method.Invoke(testedClass, null);
        //                        passedTests.Add(method.Name);
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        failedTests.Add(method.Name);
        //                    }

        //                    if (testCleanupFunction != null)
        //                    {
        //                        try
        //                        {
        //                            testCleanupFunction.Invoke(testedClass, null);
        //                        }
        //                        catch { continue; }
        //                    }
        //                }
        //            }

        //            if (classCleanupFunction != null)
        //            {
        //                try
        //                {
        //                    classCleanupFunction.Invoke(testedClass, null);
        //                }
        //                catch { continue; }
        //            }
        //        }
        //    }

        //    DateTime buildCompleteTime = DateTime.Now;

        //    StringBuilder messageBody = new StringBuilder();
        //    messageBody.AppendLine("Build Request completed at " + buildCompleteTime.ToShortTimeString());
        //    messageBody.AppendLine();
        //    messageBody.AppendLine(failedTests.Count.ToString() + " tests failed");
        //    messageBody.AppendLine(passedTests.Count.ToString() + " tests passed");
        //    messageBody.AppendLine();
        //    messageBody.AppendLine("Failed Tests:");
        //    messageBody.AppendLine();

        //    foreach (string failedTest in failedTests)
        //    {
        //        messageBody.AppendLine(failedTest);
        //    }

        //    messageBody.AppendLine();
        //    messageBody.AppendLine("Passed Tests:");
        //    messageBody.AppendLine();

        //    foreach (string passedTest in passedTests)
        //    {
        //        messageBody.AppendLine(passedTest);
        //    }

        //    MailMessage mail = new MailMessage("alawills@googlemail.com", "alawills@googlemail.com");
        //    SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

        //    mail.Subject = "Build Request " + buildRequestTime.ToShortTimeString();
        //    mail.Body = messageBody.ToString();

        //    client.Port = 587;
        //    client.Credentials = new System.Net.NetworkCredential("alawills", "favouriteprimes111929");
        //    client.EnableSsl = true;

        //    client.Send(mail);

        //    Console.WriteLine("Testing run complete");
        //}
    }
}
