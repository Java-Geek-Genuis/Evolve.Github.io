using PetWorld.Core;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace PetWorldMobile
{
    public sealed partial class MainPage : Page
    {
        private const string SaveKey = "PetWorld.Save";

        private PetState pet;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly DispatcherTimer blinkTimer = new DispatcherTimer();
        private bool sleeping;
        private bool busyReaction;
        private int reactionCount;

        public MainPage()
        {
            InitializeComponent();

            pet = LoadPet();

            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += OnTimerTick;

            blinkTimer.Interval = TimeSpan.FromSeconds(4);
            blinkTimer.Tick += BlinkTimer_Tick;

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;

            UpdateUi();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Start();
                blinkTimer.Start();
                SafeAnimate(IdlePetStoryboard);
                ShowSpeech("I'm home!");
            }
            catch (Exception ex)
            {
                RecoverFromError("startup", ex);
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Stop();
                blinkTimer.Stop();
                IdlePetStoryboard.Stop();
                BlinkStoryboard.Stop();
                PetReactionStoryboard.Stop();
                ActionBounceStoryboard.Stop();
                BubbleStoryboard.Stop();
            }
            catch
            {
                // The page is leaving; never let cleanup crash navigation.
            }
        }

        private void BlinkTimer_Tick(object sender, object e)
        {
            try
            {
                SafeAnimate(BlinkStoryboard);
            }
            catch (Exception ex)
            {
                RecoverFromError("blink animation", ex);
            }
        }

        private void OnTimerTick(object sender, object e)
        {
            try
            {
                pet.Tick(1);
                SavePet();
                UpdateUi();

                if (!sleeping && pet.Hunger < 35)
                    ShowSpeech("I'm hungry...");
                else if (!sleeping && pet.Energy < 25)
                    ShowSpeech("I need a rest.");
            }
            catch (Exception ex)
            {
                RecoverFromError("pet timer", ex);
            }
        }

        private PetState LoadPet()
        {
            try
            {
                var value = ApplicationData.Current.LocalSettings.Values[SaveKey] as string;
                if (string.IsNullOrWhiteSpace(value))
                    return new PetState();

                var loaded = PetCodec.Decode(value);
                NormalizePet(loaded);
                return loaded;
            }
            catch (Exception ex)
            {
                RecoverFromError("save load", ex);
                return new PetState();
            }
        }

        private void NormalizePet(PetState value)
        {
            if (value == null)
                return;

            value.Name = string.IsNullOrWhiteSpace(value.Name) ? "Blinky" : value.Name.Trim();
            value.Level = Math.Max(1, value.Level);
            value.Experience = Math.Max(0, value.Experience);
            value.Hunger = Clamp(value.Hunger, 0, 100);
            value.Energy = Clamp(value.Energy, 0, 100);
            value.Happiness = Clamp(value.Happiness, 0, 100);
            value.Bond = Clamp(value.Bond, 0, 100);
            value.Curiosity = Clamp(value.Curiosity, 0, 100);
            value.Strength = Clamp(value.Strength, 0, 99);
            value.Speed = Clamp(value.Speed, 0, 99);
            value.Coins = Math.Max(0, value.Coins);
            value.AgeDays = Math.Max(0, value.AgeDays);
            value.WorldX = Math.Max(0, value.WorldX);
            value.WorldY = Math.Max(0, value.WorldY);
        }

        private void SavePet()
        {
            try
            {
                if (pet == null)
                    return;

                NormalizePet(pet);
                ApplicationData.Current.LocalSettings.Values[SaveKey] = PetCodec.Encode(pet);
            }
            catch (Exception ex)
            {
                RecoverFromError("save", ex);
            }
        }

        private void UpdateUi()
        {
            if (pet == null)
                return;

            PetNameText.Text = string.IsNullOrWhiteSpace(pet.Name) ? "Blinky" : pet.Name;
            LevelText.Text = "LEVEL " + pet.Level;
            MoodText.Text = (pet.Mood ?? "CONTENT").ToUpperInvariant();
            CoinText.Text = pet.Coins + " COINS";
            StatusMiniText.Text = "BOND " + pet.Bond + "%";

            HungerBar.Value = pet.Hunger;
            EnergyBar.Value = pet.Energy;
            HappinessBar.Value = pet.Happiness;

            FoodHint.Text = pet.Hunger < 40 ? "LOW" : pet.Hunger > 85 ? "FULL" : "OK";
        }

        private void Pet_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (busyReaction || sleeping)
                    return;

                busyReaction = true;
                reactionCount++;

                if (reactionCount % 3 == 0)
                    ShowSpeech("Heehee!");
                else if (reactionCount % 2 == 0)
                    ShowSpeech("Again!");
                else
                    ShowSpeech("That tickles!");

                pet.Happiness = Clamp(pet.Happiness + 2, 0, 100);
                pet.Bond = Clamp(pet.Bond + 1, 0, 100);

                SafeAnimate(PetReactionStoryboard);
                SavePet();
                UpdateUi();

                var release = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(420) };
                release.Tick += (s, a) =>
                {
                    try
                    {
                        release.Stop();
                        busyReaction = false;
                    }
                    catch
                    {
                        busyReaction = false;
                    }
                };
                release.Start();
            }
            catch (Exception ex)
            {
                busyReaction = false;
                RecoverFromError("pet tap", ex);
            }
        }

        private void Page_Tapped(object sender, RoutedEventArgs e)
        {
            // Room taps are intentionally passive.
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sleeping = false;
                if (pet.Energy < 8)
                {
                    ShowSpeech("Too tired!");
                    MessageText.Text = "Your pet needs a rest before playing.";
                    return;
                }

                pet.Play();
                pet.Happiness = Clamp(pet.Happiness + 3, 0, 100);
                ShowSpeech("Let's play!");
                MessageText.Text = "You played together. Happiness and bond went up.";
                SafeAnimate(ActionBounceStoryboard);
                SavePet();
                UpdateUi();
            }
            catch (Exception ex) { RecoverFromError("play", ex); }
        }

        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sleeping = false;
                pet.Feed();
                ShowSpeech(pet.Hunger > 75 ? "Yum!" : "More, please!");
                MessageText.Text = "Your pet happily ate.";
                SafeAnimate(ActionBounceStoryboard);
                SavePet();
                UpdateUi();
            }
            catch (Exception ex) { RecoverFromError("feed", ex); }
        }

        private void Train_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sleeping = false;
                pet.Train();
                ShowSpeech("I can do it!");
                MessageText.Text = "Training made your pet stronger and earned experience.";
                SafeAnimate(ActionBounceStoryboard);
                SavePet();
                UpdateUi();
            }
            catch (Exception ex) { RecoverFromError("train", ex); }
        }

        private void Sleep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sleeping = true;
                pet.Sleep();
                ShowSpeech("Night night...");
                MessageText.Text = "Your pet is sleeping.";
                SavePet();
                UpdateUi();
            }
            catch (Exception ex) { RecoverFromError("sleep", ex); }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SavePet();
                ShowSpeech("Saved!");
                MessageText.Text = "PetWorld saved your pet.";
            }
            catch (Exception ex) { RecoverFromError("manual save", ex); }
        }

        private async void Explore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sleeping = false;
                string result = pet.Explore();
                ShowSpeech("Let's go!");
                SafeAnimate(ActionBounceStoryboard);
                SavePet();
                UpdateUi();

                await new ContentDialog
                {
                    Title = "DISCOVERY",
                    Content = result,
                    PrimaryButtonText = "KEEP GOING"
                }.ShowAsync();
            }
            catch (Exception ex) { RecoverFromError("explore", ex); }
        }

        private void Adventure_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SavePet();
                Frame.Navigate(typeof(AdventurePage));
            }
            catch (Exception ex) { RecoverFromError("adventure navigation", ex); }
        }

        private async void Stats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string content =
                    "Species: " + pet.Species + "\n" +
                    "Level: " + pet.Level + "\n" +
                    "XP: " + pet.Experience + "\n" +
                    "Hunger: " + pet.Hunger + "\n" +
                    "Energy: " + pet.Energy + "\n" +
                    "Happiness: " + pet.Happiness + "\n" +
                    "Bond: " + pet.Bond + "\n" +
                    "Curiosity: " + pet.Curiosity + "\n" +
                    "Strength: " + pet.Strength + "\n" +
                    "Speed: " + pet.Speed;

                await new ContentDialog
                {
                    Title = "PET PROFILE",
                    Content = content,
                    PrimaryButtonText = "CLOSE"
                }.ShowAsync();
            }
            catch (Exception ex) { RecoverFromError("profile", ex); }
        }

        private async void Link_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string myCode = PetCodec.Encode(pet);

                var copyButton = new Button
                {
                    Content = "COPY MY PET CODE",
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                copyButton.Click += (s, args) =>
                {
                    try
                    {
                        var outgoing = new DataPackage();
                        outgoing.SetText(myCode);
                        Clipboard.SetContent(outgoing);
                        copyButton.Content = "COPIED!";
                    }
                    catch (Exception ex)
                    {
                        RecoverFromError("clipboard", ex);
                    }
                };

                var input = new TextBox
                {
                    AcceptsReturn = true,
                    TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                    PlaceholderText = "Paste a friend's PetWorld code here"
                };

                var content = new StackPanel();
                content.Children.Add(new TextBlock
                {
                    Text = "Send your pet code to another player, or paste one here to let your pets meet.",
                    TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                content.Children.Add(copyButton);
                content.Children.Add(input);

                var dialog = new ContentDialog
                {
                    Title = "PET FRIENDS",
                    Content = content,
                    PrimaryButtonText = "VISIT FRIEND",
                    SecondaryButtonText = "CLOSE"
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary || string.IsNullOrWhiteSpace(input.Text))
                    return;

                PetState friend = PetCodec.Decode(input.Text);
                NormalizePet(friend);

                pet.ReceiveFriendVisit();
                ShowSpeech("A friend!");
                MessageText.Text = friend.Name + " came to visit.";
                SavePet();
                UpdateUi();
            }
            catch (Exception ex)
            {
                RecoverFromError("friends", ex);
            }
        }

        private void ShowSpeech(string text)
        {
            try
            {
                SpeechText.Text = string.IsNullOrWhiteSpace(text) ? "Hi!" : text;
                SpeechBubble.Opacity = 1;
                SafeAnimate(BubbleStoryboard);
            }
            catch (Exception ex)
            {
                RecoverFromError("speech bubble", ex);
            }
        }

        private void SafeAnimate(Storyboard storyboard)
        {
            try
            {
                if (storyboard != null)
                    storyboard.Begin();
            }
            catch (Exception ex)
            {
                RecoverFromError("animation", ex);
            }
        }

        private void RecoverFromError(string area, Exception ex)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["PetWorld.LastError"] =
                    DateTime.UtcNow.ToString("o") + " | " + area + " | " + ex.Message;
            }
            catch
            {
                // Logging must never become another crash.
            }

            try
            {
                busyReaction = false;
                if (MessageText != null)
                    MessageText.Text = "PetWorld recovered from a small error.";
            }
            catch
            {
                // Keep recovery silent if the visual tree is not available.
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
