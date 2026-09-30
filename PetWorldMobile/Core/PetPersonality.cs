using System;

namespace PetWorld.Core
{
    public enum PetTemperament
    {
        Playful,
        Curious,
        Brave,
        Gentle,
        Energetic
    }

    public sealed class PetPersonality
    {
        public PetTemperament Temperament { get; private set; }
        public int Playfulness { get; private set; }
        public int Curiosity { get; private set; }
        public int Courage { get; private set; }
        public int Gentleness { get; private set; }

        public PetPersonality(PetTemperament temperament = PetTemperament.Curious)
        {
            Temperament = temperament;
            Playfulness = 50;
            Curiosity = 50;
            Courage = 50;
            Gentleness = 50;
        }

        public void LearnFromFeed()
        {
            Gentleness = Clamp(Gentleness + 1);
        }

        public void LearnFromPlay()
        {
            Playfulness = Clamp(Playfulness + 2);
            Curiosity = Clamp(Curiosity + 1);
        }

        public void LearnFromExplore(bool foundSomething)
        {
            Curiosity = Clamp(Curiosity + 3);
            if (foundSomething) Courage = Clamp(Courage + 1);
        }

        public void LearnFromTraining()
        {
            Courage = Clamp(Courage + 2);
        }

        public void LearnFromFriend()
        {
            Gentleness = Clamp(Gentleness + 2);
            Playfulness = Clamp(Playfulness + 1);
        }

        public string Describe()
        {
            if (Temperament == PetTemperament.Playful) return "Playful";
            if (Temperament == PetTemperament.Brave) return "Brave";
            if (Temperament == PetTemperament.Gentle) return "Gentle";
            if (Temperament == PetTemperament.Energetic) return "Energetic";
            return "Curious";
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(100, value));
        }
    }
}
