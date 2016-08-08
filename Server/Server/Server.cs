using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Utils;

namespace BuildServer
{
    public class Server
    {
        #region Properties and Fields

        /// <summary>
        /// The listener we can use to detect incoming connections from clients to the server
        /// </summary>
        private TcpListener Listener { get; set; }

        /// <summary>
        /// Our interface to the single client we are supporting for now
        /// </summary>
        public Comms ClientComms { get; private set; }

        /// <summary>
        /// Determines whether we have clients connected
        /// </summary>
        public bool Connections { get; private set; }

        #endregion

        public Server()
        {
            Listener = new TcpListener(IPAddress.Any, 1490);
            Listener.Start();

            ListenForNewClient();
        }

        /// <summary>
        /// Starts an asynchronous check for new connections
        /// </summary>
        private void ListenForNewClient()
        {
            Listener.BeginAcceptTcpClient(AcceptClient, null);
        }

        /// <summary>
        /// Callback for when a new client connects to the server
        /// </summary>
        /// <param name="asyncResult"></param>
        private void AcceptClient(IAsyncResult asyncResult)
        {
            ClientComms = new Comms(Listener.EndAcceptTcpClient(asyncResult));
            Console.WriteLine("Client accepted");
            ClientComms.Send("Client accepted");

            ClientComms.OnDataReceived += ProcessMessage;

            ListenForNewClient();
        }

        #region Message Callbacks

        /// <summary>
        /// Hard decode the message into a string and see if it matches the request to build
        /// </summary>
        /// <param name="data"></param>
        private void ProcessMessage(byte[] data)
        {
            string message = Encoding.UTF8.GetString(data);
            if (message == "Request Build")
            {
                ClientComms.Send("Request Confirmed");

                TestAssembly("TestCeleste");
            }
            else
            {
                ClientComms.Send("Invalid Request");
            }
        }

        #endregion

        // Start using UnitTestFramework and move this to a helper function for invoking all MS unit tests

        private void TestAssembly(string dllFullPath)
        {
            DateTime buildRequestTime = DateTime.Now;

            Assembly assembly = Assembly.Load(dllFullPath);
            Celeste.Cel.ScriptDirectoryPath = @"C:\Users\Alan\Documents\Visual Studio 2015\Projects\Celeste\Celeste-master\Celeste\TestCeleste\Scripts";

            // Turn off asserts
            Trace.Listeners.Clear();

            List<string> passedTests = new List<string>();
            List<string> failedTests = new List<string>();

            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.IsAbstract) { continue; }

                List<MethodInfo> methods = new List<MethodInfo>(type.GetMethods());

                MethodInfo classInitialiseFunction = methods.Find(x => x.GetCustomAttribute<ClassInitializeAttribute>() != null);
                MethodInfo testInitialiseFunction = methods.Find(x => x.GetCustomAttribute<TestInitializeAttribute>() != null);
                MethodInfo testCleanupFunction = methods.Find(x => x.GetCustomAttribute<TestCleanupAttribute>() != null);
                MethodInfo classCleanupFunction = methods.Find(x => x.GetCustomAttribute<ClassCleanupAttribute>() != null);

                object testedClass = Activator.CreateInstance(type);

                if (type.GetCustomAttribute<TestClassAttribute>() != null)
                {
                    if (classInitialiseFunction != null)
                    {
                        try
                        {
                            classInitialiseFunction.Invoke(testedClass, null);
                        }
                        catch { continue; }
                    }

                    foreach (MethodInfo method in methods)
                    {
                        if (method.GetCustomAttribute<TestMethodAttribute>() != null)
                        {
                            if (testInitialiseFunction != null)
                            {
                                try
                                {
                                    testInitialiseFunction.Invoke(testedClass, null);
                                }
                                catch { continue; }
                            }

                            try
                            {
                                method.Invoke(testedClass, null);
                                passedTests.Add(method.Name);
                            }
                            catch (Exception e)
                            {
                                failedTests.Add(method.Name);
                            }

                            if (testCleanupFunction != null)
                            {
                                try
                                {
                                    testCleanupFunction.Invoke(testedClass, null);
                                }
                                catch { continue; }
                            }
                        }
                    }

                    if (classCleanupFunction != null)
                    {
                        try
                        {
                            classCleanupFunction.Invoke(testedClass, null);
                        }
                        catch { continue; }
                    }
                }
            }

            DateTime buildCompleteTime = DateTime.Now;

            StringBuilder messageBody = new StringBuilder();
            messageBody.AppendLine("Build Request completed at " + buildCompleteTime.ToShortTimeString());
            messageBody.AppendLine();
            messageBody.AppendLine(failedTests.Count.ToString() + " tests failed");
            messageBody.AppendLine(passedTests.Count.ToString() + " tests passed");
            messageBody.AppendLine();
            messageBody.AppendLine("Failed Tests:");
            messageBody.AppendLine();

            foreach (string failedTest in failedTests)
            {
                messageBody.AppendLine(failedTest);
            }

            messageBody.AppendLine();
            messageBody.AppendLine("Passed Tests:");
            messageBody.AppendLine();

            foreach (string passedTest in passedTests)
            {
                messageBody.AppendLine(passedTest);
            }

            MailMessage mail = new MailMessage("alawills@googlemail.com", "alawills@googlemail.com");
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

            mail.Subject = "Build Request " + buildRequestTime.ToShortTimeString();
            mail.Body = messageBody.ToString();

            client.Port = 587;
            client.Credentials = new System.Net.NetworkCredential("alawills", "favouriteprimes111929");
            client.EnableSsl = true;

            client.Send(mail);

            Console.WriteLine("Testing run complete");
        }
    }
}
