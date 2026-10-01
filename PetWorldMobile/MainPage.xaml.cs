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
        private PetState pet;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly DispatcherTimer blinkTimer = new DispatcherTimer();
        private bool sleeping;
        private bool busyReaction;
        private int reactionCount;
        private const string SaveKey = "PetWorld.Save";

        public MainPage()
        {
            InitializeComponent();

            pet = LoadPet();

            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += OnTimerTick;
            timer.Start();

            blinkTimer.Interval = TimeSpan.FromSeconds(4);
            blinkTimer.Tick += BlinkTimer_Tick;
            blinkTimer.Start();

            Loaded += MainPage_Loaded;
            UpdateUi();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            IdlePetStoryboard.Begin();
            ShowSpeech("I'm home!");
        }

        private void BlinkTimer_Tick(object sender, object e)
        {
            BlinkStoryboard.Begin();
        }

        private void OnTimerTick(object sender, object e)
        {
            if (sleeping)
                pet.Tick(1);
            else
                pet.Tick(1);

            SavePet();
            UpdateUi();

            if (!sleeping && pet.Hunger < 35)
                ShowSpeech("I'm hungry...");
            else if (!sleeping && pet.Energy < 25)
                ShowSpeech("I need a rest.");
        }

        private PetState LoadPet()
        {
            try
            {
                var value = ApplicationData.Current.LocalSettings.Values[SaveKey] as string;
                return string.IsNullOrWhiteSpace(value) ? new PetState() : PetCodec.Decode(value);
            }
            catch
            {
                return new PetState();
            }
        }

        private void SavePet()
        {
            ApplicationData.Current.LocalSettings.Values[SaveKey] = PetCodec.Encode(pet);
        }

        private void UpdateUi()
        {
            PetNameText.Text = pet.Name;
            LevelText.Text = "LEVEL " + pet.Level;
            MoodText.Text = (pet.Mood ?? "CONTENT").ToUpper();
            CoinText.Text = pet.Coins + " COINS";
            StatusMiniText.Text = "HAPPY " + pet.Happiness + "%";
            FoodHint.Text = pet.Hunger < 40 ? "LOW" : "OK";

            if (sleeping)
            {
                MessageText.Text = "Your pet is curled up and resting.";
            }
        }

        private void Pet_Tapped(object sender, RoutedEventArgs e)
        {
            if (busyReaction)
                return;

            busyReaction = true;
            reactionCount++;

            if (reactionCount % 3 == 0)
                ShowSpeech("Heehee!");
            else if (reactionCount % 2 == 0)
                ShowSpeech("Again!");
            else
                ShowSpeech("That tickles!");

            pet.Happiness = Math.Min(100, pet.Happiness + 2);
            pet.Bond = Math.Min(100, pet.Bond + 1);

            PetReactionStoryboard.Begin();
            SavePet();
            UpdateUi();

            var release = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(420) };
            release.Tick += (s, a) =>
            {
                release.Stop();
                busyReaction = false;
            };
            release.Start();
        }

        private void Page_Tapped(object sender, RoutedEventArgs e)
        {
            // The pet handles its own taps. This keeps the rest of the room passive.
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            sleeping = false;
            if (pet.Energy < 8)
            {
                ShowSpeech("Too tired!");
                MessageText.Text = "Your pet needs a rest before playing.";
                PetReactionStoryboard.Begin();
                return;
            }

            pet.Play();
            pet.Happiness = Math.Min(100, pet.Happiness + 3);
            ShowSpeech("Let's play!");
            MessageText.Text = "You played together. Happiness and bond went up.";
            ActionBounceStoryboard.Begin();
            SavePet();
            UpdateUi();
        }

        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            sleeping = false;
            pet.Feed();
            ShowSpeech(pet.Hunger > 75 ? "Yum!" : "More, please!");
            MessageText.Text = "Your pet happily ate. Keep an eye on hunger during the day.";
            ActionBounceStoryboard.Begin();
            SavePet();
            UpdateUi();
        }

        private void Train_Click(object sender, RoutedEventArgs e)
        {
            sleeping = false;
            pet.Train();
            ShowSpeech("I can do it!");
            MessageText.Text = "Training made your pet stronger and earned experience.";
            ActionBounceStoryboard.Begin();
            SavePet();
            UpdateUi();
        }

        private void Sleep_Click(object sender, RoutedEventArgs e)
        {
            sleeping = true;
            pet.Sleep();
            ShowSpeech("Night night...");
            MessageText.Text = "Your pet is sleeping.";
            SavePet();
            UpdateUi();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SavePet();
            ShowSpeech("Saved!");
            MessageText.Text = "PetWorld saved your pet.";
        }

        private async void Explore_Click(object sender, RoutedEventArgs e)
        {
            sleeping = false;
            string result = pet.Explore();
            ShowSpeech("Let's go!");
            ActionBounceStoryboard.Begin();
            SavePet();
            UpdateUi();

            await new ContentDialog
            {
                Title = "DISCOVERY",
                Content = result,
                PrimaryButtonText = "KEEP GOING"
            }.ShowAsync();
        }

        private void Adventure_Click(object sender, RoutedEventArgs e)
        {
            SavePet();
            Frame.Navigate(typeof(AdventurePage));
        }

        private async void Stats_Click(object sender, RoutedEventArgs e)
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

        private async void Link_Click(object sender, RoutedEventArgs e)
        {
            string myCode = PetCodec.Encode(pet);

            var copyButton = new Button
            {
                Content = "COPY MY PET CODE",
                HorizontalAlignment = HorizontalAlignment.Left
            };

            copyButton.Click += (s, args) =>
            {
                var outgoing = new DataPackage();
                outgoing.SetText(myCode);
                Clipboard.SetContent(outgoing);
                copyButton.Content = "COPIED!";
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

            try
            {
                PetState friend = PetCodec.Decode(input.Text);
                pet.ReceiveFriendVisit();
                ShowSpeech("A friend!");
                MessageText.Text = friend.Name + " came to visit.";
                SavePet();
                UpdateUi();
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "INVALID PET CODE",
                    Content = ex.Message,
                    PrimaryButtonText = "CLOSE"
                }.ShowAsync();
            }
        }

        private void ShowSpeech(string text)
        {
            SpeechText.Text = text;
            BubbleStoryboard.Begin();
        }
    }
}