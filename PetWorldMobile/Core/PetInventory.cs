namespace PetWorld.Core
{
    public sealed class PetInventory
    {
        public int Food { get; set; } = 3;
        public int Toys { get; set; } = 1;
        public int Gems { get; set; }

        public void AddFood(int amount) { if (amount > 0) Food += amount; }
        public void AddToy(int amount) { if (amount > 0) Toys += amount; }
        public void AddGem(int amount) { if (amount > 0) Gems += amount; }

        public bool UseFood()
        {
            if (Food <= 0) return false;
            Food--;
            return true;
        }

        public bool UseToy()
        {
            if (Toys <= 0) return false;
            Toys--;
            return true;
        }

        public string Encode()
        {
            return Food + "|" + Toys + "|" + Gems;
        }

        public static PetInventory Decode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new PetInventory();

            string[] parts = value.Split('|');
            if (parts.Length != 3)
                return new PetInventory();

            int food, toys, gems;
            if (!int.TryParse(parts[0], out food) ||
                !int.TryParse(parts[1], out toys) ||
                !int.TryParse(parts[2], out gems))
                return new PetInventory();

            return new PetInventory
            {
                Food = System.Math.Max(0, food),
                Toys = System.Math.Max(0, toys),
                Gems = System.Math.Max(0, gems)
            };
        }

        public string Summary()
        {
            return "Food: " + Food + "\nToys: " + Toys + "\nGems: " + Gems;
        }
    }
}
