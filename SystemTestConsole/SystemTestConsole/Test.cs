using DiscordRPC;
using Newtonsoft.Json;
using System;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SystemTestConsole
{
    class Test
    {
        private static DiscordRpcClient discordRpcClient;
        private WebSocketServer webSocketServer;
        private static RichPresence currentPresence = null;
        private static JsonSerializerSettings serializerSettings;

        public void Main()
        {
            Console.WriteLine("Starting...");
            OnStart();
            Console.WriteLine("Ready");
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "exit")
                    break;
            }
            Console.WriteLine("Exiting...");
            OnStop();
            Console.WriteLine("Program complete. Press any key to exit.");
            Console.ReadKey();
        }

        private void OnStart()
        {
            serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            discordRpcClient = new DiscordRpcClient("xxxxx", autoEvents: false);
            discordRpcClient.Initialize();

            webSocketServer = new WebSocketServer(IPAddress.Parse("127.0.0.1"), 8080);
            webSocketServer.AddWebSocketService<AnimeDRPWebSocketBehaviour>("/AnimeDRP");
            webSocketServer.Start();
        }

        private void OnStop()
        {
            discordRpcClient.Dispose();
            webSocketServer.Stop();
        }

        public static void UpdatePresence(InputData data)
        {
            switch (data.action)
            {
                case 0:
                    currentPresence = new RichPresence()
                    {
                        Details = data.title,
                        State = "Episode [ " + data.currentEpisode + " / " + data.maxEpisodes + " ]",                      
                        Timestamps = new Timestamps() { StartUnixMilliseconds = (ulong)((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - data.currentTime) },
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
                    currentPresence.Timestamps = null;
                    discordRpcClient.SetPresence(currentPresence);
                    break;

                case 2:
                    if (currentPresence == null)
                        break;
                    //discordRpcClient.SetPresence(currentPresence);
                    break;

                case 3:
                    currentPresence = null;
                    discordRpcClient.ClearPresence();
                    break;

                default:
                    break;
            }
        }

        class AnimeDRPWebSocketBehaviour : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                String json = e.Data;
                try
                {
                    UpdatePresence(JsonConvert.DeserializeObject<InputData>(json, serializerSettings));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message + "\n\n" + exception.StackTrace);
                }
            }
        }
    }
}
