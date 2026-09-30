using System;
using System.Security.Cryptography;
using System.Text;

namespace PetWorld.Core
{
    public static class PetCodec
    {
        public static string Encode(PetState p)
        {
            string raw = string.Join("|",
                "PW1", Esc(p.Name), Esc(p.Species),
                p.Level, p.Experience, p.Hunger, p.Energy, p.Happiness, p.Bond,
                p.Curiosity, p.Strength, p.Speed, p.Coins, p.AgeDays, p.WorldX, p.WorldY);
            string checksum = Sha(raw).Substring(0, 8);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw + "|" + checksum));
        }

        public static PetState Decode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Empty pet code.");
            string raw;
            try { raw = Encoding.UTF8.GetString(Convert.FromBase64String(code.Trim())); }
            catch { throw new FormatException("Pet code is not valid base64."); }

            string[] a = raw.Split('|');
            if (a.Length != 17 || a[0] != "PW1") throw new FormatException("Unknown pet code.");
            string withoutChecksum = string.Join("|", a, 0, 16);
            if (!string.Equals(Sha(withoutChecksum).Substring(0, 8), a[16], StringComparison.OrdinalIgnoreCase))
                throw new FormatException("Pet code failed its checksum.");

            int i = 1;
            var p = new PetState
            {
                Name = Unesc(a[i++]), Species = Unesc(a[i++]),
                Level = N(a[i++]), Experience = N(a[i++]), Hunger = N(a[i++]),
                Energy = N(a[i++]), Happiness = N(a[i++]), Bond = N(a[i++]),
                Curiosity = N(a[i++]), Strength = N(a[i++]), Speed = N(a[i++]),
                Coins = N(a[i++]), AgeDays = N(a[i++]), WorldX = N(a[i++]), WorldY = N(a[i++])
            };
            return p;
        }

        private static int N(string s) => int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        private static string Esc(string s) => Uri.EscapeDataString(s ?? "");
        private static string Unesc(string s) => Uri.UnescapeDataString(s ?? "");

        private static string Sha(string s)
        {
            using (var h = SHA256.Create())
            {
                byte[] hash = h.ComputeHash(Encoding.UTF8.GetBytes(s));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
