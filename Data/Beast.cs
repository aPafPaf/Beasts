namespace Beasts.Data;

public class Beast
{
    public string Path;
    public string DisplayName;
    public string[] Crafts;

    /// <summary>
    /// Talisman base type that can drop from this beast (3.29+): talismans only drop from
    /// Red Beasts and Bestiary Bosses, and every talisman base is tied to one specific beast.
    /// Null for beasts without a talisman drop.
    /// </summary>
    public string DropsTalisman;
}
