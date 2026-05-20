using System.Collections.Generic;
using KingdomRushClone.Data;
using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class WaveSpawner
{
    private readonly StageDef   _stage;
    private readonly GameEngine _engine;
    private int    _waveIndex = -1;
    private double _timeUntilAuto;

    private readonly List<EntryState> _activeEntries = new();

    private class EntryState
    {
        public WaveEntry Entry  = null!;
        public double    NextSpawn;
        public int       Spawned;
    }

    public WaveSpawner(StageDef stage, GameEngine engine)
    {
        _stage  = stage;
        _engine = engine;
        _timeUntilAuto = 5.0;
    }

    // ─── Public state ────────────────────────────────────────────────────
    public int    CurrentWave       => _waveIndex + 1;
    public int    TotalWaves        => _stage.Waves.Count;
    public double NextWaveCountdown => _timeUntilAuto;
    public bool   AllWavesStarted   => _waveIndex >= _stage.Waves.Count - 1;
    public bool   HasPendingSpawns  => _activeEntries.Count > 0;

    /// <summary>
    /// Returns the distinct enemy kinds in the NEXT wave (not yet started).
    /// Used to populate the wave-preview HUD indicator.
    /// </summary>
    public List<EnemyKind> PeekNextWaveEnemies()
    {
        int next = _waveIndex + 1;
        if (next >= _stage.Waves.Count) return new();
        var result = new List<EnemyKind>();
        foreach (var entry in _stage.Waves[next].Entries)
            if (!result.Contains(entry.Enemy))
                result.Add(entry.Enemy);
        return result;
    }

    // ─── Tick ────────────────────────────────────────────────────────────
    public void Tick(double dt)
    {
        if (!AllWavesStarted)
        {
            _timeUntilAuto -= dt;
            if (_timeUntilAuto <= 0) StartNextWave(false);
        }

        for (int i = _activeEntries.Count - 1; i >= 0; i--)
        {
            var s = _activeEntries[i];
            s.NextSpawn -= dt;
            if (s.NextSpawn <= 0 && s.Spawned < s.Entry.Count)
            {
                SpawnOne(s.Entry);
                s.Spawned++;
                s.NextSpawn = s.Entry.SpawnInterval;
                if (s.Spawned >= s.Entry.Count)
                    _activeEntries.RemoveAt(i);
            }
        }
    }

    // ─── Early call ──────────────────────────────────────────────────────
    /// <summary>
    /// Player calls the next wave early.
    /// Returns bonus gold = remaining seconds × 2 (rounded).
    /// </summary>
    public int CallNextWaveEarly()
    {
        if (AllWavesStarted) return 0;
        int bonus = (int)(System.Math.Max(0, _timeUntilAuto) * 2);
        StartNextWave(true);
        return bonus;
    }

    // ─── Private ─────────────────────────────────────────────────────────
    private void StartNextWave(bool earlyCall)
    {
        _waveIndex++;
        if (_waveIndex >= _stage.Waves.Count) return;

        var wave = _stage.Waves[_waveIndex];
        foreach (var entry in wave.Entries)
        {
            _activeEntries.Add(new EntryState
            {
                Entry     = entry,
                NextSpawn = entry.InitialDelay,
                Spawned   = 0
            });
        }
        _timeUntilAuto = wave.TimeUntilNext;
    }

    private void SpawnOne(WaveEntry entry)
    {
        var def  = EnemyCatalog.Enemies[entry.Enemy];
        var path = _stage.Paths[entry.SpawnPath % _stage.Paths.Count];
        _engine.Enemies.Add(_engine.CreateEnemy(def, path[0], path, 0));
    }
}
