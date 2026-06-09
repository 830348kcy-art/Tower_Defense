using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public sealed record AssetPreviewItem(
    string Key,
    string Name,
    string Group,
    double PreviewSize,
    IReadOnlyList<string> ProjectRelativePaths,
    IReadOnlyList<string> ExternalRelativePaths);

public sealed record AssetPreviewMap(
    int Chapter,
    IReadOnlyList<IReadOnlyList<Vec2>> Paths,
    IReadOnlyList<Vec2> TowerSlots,
    double TowerSlotSize);

public static class AssetPreviewCatalog
{
    private const string UserPicturesFolder = "\uADF8\uB9BC";
    private const string ExternalGameFolder = "\uD0C0\uC6CC \uB514\uD39C\uC2A4";
    private const string ExternalMapFolder = "\uB9F5";
    private const string ExternalEnemyFolder = "\uC801";
    private const string ExternalEnemySetFolder = "\uC8013";
    private const string ExternalTowerFolder = "\uD0C0\uC6CC";
    private const string ExternalTowerSlotFolder = "\uD0C0\uC6CC \uC2AC\uB86F";

    public static IReadOnlyList<int> MapChapters { get; } = new[] { 1, 2, 3, 4 };

    public static IReadOnlyList<EnemyKind> EnemyKinds { get; } = new[]
    {
        EnemyKind.Normal,
        EnemyKind.Fast,
        EnemyKind.SplitBody,
        EnemyKind.SplitSmall,
        EnemyKind.Elite,
        EnemyKind.EliteCharge,
        EnemyKind.EliteRegenerator,
        EnemyKind.EliteWyvern,
        EnemyKind.MidBossNormal,
        EnemyKind.MidBossCharge,
        EnemyKind.MidBossSplit,
        EnemyKind.MidBossSpeed,
        EnemyKind.BossNormal,
        EnemyKind.BossCharge,
        EnemyKind.BossSplit,
        EnemyKind.BossSpeed
    };

    public static IReadOnlyList<AssetPreviewItem> TowerAssets { get; } = new[]
    {
        Tower("Archer", "Archer-Lv1", "Archer Lv1", 96),
        Tower("Archer", "Archer-Lv2", "Archer Lv2", 96),
        Tower("Archer", "Archer-Lv3", "Archer Lv3", 96),
        Tower("Archer", "Archer-Sniper", "Archer Sniper", 96),
        Tower("Archer", "Archer-Storm", "Archer Storm", 96),
        Tower("Mage", "Mage-Lv1", "Mage Lv1", 96),
        Tower("Mage", "Mage-Lv2", "Mage Lv2", 96),
        Tower("Mage", "Mage-Lv3", "Mage Lv3", 96),
        Tower("Mage", "Mage-Frost", "Mage Frost", 96),
        Tower("Mage", "Mage-Flame", "Mage Flame", 96),
        Tower("Bombard", "Bombard-Lv1", "Bombard Lv1", 96),
        Tower("Bombard", "Bombard-Lv2", "Bombard Lv2", 96),
        Tower("Bombard", "Bombard-Lv3", "Bombard Lv3", 96),
        Tower("Bombard", "Mortar", "Mortar", 96),
        Tower("Bombard", "Mine Launcher", "Mine Launcher", 96, "Mine-Launcher"),
        Tower("Barracks", "Barracks-Lv1", "Barracks Lv1", 96),
        Tower("Barracks", "Barracks-Lv2", "Barracks Lv2", 96),
        Tower("Barracks", "Barracks-Lv3", "Barracks Lv3", 96),
        Tower("Barracks", "Barracks-Paladin", "Barracks Paladin", 96),
        Tower("Barracks", "Barracks-Rogue", "Barracks Rogue", 96),
        Tower("Slow", "Slow-Lv1", "Slow Lv1", 96),
        Tower("Slow", "Slow-Lv2", "Slow Lv2", 96),
        Tower("Slow", "Slow-Lv3", "Slow Lv3", 96)
    };

    public static IReadOnlyList<AssetPreviewItem> SoldierAssets { get; } = new[]
    {
        Soldier("Soldier-Lv1", "Soldier Lv1", 38),
        Soldier("Soldier-Lv2", "Soldier Lv2", 40),
        Soldier("Soldier-Lv3", "Soldier Lv3", 42),
        Soldier("Soldier-Paladin", "Soldier Paladin", 46),
        Soldier("Soldier-Rogue", "Soldier Rogue", 42),
        Soldier("Soldier-Support", "Soldier Support", 42)
    };

    private static readonly Dictionary<int, AssetPreviewMap> PreviewMaps = new()
    {
        [1] = new AssetPreviewMap(
            1,
            PreviewPaths(
                new[]
                {
                    new Vec2(35, 310),
                    new Vec2(1030, 310)
                }),
            PreviewPoints(
                new Vec2(248, 200),
                new Vec2(430, 200),
                new Vec2(638, 200),
                new Vec2(824, 200),
                new Vec2(248, 420),
                new Vec2(430, 420),
                new Vec2(638, 420),
                new Vec2(824, 420)),
            60),
        [2] = new AssetPreviewMap(
            2,
            PreviewPaths(
                new[]
                {
                    new Vec2(120, 142),
                    new Vec2(165, 183),
                    new Vec2(292, 201),
                    new Vec2(303, 336),
                    new Vec2(406, 335),
                    new Vec2(434, 251),
                    new Vec2(511, 240),
                    new Vec2(553, 202),
                    new Vec2(620, 208),
                    new Vec2(647, 298),
                    new Vec2(731, 308),
                    new Vec2(782, 363),
                    new Vec2(957, 363),
                    new Vec2(1030, 340)
                }),
            PreviewPoints(
                new Vec2(245, 242),
                new Vec2(262, 372),
                new Vec2(324, 150),
                new Vec2(358, 297),
                new Vec2(382, 395),
                new Vec2(428, 195),
                new Vec2(482, 294),
                new Vec2(570, 146),
                new Vec2(604, 329),
                new Vec2(701, 255),
                new Vec2(710, 364),
                new Vec2(818, 426)),
            60),
        [3] = new AssetPreviewMap(
            3,
            PreviewPaths(
                new[]
                {
                    new Vec2(84, 85),
                    new Vec2(170, 175),
                    new Vec2(275, 152),
                    new Vec2(368, 203),
                    new Vec2(498, 178),
                    new Vec2(510, 305),
                    new Vec2(703, 361),
                    new Vec2(795, 348),
                    new Vec2(872, 361),
                    new Vec2(974, 288)
                },
                new[]
                {
                    new Vec2(64, 462),
                    new Vec2(157, 529),
                    new Vec2(225, 462),
                    new Vec2(468, 387),
                    new Vec2(510, 305),
                    new Vec2(703, 361),
                    new Vec2(795, 348),
                    new Vec2(872, 361),
                    new Vec2(974, 288)
                }),
            PreviewPoints(
                new Vec2(197, 367),
                new Vec2(203, 250),
                new Vec2(259, 572),
                new Vec2(291, 82),
                new Vec2(338, 310),
                new Vec2(365, 508),
                new Vec2(446, 122),
                new Vec2(536, 445),
                new Vec2(603, 222),
                new Vec2(650, 425),
                new Vec2(693, 290),
                new Vec2(774, 429),
                new Vec2(817, 295),
                new Vec2(926, 419)),
            60),
        [4] = new AssetPreviewMap(
            4,
            PreviewPaths(
                new[]
                {
                    new Vec2(94, 75),
                    new Vec2(169, 156),
                    new Vec2(386, 180),
                    new Vec2(387, 502),
                    new Vec2(543, 508),
                    new Vec2(551, 313),
                    new Vec2(712, 305),
                    new Vec2(723, 138),
                    new Vec2(864, 134),
                    new Vec2(871, 355),
                    new Vec2(820, 382),
                    new Vec2(827, 476),
                    new Vec2(912, 513),
                    new Vec2(1008, 418)
                }),
            PreviewPoints(
                new Vec2(262, 108),
                new Vec2(307, 247),
                new Vec2(307, 491),
                new Vec2(311, 378),
                new Vec2(441, 119),
                new Vec2(458, 414),
                new Vec2(460, 328),
                new Vec2(467, 226),
                new Vec2(624, 448),
                new Vec2(631, 372),
                new Vec2(645, 118),
                new Vec2(761, 534),
                new Vec2(797, 288),
                new Vec2(797, 196),
                new Vec2(941, 156),
                new Vec2(950, 259)),
            60)
    };

    private static readonly Dictionary<(int Chapter, EnemyKind Kind), string> ExternalEnemyFiles = new()
    {
        [(1, EnemyKind.Normal)] = Path.Combine("1", "01_goblin_soldier.png"),
        [(1, EnemyKind.Fast)] = Path.Combine("1", "02_forest_wolf.png"),
        [(1, EnemyKind.SplitBody)] = Path.Combine("1", "03_large_slime.png"),
        [(1, EnemyKind.SplitSmall)] = Path.Combine("1", "04_small_slime.png"),
        [(1, EnemyKind.Elite)] = Path.Combine("1", "05_shield_ogre.png"),
        [(1, EnemyKind.EliteCharge)] = Path.Combine("1", "06_boar.png"),
        [(1, EnemyKind.BossNormal)] = Path.Combine("1", "07_ogre_boss.png"),
        [(1, EnemyKind.EliteRegenerator)] = Path.Combine("1", "08_shaman_guard.png"),
        [(2, EnemyKind.Normal)] = Path.Combine("2", "01_goblin_soldier.png"),
        [(2, EnemyKind.Fast)] = Path.Combine("2", "02_forest_wolf.png"),
        [(2, EnemyKind.SplitBody)] = Path.Combine("2", "03_large_slime.png"),
        [(2, EnemyKind.SplitSmall)] = Path.Combine("2", "04_small_slime.png"),
        [(2, EnemyKind.Elite)] = Path.Combine("2", "05_shield_ogre.png"),
        [(2, EnemyKind.EliteCharge)] = Path.Combine("2", "06_boar.png"),
        [(2, EnemyKind.EliteRegenerator)] = Path.Combine("2", "07_shaman_guard.png"),
        [(2, EnemyKind.MidBossCharge)] = Path.Combine("2", "08_boar_chieftain.png"),
        [(2, EnemyKind.BossCharge)] = Path.Combine("2", "09_boar_king.png"),
        [(3, EnemyKind.Normal)] = Path.Combine("3", "01_normal_skeleton.png"),
        [(3, EnemyKind.Fast)] = Path.Combine("3", "02_fast_skeleton_beast.png"),
        [(3, EnemyKind.SplitBody)] = Path.Combine("3", "03_split_spider.png"),
        [(3, EnemyKind.SplitSmall)] = Path.Combine("3", "04_small_split_spider.png"),
        [(3, EnemyKind.Elite)] = Path.Combine("3", "05_elite_skeleton.png"),
        [(3, EnemyKind.EliteCharge)] = Path.Combine("3", "06_charge_bone_boar.png"),
        [(3, EnemyKind.EliteRegenerator)] = Path.Combine("3", "07_regen_shaman.png"),
        [(3, EnemyKind.EliteWyvern)] = Path.Combine("3", "08_wyvern.png"),
        [(3, EnemyKind.MidBossSplit)] = Path.Combine("3", "09_split_mid_boss.png"),
        [(3, EnemyKind.BossSplit)] = Path.Combine("3", "spider_boss_transparent.png"),
        [(4, EnemyKind.Normal)] = Path.Combine("4", "01_normal_lava_goblin.png"),
        [(4, EnemyKind.Fast)] = Path.Combine("4", "02_fast_volcanic_wolf.png"),
        [(4, EnemyKind.SplitBody)] = Path.Combine("4", "03_split_molten_blob.png"),
        [(4, EnemyKind.SplitSmall)] = Path.Combine("4", "04_small_molten_spawn.png"),
        [(4, EnemyKind.Elite)] = Path.Combine("4", "05_elite_lava_brute.png"),
        [(4, EnemyKind.EliteCharge)] = Path.Combine("4", "06_charge_lava_boar.png"),
        [(4, EnemyKind.EliteWyvern)] = Path.Combine("4", "07_ember_wyvern.png"),
        [(4, EnemyKind.MidBossSpeed)] = Path.Combine("4", "08_speed_mid_boss.png"),
        [(4, EnemyKind.BossSpeed)] = Path.Combine("4", "09_speed_boss.png")
    };

    public static IEnumerable<string> ProjectAssetRoots()
    {
        return DistinctPaths(new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets")
        });
    }

    public static string? ExternalAssetRoot()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(userProfile)) return null;

        var root = Path.Combine(userProfile, "OneDrive", UserPicturesFolder, "ai img", ExternalGameFolder);
        return Directory.Exists(root) ? root : null;
    }

    public static IEnumerable<string> MapCandidates(
        int chapter,
        IEnumerable<string>? projectRoots = null,
        string? externalRoot = null)
    {
        var candidates = new List<string>();
        foreach (var root in CandidateProjectRoots(projectRoots))
        {
            candidates.Add(Path.Combine(root, "Maps", $"Chapter{chapter}.png"));
            candidates.Add(Path.Combine(root, "Maps", $"{chapter}.png"));
        }

        var external = externalRoot ?? ExternalAssetRoot();
        if (!string.IsNullOrWhiteSpace(external))
            candidates.Add(Path.Combine(external, ExternalMapFolder, $"{chapter}.png"));

        return DistinctPaths(candidates);
    }

    public static IEnumerable<string> TowerSlotCandidates(
        int chapter,
        IEnumerable<string>? projectRoots = null,
        string? externalRoot = null)
    {
        var candidates = new List<string>();
        foreach (var root in CandidateProjectRoots(projectRoots))
        {
            candidates.Add(Path.Combine(root, "TowerSlot", $"TowerSlot-Chapter{chapter}.png"));
            candidates.Add(Path.Combine(root, "MapSlots", $"TowerSlot-Chapter{chapter}.png"));
            candidates.Add(Path.Combine(root, "MapSlots", $"Chapter{chapter}.png"));
        }

        var external = externalRoot ?? ExternalAssetRoot();
        if (!string.IsNullOrWhiteSpace(external))
        {
            candidates.Add(Path.Combine(external, ExternalTowerSlotFolder, $"TowerSlot-Chapter{chapter}.png"));
            candidates.Add(Path.Combine(external, ExternalTowerSlotFolder, $"Chapter{chapter}.png"));
        }

        return DistinctPaths(candidates);
    }

    public static IEnumerable<string> EnemyCandidates(
        int chapter,
        EnemyKind kind,
        IEnumerable<string>? projectRoots = null,
        string? externalRoot = null)
    {
        var candidates = new List<string>();
        foreach (var root in CandidateProjectRoots(projectRoots))
        {
            candidates.Add(Path.Combine(root, "Enemies", $"Chapter{chapter}", $"{kind}.png"));
            candidates.Add(Path.Combine(root, "Enemies", $"Chapter{chapter}", $"{kind}.PNG"));
            candidates.Add(Path.Combine(root, "Enemies", $"Chapter{chapter}", $"{kind}({chapter}).png"));
            candidates.Add(Path.Combine(root, "Enemies", $"Chapter{chapter}", $"{kind}({chapter}).PNG"));
            candidates.Add(Path.Combine(root, "Enemies", $"{kind}.png"));
            candidates.Add(Path.Combine(root, "Enemies", $"{kind}.PNG"));
        }

        var external = externalRoot ?? ExternalAssetRoot();
        if (!string.IsNullOrWhiteSpace(external) && ExternalEnemyFiles.TryGetValue((chapter, kind), out var file))
            candidates.Add(Path.Combine(external, ExternalEnemyFolder, ExternalEnemySetFolder, file));

        return DistinctPaths(candidates);
    }

    public static IEnumerable<string> AssetCandidates(
        AssetPreviewItem item,
        IEnumerable<string>? projectRoots = null,
        string? externalRoot = null)
    {
        var candidates = new List<string>();
        foreach (var root in CandidateProjectRoots(projectRoots))
            foreach (var relativePath in item.ProjectRelativePaths)
                candidates.Add(Path.Combine(root, relativePath));

        var external = externalRoot ?? ExternalAssetRoot();
        if (!string.IsNullOrWhiteSpace(external))
            candidates.AddRange(item.ExternalRelativePaths.Select(relativePath => Path.Combine(external, relativePath)));

        return DistinctPaths(candidates);
    }

    public static string? FirstExisting(IEnumerable<string> candidates)
    {
        foreach (var candidate in candidates)
            if (File.Exists(candidate))
                return candidate;
        return null;
    }

    public static AssetPreviewMap PreviewMapForChapter(int chapter)
    {
        if (PreviewMaps.TryGetValue(chapter, out var map)) return map;

        return PreviewMaps[1];
    }

    public static double TowerVisualSizeForChapter(int chapter) =>
        Math.Clamp(PreviewMapForChapter(chapter).TowerSlotSize * 1.35, 86, 140);

    public static double TowerVisualScaleFor(TowerKind kind, int level, TowerBranch branch)
    {
        var key = TowerAssetFor(kind, level, branch).Key;
        return key switch
        {
            "Archer-Sniper" => 1.2,
            "Bombard-Lv1" or "Bombard-Lv2" or "Bombard-Lv3" => 0.84,
            "Mine Launcher" => 0.82,
            "Barracks-Lv1" or "Barracks-Lv2" or "Barracks-Lv3"
                or "Barracks-Paladin" or "Barracks-Rogue" => 0.82,
            "Slow-Lv1" => 1.14,
            "Slow-Lv2" => 0.98,
            _ => 1.0
        };
    }

    public static double TowerVisualAnchorFor(TowerKind kind, int level, TowerBranch branch)
    {
        var key = TowerAssetFor(kind, level, branch).Key;
        return key switch
        {
            "Archer-Lv1"or "Archer-Lv2" or "Archer-Lv3" => 0.80,
            "Mage-Lv1" => 0.68,
            "Slow-Lv1" => 0.68,
            "Slow-Lv2" => 0.73,
            "Barracks-Lv1" or "Barracks-Lv2" or "Barracks-Lv3"
                or "Barracks-Paladin" or "Barracks-Rogue" => 0.72,
            "Bombard-Lv1" or "Bombard-Lv2" or "Bombard-Lv3" => 0.72,
            "Mine Launcher" => 0.68,

            _ => 0.78
        };
    }

    public static Vec2 TowerVisualOffsetFor(TowerKind kind, int level, TowerBranch branch)
    {
        var key = TowerAssetFor(kind, level, branch).Key;
        return key switch
        {
            "Archer-Sniper" => new Vec2(-4, -6),
            "Archer-Lv3" => new Vec2(-2, 0),
            _ => new Vec2(0, 0)
        };
    }
    public static double TowerVisualSizeFor(
        int chapter,
        TowerKind kind,
        int level,
        TowerBranch branch) =>
        TowerVisualSizeForChapter(chapter) * TowerVisualScaleFor(kind, level, branch);
    public static int ChapterForStage(int stageNumber) => ((stageNumber - 1) / 5) + 1;

    public static AssetPreviewItem TowerAssetFor(TowerKind kind, int level, TowerBranch branch)
    {
        var key = kind switch
        {
            TowerKind.Archer => branch switch
            {
                TowerBranch.A => "Archer-Sniper",
                TowerBranch.B => "Archer-Storm",
                _ => $"Archer-Lv{ClampTowerLevel(level)}"
            },
            TowerKind.Mage => branch switch
            {
                TowerBranch.A => "Mage-Frost",
                TowerBranch.B => "Mage-Flame",
                _ => $"Mage-Lv{ClampTowerLevel(level)}"
            },
            TowerKind.Bombard => branch switch
            {
                TowerBranch.A => "Mortar",
                TowerBranch.B => "Mine Launcher",
                _ => $"Bombard-Lv{ClampTowerLevel(level)}"
            },
            TowerKind.Barracks => branch switch
            {
                TowerBranch.A => "Barracks-Paladin",
                TowerBranch.B => "Barracks-Rogue",
                _ => $"Barracks-Lv{ClampTowerLevel(level)}"
            },
            TowerKind.Slow => $"Slow-Lv{ClampTowerLevel(level)}",
            _ => "Archer-Lv1"
        };

        return TowerAssets.First(item => item.Key == key);
    }

    public static AssetPreviewItem SoldierAssetFor(int ownerLevel, TowerBranch branch, bool isReinforcement)
    {
        var key = isReinforcement
            ? "Soldier-Support"
            : branch switch
            {
                TowerBranch.A => "Soldier-Paladin",
                TowerBranch.B => "Soldier-Rogue",
                _ => $"Soldier-Lv{ClampTowerLevel(ownerLevel)}"
            };

        return SoldierAssets.First(item => item.Key == key);
    }

    private static int ClampTowerLevel(int level) => Math.Clamp(level + 1, 1, 3);

    private static IReadOnlyList<string> CandidateProjectRoots(IEnumerable<string>? projectRoots)
    {
        return DistinctPaths(projectRoots ?? ProjectAssetRoots()).ToArray();
    }

    private static AssetPreviewItem Tower(string group, string key, string name, double size, params string[] alternateKeys)
    {
        var keys = new[] { key }.Concat(alternateKeys).ToArray();

        return new AssetPreviewItem(
            key,
            name,
            group,
            size,
            keys.SelectMany(fileKey => new[]
            {
                Path.Combine("Towers", group, $"{fileKey}.png"),
                Path.Combine("Towers", $"{fileKey}.png")
            }).ToArray(),
            keys.SelectMany(fileKey => new[]
            {
                Path.Combine(ExternalTowerFolder, group, $"{fileKey}.png"),
                Path.Combine(ExternalTowerFolder, $"{fileKey}.png")
            }).ToArray());
    }

    private static AssetPreviewItem Soldier(string key, string name, double size)
    {
        return new AssetPreviewItem(
            key,
            name,
            "Soldiers",
            size,
            new[]
            {
                Path.Combine("Soldiers", $"{key}.png"),
                Path.Combine("Towers", "Barracks", $"{key}.png")
            },
            new[]
            {
                Path.Combine(ExternalTowerFolder, "Barracks", $"{key}.png")
            });
    }

    private static IReadOnlyList<IReadOnlyList<Vec2>> PreviewPaths(params Vec2[][] paths)
    {
        return paths.Select(path => (IReadOnlyList<Vec2>)path).ToArray();
    }

    private static IReadOnlyList<Vec2> PreviewPoints(params Vec2[] points)
    {
        return points;
    }

    private static IEnumerable<string> DistinctPaths(IEnumerable<string> paths)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            if (seen.Add(path)) yield return path;
        }
    }
}
