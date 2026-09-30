using PetWorld.Core;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Shapes;

namespace PetWorldMobile
{
    public sealed partial class MainPage : Page
    {
        private PetState pet;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private bool sleeping;
        private const string SaveKey = "PetWorld.Save";

        public MainPage()
        {
            InitializeComponent();
            pet = LoadPet();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += OnTimerTick;
            timer.Start();
            UpdateUi();
        }

        private void OnTimerTick(object sender, object e)
        {
            pet.Tick(1);
            SavePet();
            UpdateUi();
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
            PetNameText.Text = pet.Name + (sleeping ? "  zZ" : "");
            StatusText.Text = "Lv " + pet.Level + " • " + pet.Mood + " • Happiness " + pet.Happiness + "%";
            CoinText.Text = "Coins " + pet.Coins;
            SpeciesText.Text = pet.Species;
            MoodBubble.Text = sleeping ? "Zzz..." : pet.Mood + "!";
            DrawPet();
        }

        private void DrawPet()
        {
            double w = Math.Max(200, WorldCanvas.ActualWidth);
            double h = Math.Max(240, WorldCanvas.ActualHeight);
            double cx = w / 2.0;
            double cy = h * 0.54;
            double size = Math.Min(w, h) * 0.27;

            Canvas.SetLeft(Shadow, cx - size * 0.72);
            Canvas.SetTop(Shadow, cy + size * 0.48);
            Shadow.Width = size * 1.44;
            Shadow.Height = size * 0.28;

            Canvas.SetLeft(Body, cx - size);
            Canvas.SetTop(Body, cy - size * 0.78);
            Body.Width = size * 2;
            Body.Height = size * 1.56;

            Canvas.SetLeft(Belly, cx - size * 0.62);
            Canvas.SetTop(Belly, cy - size * 0.02);
            Belly.Width = size * 1.24;
            Belly.Height = size * 0.95;

            Canvas.SetLeft(Eye1, cx - size * 0.42);
            Canvas.SetTop(Eye1, cy - size * 0.28);
            Eye1.Width = size * 0.22;
            Eye1.Height = size * 0.30;

            Canvas.SetLeft(Eye2, cx + size * 0.20);
            Canvas.SetTop(Eye2, cy - size * 0.28);
            Eye2.Width = size * 0.22;
            Eye2.Height = size * 0.30;

            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Windows.Foundation.Point(cx - size * 0.18, cy + size * 0.18),
                IsClosed = false,
                IsFilled = false
            };
            figure.Segments.Add(new ArcSegment
            {
                Point = new Windows.Foundation.Point(cx + size * 0.18, cy + size * 0.18),
                Size = new Windows.Foundation.Size(size * 0.36, size * 0.25),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = false
            });
            geometry.Figures.Add(figure);
            Smile.Data = geometry;

            Ear1.Points = new PointCollection
            {
                new Windows.Foundation.Point(cx - size * 0.70, cy - size * 0.62),
                new Windows.Foundation.Point(cx - size * 0.96, cy - size * 1.18),
                new Windows.Foundation.Point(cx - size * 0.32, cy - size * 0.86)
            };
            Ear2.Points = new PointCollection
            {
                new Windows.Foundation.Point(cx + size * 0.70, cy - size * 0.62),
                new Windows.Foundation.Point(cx + size * 0.96, cy - size * 1.18),
                new Windows.Foundation.Point(cx + size * 0.32, cy - size * 0.86)
            };

            MoodBubble.Measure(new Windows.Foundation.Size(w, h));
            Canvas.SetLeft(MoodBubble, Math.Min(w - MoodBubble.DesiredSize.Width - 10, cx + size * 0.50));
            Canvas.SetTop(MoodBubble, Math.Max(10, cy - size * 1.15));
        }

        private void WorldCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawPet();

        private void Feed_Click(object sender, RoutedEventArgs e) { sleeping = false; pet.Feed(); SavePet(); UpdateUi(); }
        private void Play_Click(object sender, RoutedEventArgs e) { sleeping = false; pet.Play(); SavePet(); UpdateUi(); }
        private void Train_Click(object sender, RoutedEventArgs e) { sleeping = false; pet.Train(); SavePet(); UpdateUi(); }
        private void Sleep_Click(object sender, RoutedEventArgs e) { sleeping = true; pet.Sleep(); SavePet(); UpdateUi(); }
        private void Save_Click(object sender, RoutedEventArgs e) { SavePet(); MoodBubble.Text = "Saved!"; }

        private async void Explore_Click(object sender, RoutedEventArgs e)
        {
            sleeping = false;
            string result = pet.Explore();
            SavePet();
            UpdateUi();
            await new ContentDialog
            {
                Title = "Adventure",
                Content = result,
                PrimaryButtonText = "Nice!"
            }.ShowAsync();
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
                Title = "Pet Stats",
                Content = content,
                PrimaryButtonText = "Close"
            }.ShowAsync();
        }

        private async void Link_Click(object sender, RoutedEventArgs e)
        {
            string myCode = PetCodec.Encode(pet);

            var copyButton = new Button { Content = "COPY MY PET CODE", HorizontalAlignment = HorizontalAlignment.Left };
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
                TextWrapping = TextWrapping.Wrap,
                PlaceholderText = "Paste a friend's PetWorld code here"
            };

            var content = new StackPanel { Spacing = 8 };
            content.Children.Add(new TextBlock
            {
                Text = "Your code is ready to send. To visit a friend, paste their code below. Visits give your pet a bond and happiness boost without replacing your pet.",
                TextWrapping = TextWrapping.Wrap
            });
            content.Children.Add(copyButton);
            content.Children.Add(input);

            var dialog = new ContentDialog
            {
                Title = "Pet Link / Visit",
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
                SavePet();
                UpdateUi();

                await new ContentDialog
                {
                    Title = "Friend Visit!",
                    Content = friend.Name + " the " + friend.Species + " visited your pet. " +
                              "Your bond and happiness increased!",
                    PrimaryButtonText = "YAY!"
                }.ShowAsync();
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "Invalid Pet Code",
                    Content = ex.Message,
                    PrimaryButtonText = "Close"
                }.ShowAsync();
            }
        }
    }
}
