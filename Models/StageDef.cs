using System.Collections.Generic;

namespace KingdomRushClone.Models;

public class WaveEntry
{
    public EnemyKind Enemy;
    public int Count;
    public double SpawnInterval = 0.7;
    public double InitialDelay;
    public int SpawnPath;
}

public class WaveDef
{
    public List<WaveEntry> Entries = new();
    public double TimeUntilNext = 25;
}

public enum StageTheme { Grassland, Forest, Desert, Volcano, Snow, Castle }

public class StageDef
{
    public int Number;
    public string Name = "";
    public StageTheme Theme;
    public int StartingGold = 200;
    public int StartingLives = 20;
    public List<List<Vec2>> Paths = new();
    public List<Vec2> BuildSlots = new();
    public List<WaveDef> Waves = new();
    public List<TowerKind> AllowedTowers = new();
    public bool HasMidBoss;
    public bool HasBoss;
    public double EnemyHpScale = 1.0;
    public double EnemySpeedScale = 1.0;
    public List<EnvEffect> Effects = new();
}

public enum EnvEffect { None, LavaTiles, NightVision, IcePath, NarrowCorridor }
