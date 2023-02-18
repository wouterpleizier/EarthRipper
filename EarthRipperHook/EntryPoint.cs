using EasyHook;
using EarthRipperHook.EarthPro;
using EarthRipperHook.OpenGL;
using EarthRipperHook.Qt5;
using EarthRipperHook.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EarthRipperHook
{
    public class EntryPoint : IEntryPoint
    {
        private ServerInterface _server = null;

        public EntryPoint(RemoteHooking.IContext context, string channelName, Settings settings)
        {
            _server = RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
            _server.Ping();
        }

        public void Run(RemoteHooking.IContext context, string channelName, Settings settings)
        {
            HookContainer[] hooks = null;
            Utility[] utilities = null;
            bool hasInitialized;

            try
            {
                _server.ReportMessage($"Hook library injected into process {RemoteHooking.GetCurrentProcessId()}");

                hooks = new HookContainer[]
                {
                    new Qt5Hooks(),
                    new IGHooks(),
                    new OpenGLHooks(),
                };

                RemoteHooking.WakeUpProcess();

                utilities = new Utility[]
                {
                    new RenderScale(),
                    new OrthoCamera(),
                    new HideUI(),
                    new Capture(settings.OutputDirectory, settings.CaptureFlags)
                };

                _server.ReportMessage("Hooks installed, awaiting input");
                hasInitialized = true;
            }
            catch (Exception exception)
            {
                hasInitialized = false;
                _server.ReportException(exception);
            }

            try
            {
                while (hasInitialized)
                {
                    Thread.Sleep(100);

                    List<object> messages = Logger.DequeueMessages();
                    if (messages.Count > 0)
                    {
                        foreach (object message in messages)
                        {
                            if (message is Exception exception)
                            {
                                _server.ReportException(exception);
                            }
                            else
                            {
                                _server.ReportMessage(message.ToString());
                            }
                        }
                    }
                    else
                    {
                        _server.Ping();
                    }
                }
            }
            catch
            {
                // An exception is raised when the server can't be reached - hopefully because the user closed the
                // console window. In that case we should clean everything up.
            }

            if (utilities != null)
            {
                foreach (Utility utility in utilities)
                {
                    utility.Dispose();
                }
            }

            if (hooks != null)
            {
                foreach (HookContainer hook in hooks)
                {
                    hook.Dispose();
                }
            }

            LocalHook.Release();
        }
    }
}
