using System.Text.Json;

namespace DungeonCrawler.World;

public sealed class LevelMetadata
{
    public required string MapImage { get; init; }
    public required List<EnemySpawnData> Enemies { get; init; }
    public required List<KeySpawnData> Keys { get; init; }
    public required List<DoorSpawnData> Doors { get; init; }
    public required List<LightSpawnData> Lights { get; init; }
}

public static class LevelMetadataLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static LevelMetadata Load(string levelMetadataPath, int tileSize)
    {
        if (!File.Exists(levelMetadataPath))
        {
            throw new FileNotFoundException($"[MapValidation] Level metadata file not found: '{levelMetadataPath}'.", levelMetadataPath);
        }

        string json = File.ReadAllText(levelMetadataPath);
        LevelMetadataDto? dto = JsonSerializer.Deserialize<LevelMetadataDto>(json, JsonOptions);
        if (dto is null)
        {
            throw new InvalidOperationException($"[MapValidation] Failed to parse level metadata JSON: '{levelMetadataPath}'.");
        }

        if (string.IsNullOrWhiteSpace(dto.MapImage))
        {
            throw new InvalidOperationException($"[MapValidation] Level metadata '{levelMetadataPath}' is missing required field 'mapImage'.");
        }

        return new LevelMetadata
        {
            MapImage = dto.MapImage,
            Enemies = (dto.Enemies ?? []).Select(e => new EnemySpawnData
            {
                Type = e.Type,
                X = MapLoader.TileToWorld(e.TileX, tileSize),
                Y = MapLoader.TileToWorld(e.TileY, tileSize)
            }).ToList(),
            Keys = (dto.Keys ?? []).Select(k => new KeySpawnData
            {
                Id = k.Id,
                X = MapLoader.TileToWorld(k.TileX, tileSize),
                Y = MapLoader.TileToWorld(k.TileY, tileSize)
            }).ToList(),
            Doors = (dto.Doors ?? []).Select(d => new DoorSpawnData
            {
                Id = d.Id,
                RequiredKeyId = d.RequiredKeyId,
                X = MapLoader.TileToWorld(d.TileX, tileSize),
                Y = MapLoader.TileToWorld(d.TileY, tileSize),
                Locked = !string.IsNullOrWhiteSpace(d.RequiredKeyId)
            }).ToList(),
            Lights = (dto.Lights ?? []).Select(ParseLight).ToList()
        };
    }

    private sealed class LevelMetadataDto
    {
        public string MapImage { get; set; } = string.Empty;
        public List<EnemyDto>? Enemies { get; set; }
        public List<KeyDto>? Keys { get; set; }
        public List<DoorDto>? Doors { get; set; }
        public List<LightDto>? Lights { get; set; }
    }

    private sealed class EnemyDto
    {
        public string Type { get; set; } = "goblin";
        public int TileX { get; set; }
        public int TileY { get; set; }
    }

    private sealed class KeyDto
    {
        public string Id { get; set; } = "silver_key";
        public int TileX { get; set; }
        public int TileY { get; set; }
    }

    private static LightSpawnData ParseLight(LightDto light)
    {
        if (!Enum.TryParse(light.Colour, ignoreCase: true, out LightColour colour))
        {
            throw new InvalidOperationException($"[MapValidation] Light '{light.Id}' has unsupported colour '{light.Colour}'. Supported colours: red, green, white, yellow.");
        }

        if (!Enum.TryParse(light.EnabledWhen, ignoreCase: true, out PowerState enabledWhen))
        {
            throw new InvalidOperationException($"[MapValidation] Light '{light.Id}' has unsupported enabledWhen state '{light.EnabledWhen}'. Supported states: Emergency, Online, Always.");
        }

        if (light.Radius <= 0f)
        {
            throw new InvalidOperationException($"[MapValidation] Light '{light.Id}' must have a radius greater than zero.");
        }

        return new LightSpawnData
        {
            Id = light.Id,
            TileX = light.TileX,
            TileY = light.TileY,
            Radius = light.Radius,
            Colour = colour,
            Enabled = light.Enabled,
            EnabledWhen = enabledWhen
        };
    }

    private sealed class DoorDto
    {
        public string Id { get; set; } = "silver_door";
        public int TileX { get; set; }
        public int TileY { get; set; }
        public string? RequiredKeyId { get; set; }
    }

    private sealed class LightDto
    {
        public string Id { get; set; } = "light";
        public int TileX { get; set; }
        public int TileY { get; set; }
        public float Radius { get; set; }
        public string Colour { get; set; } = "white";
        public bool Enabled { get; set; } = true;
        public string EnabledWhen { get; set; } = "Always";
    }
}
