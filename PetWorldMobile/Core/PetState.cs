using System;

namespace PetWorld.Core
{
    public sealed class PetState
    {
        public string Name { get; set; } = "Blinky";
        public string Species { get; set; } = "Sproutling";
        public int Level { get; set; } = 1;
        public int Experience { get; set; }
        public int Hunger { get; set; } = 72;
        public int Energy { get; set; } = 88;
        public int Happiness { get; set; } = 80;
        public int Bond { get; set; } = 12;
        public int Curiosity { get; set; } = 55;
        public int Strength { get; set; } = 10;
        public int Speed { get; set; } = 10;
        public int Coins { get; set; } = 40;
        public int AgeDays { get; set; }
        public int WorldX { get; set; }
        public int WorldY { get; set; }
        public string Mood => CalculateMood();

        public void Tick(int minutes)
        {
            if (minutes <= 0) return;
            Hunger = Clamp(Hunger - Math.Max(1, minutes / 30), 0, 100);
            Energy = Clamp(Energy - Math.Max(1, minutes / 45), 0, 100);
            Happiness = Clamp(Happiness - Math.Max(0, minutes / 90), 0, 100);
            if (minutes >= 1440) AgeDays += minutes / 1440;
            if (Hunger < 20) Happiness = Clamp(Happiness - 2, 0, 100);
            if (Energy < 15) Happiness = Clamp(Happiness - 1, 0, 100);
        }

        public void Feed()
        {
            Hunger = Clamp(Hunger + 24, 0, 100);
            Happiness = Clamp(Happiness + 5, 0, 100);
            Bond = Clamp(Bond + 2, 0, 100);
            GainXp(5);
        }

        public void Play()
        {
            if (Energy < 12) return;
            Energy = Clamp(Energy - 12, 0, 100);
            Happiness = Clamp(Happiness + 14, 0, 100);
            Bond = Clamp(Bond + 3, 0, 100);
            Curiosity = Clamp(Curiosity + 2, 0, 100);
            GainXp(10);
        }

        public void Train()
        {
            if (Energy < 20) return;
            Energy = Clamp(Energy - 20, 0, 100);
            Strength = Clamp(Strength + 1, 0, 99);
            if (Curiosity >= 60) Speed = Clamp(Speed + 1, 0, 99);
            Happiness = Clamp(Happiness + 4, 0, 100);
            GainXp(18);
        }

        public void Sleep()
        {
            Energy = Clamp(Energy + 45, 0, 100);
            Hunger = Clamp(Hunger - 4, 0, 100);
            Happiness = Clamp(Happiness + 4, 0, 100);
            GainXp(2);
        }

        public string Explore()
        {
            if (Energy < 18) return "Too tired to explore.";
            Energy = Clamp(Energy - 18, 0, 100);
            Curiosity = Clamp(Curiosity + 6, 0, 100);
            Happiness = Clamp(Happiness + 8, 0, 100);
            Bond = Clamp(Bond + 1, 0, 100);
            int reward = 6 + (Curiosity / 20);
            Coins += reward;
            GainXp(14);
            return "Adventure found " + reward + " coins!";
        }

        public void ReceiveFriendVisit()
        {
            Happiness = Clamp(Happiness + 7, 0, 100);
            Bond = Clamp(Bond + 2, 0, 100);
            GainXp(8);
        }

        private void GainXp(int amount)
        {
            Experience += amount;
            while (Experience >= Level * 40)
            {
                Experience -= Level * 40;
                Level++;
                Strength = Clamp(Strength + 1, 0, 99);
                Speed = Clamp(Speed + 1, 0, 99);
                Happiness = Clamp(Happiness + 10, 0, 100);
            }

            if (Level >= 12) Species = "Aurora";
            else if (Level >= 6) Species = "Glimmer";
        }

        private string CalculateMood()
        {
            if (Hunger < 20) return "Hungry";
            if (Energy < 20) return "Sleepy";
            if (Happiness >= 85) return "Joyful";
            if (Curiosity >= 75) return "Curious";
            if (Bond >= 70) return "Loyal";
            return "Content";
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
