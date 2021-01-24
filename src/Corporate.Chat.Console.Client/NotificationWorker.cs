using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;

namespace Corporate.Chat.Console.Client
{
    public class NotificationWorker : IHostedService
    {
        private HubConnection _hubConnection;
        private bool isConnected;
        private string name;
        private readonly ILogger<NotificationWorker> logger;
        private IConfiguration configuration;

        public NotificationWorker(IConfiguration configuration, ILogger<NotificationWorker> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        private static bool _hasPerformedCleanup;
        private string DownloadImageToDisk(string httpImage)
        {
            // Toasts can live for up to 3 days, so we cache images for up to 3 days.
            // Note that this is a very simple cache that doesn't account for space usage, so
            // this could easily consume a lot of space within the span of 3 days.
            try
            {
                if (ToastNotificationManagerCompat.CanUseHttpImages)
                {
                    return httpImage;
                }

                var directory = Directory.CreateDirectory(System.IO.Path.GetTempPath() + "WindowsNotifications.DesktopToasts.Images");

                if (!_hasPerformedCleanup)
                {
                    // First time we run, we'll perform cleanup of old images
                    _hasPerformedCleanup = true;

                    foreach (var d in directory.EnumerateDirectories())
                    {
                        if (d.CreationTimeUtc.Date < DateTime.UtcNow.Date.AddDays(-3))
                        {
                            d.Delete(true);
                        }
                    }
                }

                var dayDirectory = directory.CreateSubdirectory(DateTime.UtcNow.Day.ToString());
                string imagePath = dayDirectory.FullName + "\\" + (uint)httpImage.GetHashCode();

                if (File.Exists(imagePath))
                {
                    return imagePath;
                }

                HttpClient c = new HttpClient();
                using (var stream = c.GetStreamAsync(httpImage).Result)
                {
                    using (var fileStream = File.OpenWrite(imagePath))
                    {
                        stream.CopyTo(fileStream);
                    }
                }

                return imagePath;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("---Notification Client ---");

            logger.LogInformation("Type your name:");

            // TODO : get windows authenticated user
            name = System.Console.ReadLine();

            await SetupSignalRHubAsync();

            // Listen to notification activation
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                if (args.Contains("action"))
                {
                    switch(args["action"])
                    {
                        case "Send":
                            // Obtain any user input (text boxes, menu selections) from the notification
                            ValueSet userInput = toastArgs.UserInput;
                            logger.LogInformation($"Received response from user : {userInput["tbReply"]}");
                            break;
                    }
                }
            };

            do
            {
                try
                {
                    if (!isConnected)
                    {
                        await SetupSignalRHubAsync();
                    }
                    else
                    {
                        var message = System.Console.ReadLine();

                        if (!string.IsNullOrEmpty(message))
                        {
                            //await _hubConnection.SendAsync("SendToUser", new Message { Name = name, Text = message }, "Ludo");
                            await _hubConnection.SendAsync("Send", new Message { Name = name, Text = message });
                            logger.LogInformation("SendAsync to Hub");
                        }
                    }
                }
                catch (Exception ex)
                {
                    isConnected = false;
                    logger.LogInformation($"Error on SendAsync: {ex.Message}");
                }
            }
            while (System.Console.ReadKey(false).Key != ConsoleKey.Escape);

            await _hubConnection.DisposeAsync();
        }

        private async Task SetupSignalRHubAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/notification", option=> { option.UseDefaultCredentials = true; })
                .WithAutomaticReconnect()
                .ConfigureLogging(factory =>
                {
                    factory.AddConsole();
                    factory.AddFilter("Console", level => level >= LogLevel.Trace);
                }).Build();

            await _hubConnection.StartAsync();

            logger.LogInformation("Connected to Hub");
            logger.LogInformation("Press ESC to stop");
            logger.LogInformation("Type message:");

            isConnected = true;

            try
            {
                await _hubConnection.SendAsync("OnUserConnected", new Message { Name = name });
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error on OnUserConnected: {ex.Message}");
                isConnected = false;
            }

            _hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(60);

            _hubConnection.Closed += (args) =>
            {
                isConnected = false;
                logger.LogInformation($"Connection close {args?.Message}");
                return Task.CompletedTask;
            };

            _hubConnection.On<Message>("ReceiveMessage", (message) =>
            {
                if (message != null)
                {
                    logger.LogInformation($"Received Message -> {message.Name} said: {message.Text}");

                    var appLogoImage = new Uri(DownloadImageToDisk("https://unsplash.it/64?image=1005"));
                    var inlineImage = new Uri(DownloadImageToDisk("https://picsum.photos/364/202?image=883"));

                    // TODO : validate link ok
                    if (!string.IsNullOrEmpty(message.CircleImageUri))
                    {
                        appLogoImage = new Uri(DownloadImageToDisk(message.CircleImageUri));
                    }

                    // TODO : validate link ok
                    if (!string.IsNullOrEmpty(message.ImageUri))
                    {
                        inlineImage = new Uri(DownloadImageToDisk(message.ImageUri));
                    }

                    // show notification in windows notif center
                    var toast = new ToastContentBuilder()
                        //.SetProtocolActivation("http://mydigitalworkplace.com")

                        .AddArgument("action", "viewConversation")
                        .AddArgument("conversationId", 9813)

                        .AddHeader("1", "My digital workplace is talking to you", "")

                        .AddText($"{message.Name} send you a message")
                        .AddText(message.Text)

                        .AddAttributionText("Atribution text")

                        //.AddHeroImage(new Uri(DownloadImageToDisk("https://picsum.photos/364/202?image=883")))

                        .AddInlineImage(inlineImage)
                        .AddAppLogoOverride(appLogoImage, ToastGenericAppLogoCrop.Circle)

                        .SetToastScenario(ToastScenario.Reminder);

                    if (message.WithTextFeedback)
                    {
                        toast.AddInputTextBox("tbReply", "Type a reply")
                               .AddButton(new ToastButton()
                                    .SetContent("Send")
                                    //.SetImageUri(new Uri("Assets/NotifIcon.png", UriKind.Relative))
                                    .SetTextBoxId("tbReply") // Reference text box ID to place this next to the text box
                                    .AddArgument("action", "Send")
                                    .SetBackgroundActivation())
                                .AddButton(new ToastButton()
                                    .SetContent("No thanks")
                                    .SetBackgroundActivation());
                    }
                    else
                    {
                        toast.AddButton(new ToastButton()
                                    .SetContent("Ok")
                                    .SetBackgroundActivation());
                    }

                    toast.Show();
                }
            });

            _hubConnection.On<Message>("UserConnected", (message) =>
            {
                logger.LogInformation($"{message.Text}");
            });

            _hubConnection.On<Message>("UserDisconnected", (message) =>
            {
                logger.LogInformation($"{message.Text}");
            });
        }

        

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Stop Service..");
            await _hubConnection?.DisposeAsync();
        }
    }
}