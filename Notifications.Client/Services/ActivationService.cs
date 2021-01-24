using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Notifications.Client.Activation;
using Notifications.Client.Core.Helpers;
using Notifications.Client.Core.Services;
using Notifications.Client.Services;

using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Notifications.Client.Services
{
    // For more information on understanding and extending activation flow see
    // https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/activation.md
    internal class ActivationService
    {
        private readonly App _app;
        private readonly Type _defaultNavItem;
        private Lazy<UIElement> _shell;

        private object _lastActivationArgs;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        public static readonly KeyboardAccelerator AltLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);

        public static readonly KeyboardAccelerator BackKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        public ActivationService(App app, Type defaultNavItem, Lazy<UIElement> shell = null)
        {
            _app = app;
            _shell = shell;
            _defaultNavItem = defaultNavItem;
            IdentityService.LoggedIn += OnLoggedIn;
        }

        public async Task ActivateAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs))
            {
                // Initialize services that you need before app activation
                // take into account that the splash screen is shown while this code runs.
                await InitializeAsync();
                UserDataService.Initialize();
                IdentityService.InitializeWithAadAndPersonalMsAccounts();
                var silentLoginSuccess = await IdentityService.AcquireTokenSilentAsync();
                if (!silentLoginSuccess || !IdentityService.IsAuthorized())
                {
                    await RedirectLoginPageAsync();
                }

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null)
                {
                    // Create a Shell or Frame to act as the navigation context
                    Window.Current.Content = _shell?.Value ?? new Frame();
                    NavigationService.NavigationFailed += (sender, e) =>
                    {
                        throw e.Exception;
                    };
                }
            }

            // Depending on activationArgs one of ActivationHandlers or DefaultActivationHandler
            // will navigate to the first page
            if (IdentityService.IsLoggedIn())
            {
                await HandleActivationAsync(activationArgs);
            }

            _lastActivationArgs = activationArgs;

            if (IsInteractive(activationArgs))
            {
                var activation = activationArgs as IActivatedEventArgs;
                if (activation.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    await Singleton<SuspendAndResumeService>.Instance.RestoreSuspendAndResumeData();
                }

                // Ensure the current window is active
                Window.Current.Activate();

                // Tasks after activation
                await StartupAsync();
            }
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            if (_shell?.Value != null)
            {
                Window.Current.Content = _shell.Value;
            }
            else
            {
                var frame = new Frame();
                Window.Current.Content = frame;
                NavigationService.Frame = frame;
            }

            await ThemeSelectorService.SetRequestedThemeAsync();
            await HandleActivationAsync(_lastActivationArgs);
        }

        public async Task RedirectLoginPageAsync()
        {
            var frame = new Frame();
            NavigationService.Frame = frame;
            Window.Current.Content = frame;
            await ThemeSelectorService.SetRequestedThemeAsync();
            NavigationService.Navigate<Views.LogInPage>();
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
        }

        private async Task InitializeAsync()
        {
            await Singleton<BackgroundTaskService>.Instance.RegisterBackgroundTasksAsync().ConfigureAwait(false);
            await ThemeSelectorService.InitializeAsync().ConfigureAwait(false);
        }

        private async Task HandleActivationAsync(object activationArgs)
        {
            var activationHandler = GetActivationHandlers()
                                                .FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync(activationArgs);
            }

            if (IsInteractive(activationArgs))
            {
                var defaultHandler = new DefaultActivationHandler(_defaultNavItem);
                if (defaultHandler.CanHandle(activationArgs))
                {
                    await defaultHandler.HandleAsync(activationArgs);
                }
            }
        }

        private async Task StartupAsync()
        {
            await ThemeSelectorService.SetRequestedThemeAsync();

            // TODO WTS: Configure and enable Azure Notification Hub integration.
            //  1. Go to the HubNotificationsService class, in the InitializeAsync() method, provide the Hub Name and DefaultListenSharedAccessSignature.
            //  2. Uncomment the following line (an exception will be thrown if it is executed and the above information is not provided).
            // await Singleton<HubNotificationsService>.Instance.InitializeAsync().ConfigureAwait(false);
            await FirstRunDisplayService.ShowIfAppropriateAsync();
            await WhatsNewDisplayService.ShowIfAppropriateAsync();
        }

        private IEnumerable<ActivationHandler> GetActivationHandlers()
        {
            yield return Singleton<BackgroundTaskService>.Instance;
            yield return Singleton<HubNotificationsService>.Instance;
            yield return Singleton<ToastNotificationsService>.Instance;
            yield return Singleton<SuspendAndResumeService>.Instance;
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }
    }
}
