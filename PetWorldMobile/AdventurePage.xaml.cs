using PetWorld.Core;
using System;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PetWorldMobile
{
    public sealed partial class AdventurePage : Page
    {
        private const string SaveKey = "PetWorld.Save";
        private const int Size = 7;

        private PetState pet;
        private readonly int[,] terrain = new int[Size, Size];
        private readonly Random random = new Random();

        public AdventurePage()
        {
            InitializeComponent();

            try
            {
                pet = LoadPet();
                GenerateMap();
                RenderMap();
                UpdateUi();
            }
            catch (Exception ex)
            {
                pet = pet ?? new PetState();
                MessageText.Text = "Adventure recovered. Please try again.";
                RecordError("adventure startup", ex);
            }
        }

        private PetState LoadPet()
        {
            try
            {
                var code = ApplicationData.Current.LocalSettings.Values[SaveKey] as string;
                if (string.IsNullOrWhiteSpace(code))
                    return new PetState();

                var value = PetCodec.Decode(code);
                NormalizePet(value);
                return value;
            }
            catch (Exception ex)
            {
                RecordError("adventure save load", ex);
                return new PetState();
            }
        }

        private void SavePet()
        {
            try
            {
                NormalizePet(pet);
                ApplicationData.Current.LocalSettings.Values[SaveKey] = PetCodec.Encode(pet);
            }
            catch (Exception ex)
            {
                RecordError("adventure save", ex);
            }
        }

        private static void NormalizePet(PetState value)
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
            value.WorldX = Math.Max(0, value.WorldX);
            value.WorldY = Math.Max(0, value.WorldY);
        }

        private void GenerateMap()
        {
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    terrain[x, y] = 0;

            int pickups = 9;
            while (pickups > 0)
            {
                int x = random.Next(Size);
                int y = random.Next(Size);

                if ((x == pet.WorldX && y == pet.WorldY) || terrain[x, y] != 0)
                    continue;

                terrain[x, y] = random.Next(1, 4);
                pickups--;
            }

            terrain[0, 0] = 3;
        }

        private void RenderMap()
        {
            try
            {
                MapGrid.Children.Clear();
                MapGrid.RowDefinitions.Clear();
                MapGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < Size; i++)
                {
                    MapGrid.RowDefinitions.Add(new RowDefinition());
                    MapGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (int y = 0; y < Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        int item = terrain[x, y];

                        var tile = new Border
                        {
                            Margin = new Thickness(2),
                            CornerRadius = new CornerRadius(8),
                            Background = new SolidColorBrush(
                                item == 0
                                ? Color.FromArgb(255, 33, 56, 66)
                                : item == 1
                                ? Color.FromArgb(255, 51, 77, 73)
                                : item == 2
                                ? Color.FromArgb(255, 54, 68, 95)
                                : Color.FromArgb(255, 77, 62, 92))
                        };

                        var label = new TextBlock
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = item == 0 ? 9 : 18,
                            Foreground = new SolidColorBrush(Colors.White),
                            Text = item == 1 ? "★" : item == 2 ? "◆" : item == 3 ? "!" : "·"
                        };

                        if (x == pet.WorldX && y == pet.WorldY)
                        {
                            tile.Background = new SolidColorBrush(Color.FromArgb(255, 72, 190, 139));
                            label.Text = "YOU";
                            label.FontSize = 10;
                            label.FontWeight = Windows.UI.Text.FontWeights.Bold;
                        }

                        tile.Child = label;
                        Grid.SetColumn(tile, x);
                        Grid.SetRow(tile, y);
                        MapGrid.Children.Add(tile);
                    }
                }
            }
            catch (Exception ex)
            {
                RecordError("map render", ex);
            }
        }

        private void UpdateUi()
        {
            try
            {
                if (pet == null)
                    return;

                string area = GetAreaName(pet.WorldX, pet.WorldY);

                TitleText.Text = area;
                InfoText.Text = pet.Name + " • " + area + " • " + pet.WorldX + "," + pet.WorldY;
                EnergyText.Text = pet.Energy.ToString();
                HappinessText.Text = pet.Happiness.ToString();
                CoinsText.Text = pet.Coins.ToString();
            }
            catch (Exception ex)
            {
                RecordError("adventure ui", ex);
            }
        }

        private string GetAreaName(int x, int y)
        {
            if (x <= 1 && y <= 1) return "Moonlit Grove";
            if (x >= 5 && y <= 2) return "Sunny Ridge";
            if (x <= 2 && y >= 4) return "Whisper Woods";
            if (x >= 4 && y >= 4) return "Crystal Meadow";
            return "Meadow Trail";
        }

        private void Move(int dx, int dy)
        {
            try
            {
                if (pet.Energy < 5)
                {
                    MessageText.Text = "Your pet is too tired. Use REST.";
                    return;
                }

                int nx = Math.Max(0, Math.Min(Size - 1, pet.WorldX + dx));
                int ny = Math.Max(0, Math.Min(Size - 1, pet.WorldY + dy));

                if (nx == pet.WorldX && ny == pet.WorldY)
                {
                    MessageText.Text = "You reached the edge of this trail.";
                    return;
                }

                pet.WorldX = nx;
                pet.WorldY = ny;
                pet.Energy = Math.Max(0, pet.Energy - 5);
                pet.Curiosity = Math.Min(100, pet.Curiosity + 1);

                AutoCollect();
                SavePet();
                RenderMap();
                UpdateUi();
            }
            catch (Exception ex)
            {
                RecordError("move", ex);
                MessageText.Text = "The trail recovered. Try moving again.";
            }
        }

        private void AutoCollect()
        {
            int item = terrain[pet.WorldX, pet.WorldY];
            if (item == 0)
                return;

            terrain[pet.WorldX, pet.WorldY] = 0;

            if (item == 1)
            {
                pet.Coins += 12;
                pet.Happiness = Clamp(pet.Happiness + 4, 0, 100);
                MessageText.Text = "A shiny token! +12 coins.";
            }
            else if (item == 2)
            {
                pet.Energy = Clamp(pet.Energy + 10, 0, 100);
                pet.Happiness = Clamp(pet.Happiness + 6, 0, 100);
                MessageText.Text = "A wild fruit! +10 energy.";
            }
            else
            {
                pet.Coins += 30;
                pet.Bond = Clamp(pet.Bond + 3, 0, 100);
                MessageText.Text = "Treasure found! +30 coins and +3 bond.";
            }
        }

        private void Act_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pet.Energy < 8)
                {
                    MessageText.Text = "Too tired to investigate.";
                    return;
                }

                pet.Energy -= 8;

                if (random.Next(100) < 45)
                {
                    pet.Coins += 15 + random.Next(16);
                    pet.Happiness = Clamp(pet.Happiness + 5, 0, 100);
                    pet.Bond = Clamp(pet.Bond + 2, 0, 100);
                    MessageText.Text = "A hidden path revealed treasure!";
                }
                else
                {
                    pet.Curiosity = Clamp(pet.Curiosity + 8, 0, 100);
                    MessageText.Text = "Your pet sniffed around and learned something new.";
                }

                SavePet();
                RenderMap();
                UpdateUi();
            }
            catch (Exception ex)
            {
                RecordError("investigate", ex);
                MessageText.Text = "Your pet paused for a moment. Try again.";
            }
        }

        private void Rest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pet.Sleep();
                SavePet();
                MessageText.Text = "Your pet curled up under the stars and rested.";
                RenderMap();
                UpdateUi();
            }
            catch (Exception ex)
            {
                RecordError("adventure rest", ex);
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e) { Move(0, -1); }
        private void Down_Click(object sender, RoutedEventArgs e) { Move(0, 1); }
        private void Left_Click(object sender, RoutedEventArgs e) { Move(-1, 0); }
        private void Right_Click(object sender, RoutedEventArgs e) { Move(1, 0); }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SavePet();
                if (Frame.CanGoBack)
                    Frame.GoBack();
                else
                    Frame.Navigate(typeof(MainPage));
            }
            catch (Exception ex)
            {
                RecordError("back navigation", ex);
            }
        }

        private void RecordError(string area, Exception ex)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["PetWorld.LastError"] =
                    DateTime.UtcNow.ToString("o") + " | " + area + " | " + ex.Message;
            }
            catch
            {
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
