# Slow System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the slow status effect used by slow towers without changing the existing enemy, wave, or popup flow.

**Architecture:** `EnemyBase` owns slow state and timer because slow is an enemy status. Tower code only applies the effect through `EnemyBase.ApplySlow`. Keep the tower system minimal until the broader tower attack loop exists.

**Tech Stack:** C# core library, manual console test harness, no external packages.

---

### Task 1: Write Slow Behavior Tests

**Files:**
- Modify: `tests/TowerDefense.Core.Tests/Program.cs`

- [ ] Add tests for slow factor affecting `ActualMoveSpeed`.
- [ ] Add tests for stronger factor and longer duration priority.
- [ ] Add tests for slow expiration through `Update`.
- [ ] Add tests for invincible enemies ignoring slow.
- [ ] Add tests for `SlowTower` applying slow through `OnHit`.
- [ ] Run the test harness and confirm it fails because slow APIs do not exist.

### Task 2: Implement Enemy Slow State

**Files:**
- Modify: `src/TowerDefense.Core/Enemies/EnemyBase.cs`
- Modify: `src/TowerDefense.Core/Enemies/EliteEnemies.cs`
- Modify: `src/TowerDefense.Core/Enemies/BossEnemies.cs`

- [ ] Add `SlowFactor`, `_slowTimer`, and `ApplySlow`.
- [ ] Multiply `ActualMoveSpeed` by `SlowFactor`.
- [ ] Tick slow duration in `EnemyBase.Update`.
- [ ] Call `base.Update(deltaTime)` from derived enemy updates so slow can expire consistently.

### Task 3: Implement Minimal Slow Tower

**Files:**
- Create: `src/TowerDefense.Core/Towers/TowerBase.cs`
- Create: `src/TowerDefense.Core/Towers/SlowTower.cs`

- [ ] Add `TowerBase.Hit(enemy)` as the public entry point.
- [ ] Add protected `OnHit(enemy)` override point.
- [ ] Add `SlowTower` with `SlowFactor = 0.5f`, `SlowDuration = 2.0f`, and `OnHit` calling `enemy.ApplySlow`.

### Task 4: Verify

- [ ] Run the console test harness.
- [ ] Run `dotnet build TowerDefense.sln`.
- [ ] Report exact pass/fail status.
