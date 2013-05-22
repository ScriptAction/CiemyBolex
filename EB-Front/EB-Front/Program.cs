using System;
using System.Diagnostics;
using System.IO;
using EasyHook;
using System.Runtime.Remoting;

namespace EB_Front
{
    public class InjectorInterface : MarshalByRefObject
    {
        public void ReportException(Exception ex)
        {
            Console.WriteLine("The target process has reported anerror:\r\n" + ex.ToString());
        }

        public void Log(String msg)
        {
            Console.WriteLine(msg);
        }

        public void LogToFile(String msg)
        {
            File.WriteAllText("debug.log", msg);
        }

        public void Ping()
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int pid = GetExeFilePID();
            if (pid < 0)
            {
                Console.WriteLine("ExeFile could not find!");
                return;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: EB-Front init_path script_path");
                return;
            }

            string initPath = args[0];
            string scriptPath = args[1];

            try
            {
                try
                {
                    Config.Register(
                        "by kid",
                        "EB-Front.exe",
                        "EB-Inject-Shell.dll");
                }
                catch (ApplicationException)
                {
                    Console.WriteLine("This is an adminstrative task! Permission denied...");
                    Process.GetCurrentProcess().Kill();
                }

                String channelName = null;

                RemoteHooking.IpcCreateServer<InjectorInterface>(ref channelName, WellKnownObjectMode.SingleCall);

                RemoteHooking.Inject(
                    pid,
                    "EB-Inject-Shell.dll",
                    "EB-Inject-Shell.dll",
                    channelName,
                    initPath,
                    scriptPath);

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error while connecting to target:\r\n{0}", ex.ToString());
            }
        }

        static int GetExeFilePID()
        {
            Process[] ps = Process.GetProcessesByName("ExeFile");
            return ps.Length > 0 ? ps[0].Id : -1;
        }
    }
}
