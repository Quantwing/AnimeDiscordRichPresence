using DiscordRPC;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Quantwing
{
    public partial class AnimeDRPService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        public static DiscordRpcClient discordRpcClient;
        public WebSocketServer webSocketServer;
        public static EventLog eventLog;
        public static RichPresence currentPresence = null;

        public AnimeDRPService()
        {
            InitializeComponent();

            eventLog = new EventLog();

            if (!EventLog.SourceExists("AnimeDRP"))
            {
                EventLog.CreateEventSource("AnimeDRP", "Anime Discord Rich Presence");
            }

            eventLog.Source = "AnimeDRP";
            eventLog.Log = "Anime Discord Rich Presence";
        }

        protected override void OnStart(string[] args)
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 30000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            discordRpcClient = new DiscordRpcClient("639207220332068864", autoEvents: false);
            discordRpcClient.Initialize();

            webSocketServer = new WebSocketServer(IPAddress.Parse("127.0.0.1"), 8080);
            webSocketServer.AddWebSocketService<AnimeDRPWebSocketBehaviour>("/AnimeDRP");
            webSocketServer.Start();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 10000
            };

            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            discordRpcClient.Dispose();
            webSocketServer.Stop();
            eventLog.Close();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        public static void UpdatePresence(InputData data)
        {
            switch (data.action)
            {
                case 0:
                    currentPresence = new RichPresence()
                    {
                        Details = data.title,
                        State = "(" + data.currentEpisode + " of " + data.maxEpisodes + ")",
                        Timestamps = CreateTimestamp(data.currentTime, data.duration),
                        Assets = new Assets()
                        {
                            LargeImageKey = "image_large",
                            LargeImageText = "Anime Discord Rich Presence",
                            SmallImageKey = "image_small"
                        }
                    };
                    discordRpcClient.SetPresence(currentPresence);
                    break;

                case 1:
                    if (currentPresence == null)
                        break;
                    discordRpcClient.SetPresence(currentPresence);
                    break;

                case 2:
                    if (currentPresence == null)
                        break;
                    currentPresence.Timestamps = CreateTimestamp(data.currentTime, data.duration);
                    discordRpcClient.SetPresence(currentPresence);
                    break;

                case 3:
                    currentPresence = null;
                    discordRpcClient.ClearPresence();
                    break;

                default:
                    break;
            }
        }

        private static Timestamps CreateTimestamp(float currentSeconds, float maxSeconds)
        {
            return new Timestamps(new DateTime((long)(currentSeconds * Math.Pow(10, 7))), new DateTime((long)(maxSeconds * Math.Pow(10, 7))));
        }

        class AnimeDRPWebSocketBehaviour : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                String json = e.Data;

                try
                {
                    UpdatePresence(JsonConvert.DeserializeObject<InputData>(json));
                }
                catch (Exception exception)
                {
                    eventLog.WriteEntry(exception.Message + "\n\n" + exception.StackTrace);
                }
            }
        }
    }
}
