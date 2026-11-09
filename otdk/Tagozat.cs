using otdk.Enums;
using System.Data;

namespace otdk;

public class Tagozat
{
    public TagozatAllapot MyProperty { get; private set; }

    public int PresentationCount { get; private set; } = 1;
    
    public List<Hallgato> MyProperty { get; set; }
}
