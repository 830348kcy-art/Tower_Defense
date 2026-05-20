using System.Collections.Generic;
using KingdomRushClone.Data;
using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class WaveSpawner
{
    private readonly StageDef _stage;
    private readonly GameEngine _engine;
    private int _waveIndex = -1;
    private double _timeUntilAuto;
    private readonly List<EntryState> _activeEntries = new();
    private class EntryState
    {
        public WaveEntry Entry = null!;
        public double NextSpawn;
        public int Spawned;
    }

    public WaveSpawner(StageDef stage, GameEngine engine)
    {
        _stage = stage;
        _engine = engine;
        _timeUntilAuto = 5.0;
    }

    public int CurrentWave => _waveIndex + 1;
    public int TotalWaves => _stage.Waves.Count;
    public double NextWaveCountdown => _timeUntilAuto;
    public bool AllWavesStarted => _waveIndex >= _stage.Waves.Count - 1;
    public bool HasPendingSpawns => _activeEntries.Count > 0;

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
                if (s.Spawned >= s.Entry.Count) _activeEntries.RemoveAt(i);
            }
        }
    }

    public int CallNextWaveEarly()
    {
        if (AllWavesStarted) return 0;
        double remaining = _timeUntilAuto > 0 ? _timeUntilAuto : 0;
        int bonus = (int)(remaining * 2);
        StartNextWave(true);
        return bonus;
    }

    private void StartNextWave(bool earlyCall)
    {
        _waveIndex++;
        if (_waveIndex >= _stage.Waves.Count) return;
        var wave = _stage.Waves[_waveIndex];
        foreach (var entry in wave.Entries)
        {
            _activeEntries.Add(new EntryState
            {
                Entry = entry,
                NextSpawn = entry.InitialDelay,
                Spawned = 0
            });
        }
        _timeUntilAuto = wave.TimeUntilNext;
    }

    private void SpawnOne(WaveEntry entry)
    {
        var def = EnemyCatalog.Enemies[entry.Enemy];
        var path = _stage.Paths[entry.SpawnPath % _stage.Paths.Count];
        var enemy = _engine.CreateEnemy(def, path[0], path, 0);
        _engine.Enemies.Add(enemy);
    }
}
