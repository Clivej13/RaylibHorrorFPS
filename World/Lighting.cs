using Raylib_cs;

namespace DungeonCrawler.World;

public enum LightColour
{
    Red,
    Green,
    White,
    Yellow
}

public enum PowerState
{
    Emergency,
    Online,
    Always
}

public sealed class Light
{
    public required string Id { get; init; }
    public int TileX { get; init; }
    public int TileY { get; init; }
    public float Radius { get; init; }
    public LightColour Colour { get; init; }
    public bool Enabled { get; set; } = true;
    public PowerState EnabledWhen { get; init; } = PowerState.Always;

    public bool IsActive(PowerState currentPowerState)
        => Enabled && (EnabledWhen == PowerState.Always || EnabledWhen == currentPowerState);

    public Color ToRaylibColor() => Colour switch
    {
        LightColour.Red => Color.Red,
        LightColour.Green => Color.Green,
        LightColour.White => Color.White,
        LightColour.Yellow => Color.Yellow,
        _ => Color.White
    };
}
