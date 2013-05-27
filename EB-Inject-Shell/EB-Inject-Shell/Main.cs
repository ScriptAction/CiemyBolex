using System;
using EasyHook;
using System.Diagnostics;

namespace EB_Inject_Shell
{
    public unsafe class Main : EasyHook.IEntryPoint
    {
        EB_Front.InjectorInterface injector;
        PyAdapter adapter;

        public Main(RemoteHooking.IContext context, String channelName, String workingDir, String initPath, String scriptPath)
        {
            injector = RemoteHooking.IpcConnectClient<EB_Front.InjectorInterface>(channelName);
            injector.Ping();
            adapter = new PyAdapter(injector);
        }

        public void Run(RemoteHooking.IContext context, String channelName, String workingDir, String initPath, String scriptPath)
        {
            try
            {
                CppDll.EBInit();
                adapter.Run(workingDir, initPath, scriptPath);
            }
            catch (Exception ex)
            {
                injector.ReportException(ex);
            }
            return;
        }
    }
}
