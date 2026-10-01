using System;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;

namespace PetWorldMobile
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            UnhandledException += OnUnhandledException;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                var rootFrame = Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
                if (rootFrame == null)
                {
                    rootFrame = new Windows.UI.Xaml.Controls.Frame();
                    Window.Current.Content = rootFrame;
                }

                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage));

                Window.Current.Activate();
            }
            catch (Exception ex)
            {
                RecordCrash("launch", ex);
                try
                {
                    Window.Current.Activate();
                }
                catch
                {
                }
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RecordCrash("unhandled", e.Exception);

            // Let the UI stay alive when a recoverable event exception escapes
            // one of the page-level guards.
            e.Handled = true;
        }

        private void RecordCrash(string area, Exception ex)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["PetWorld.LastCrash"] =
                    DateTime.UtcNow.ToString("o") + " | " + area + " | " + ex.Message;
            }
            catch
            {
                // Never throw while attempting to log a crash.
            }
        }

        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            // Game state is saved by the page after each gameplay action.
        }
    }
}
