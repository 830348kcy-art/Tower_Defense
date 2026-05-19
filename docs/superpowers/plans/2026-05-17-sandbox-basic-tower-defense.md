# Sandbox Basic Tower Defense Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a disposable WPF sandbox where the current enemy, damage, and slow mechanics can be tested interactively.

**Architecture:** Keep production core systems untouched. Put all throwaway gameplay scaffolding under `src/TowerDefense/Sandbox`, with only a small button hook in `MainWindow` to open the sandbox. The sandbox owns its own simple game loop model and uses existing `EnemyBase`, `EnemyFactory`, and `SlowTower` behavior.

**Tech Stack:** C# WPF, `DispatcherTimer` for runtime ticks, manual console test harness, no external packages.

---

### Task 1: Test Sandbox Logic

**Files:**
- Modify: `tests/TowerDefense.Core.Tests/Program.cs`

- [ ] Add tests that create a `SandboxGame`, spawn a wave, tick enemies forward, let towers attack, and verify slow tower reduces speed.
- [ ] Run the test harness and confirm compile failure because sandbox classes do not exist.

### Task 2: Implement Sandbox Model

**Files:**
- Create: `src/TowerDefense/Sandbox/SandboxEnemy.cs`
- Create: `src/TowerDefense/Sandbox/SandboxTower.cs`
- Create: `src/TowerDefense/Sandbox/SandboxGame.cs`

- [ ] Add a short path with fixed points.
- [ ] Spawn enemies from existing `EnemyFactory`.
- [ ] Add a basic damage tower and a slow tower.
- [ ] Tick enemies by `ActualMoveSpeed`, update slow timers through `EnemyBase.Update`, and remove dead/reached enemies.

### Task 3: Implement Sandbox Window

**Files:**
- Create: `src/TowerDefense/Sandbox/SandboxWindow.xaml`
- Create: `src/TowerDefense/Sandbox/SandboxWindow.xaml.cs`
- Modify: `src/TowerDefense/MainWindow.xaml`
- Modify: `src/TowerDefense/MainWindow.xaml.cs`

- [ ] Add a Canvas-based view with path, enemies, towers, HUD, and start/reset controls.
- [ ] Use `DispatcherTimer`; no `Task`, `Thread`, or `BackgroundWorker`.
- [ ] Add a main-window button that opens the sandbox.

### Task 4: Verify

- [ ] Run the console test harness.
- [ ] Run `dotnet build TowerDefense.sln`.
- [ ] Report exact pass/fail status.
