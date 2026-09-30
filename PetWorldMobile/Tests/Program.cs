using PetWorld.Core;

static void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
}

var p = new PetState { Name = "Test Pet" };
var code = PetCodec.Encode(p);
Check(!string.IsNullOrWhiteSpace(code), "encode creates code");
var q = PetCodec.Decode(code);
Check(q.Name == "Test Pet" && q.Level == 1 && q.Hunger == 72, "round trip");
q.Feed();
Check(q.Hunger > 72 && q.Bond > 12, "feeding");
var before = q.Happiness;
q.Play();
Check(q.Happiness >= before, "playing");
for (int i = 0; i < 30; i++) q.Train();
Check(q.Level > 1, "leveling");
var oldEnergy = q.Energy;
q.Sleep();
Check(q.Energy >= oldEnergy, "sleep");
var oldLevel = q.Level;
q.Explore();
Check(q.Level >= oldLevel, "exploration");

Console.WriteLine("ALL PETWORLD CORE TESTS PASSED");
