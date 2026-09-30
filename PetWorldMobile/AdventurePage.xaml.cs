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
            pet = LoadPet();
            GenerateMap();
            RenderMap();
            UpdateUi();
        }

        private PetState LoadPet()
        {
            try
            {
                var code = ApplicationData.Current.LocalSettings.Values[SaveKey] as string;
                return string.IsNullOrWhiteSpace(code) ? new PetState() : PetCodec.Decode(code);
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

        private void GenerateMap()
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                    terrain[x, y] = 0;
            }

            int pickups = 7;
            while (pickups > 0)
            {
                int x = random.Next(Size);
                int y = random.Next(Size);
                if ((x == pet.WorldX && y == pet.WorldY) || terrain[x, y] != 0) continue;
                terrain[x, y] = random.Next(1, 4);
                pickups--;
            }

            terrain[0, 0] = 3;
        }

        private void RenderMap()
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
                    var tile = new Border
                    {
                        Margin = new Thickness(2),
                        CornerRadius = new CornerRadius(8),
                        Background = new SolidColorBrush(Color.FromArgb(255, 33, 61, 66))
                    };

                    var label = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 18,
                        Foreground = new SolidColorBrush(Colors.White),
                        Text = terrain[x, y] == 1 ? "★" : terrain[x, y] == 2 ? "◆" : terrain[x, y] == 3 ? "!" : ""
                    };

                    if (x == pet.WorldX && y == pet.WorldY)
                    {
                        tile.Background = new SolidColorBrush(Color.FromArgb(255, 83, 199, 151));
                        label.Text = "PET";
                        label.FontSize = 12;
                    }

                    tile.Child = label;
                    Grid.SetColumn(tile, x);
                    Grid.SetRow(tile, y);
                    MapGrid.Children.Add(tile);
                }
            }
        }

        private void UpdateUi()
        {
            InfoText.Text = pet.Name + " • Energy " + pet.Energy + " • Happiness " + pet.Happiness +
                             " • Position " + pet.WorldX + "," + pet.WorldY;
        }

        private void Move(int dx, int dy)
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
                MessageText.Text = "A little boundary keeps your pet in the meadow.";
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

        private void AutoCollect()
        {
            int item = terrain[pet.WorldX, pet.WorldY];
            if (item == 0) return;

            terrain[pet.WorldX, pet.WorldY] = 0;

            if (item == 1)
            {
                pet.Coins += 12;
                pet.Happiness = Math.Min(100, pet.Happiness + 4);
                MessageText.Text = "You found a shiny token! +12 coins.";
            }
            else if (item == 2)
            {
                pet.Energy = Math.Min(100, pet.Energy + 10);
                pet.Happiness = Math.Min(100, pet.Happiness + 6);
                MessageText.Text = "You found a fruit! +10 energy.";
            }
            else
            {
                pet.Coins += 30;
                pet.Bond = Math.Min(100, pet.Bond + 3);
                MessageText.Text = "A mystery marker led to a treasure! +30 coins and +3 bond.";
            }
        }

        private void Act_Click(object sender, RoutedEventArgs e)
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
                pet.Happiness = Math.Min(100, pet.Happiness + 5);
                pet.Bond = Math.Min(100, pet.Bond + 2);
                MessageText.Text = "Your pet discovered a hidden path and found treasure!";
            }
            else
            {
                pet.Curiosity = Math.Min(100, pet.Curiosity + 8);
                MessageText.Text = "Your pet sniffed around and learned something new.";
            }

            SavePet();
            RenderMap();
            UpdateUi();
        }

        private void Rest_Click(object sender, RoutedEventArgs e)
        {
            pet.Sleep();
            SavePet();
            MessageText.Text = "Your pet curled up under a tree and rested.";
            RenderMap();
            UpdateUi();
        }

        private void Up_Click(object sender, RoutedEventArgs e) { Move(0, -1); }
        private void Down_Click(object sender, RoutedEventArgs e) { Move(0, 1); }
        private void Left_Click(object sender, RoutedEventArgs e) { Move(-1, 0); }
        private void Right_Click(object sender, RoutedEventArgs e) { Move(1, 0); }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }
}
