# Tower Defense MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first playable-code foundation for the WPF tower defense project: app shell, core enemy system, wave/session flow, and stage intro data.

**Architecture:** Keep game rules in `TowerDefense.Core` so they can be tested without WPF. Keep the WPF project thin for now, referencing the core library and showing a simple shell until the popup and sprite UI are expanded.

**Tech Stack:** C# WPF, .NET 10 SDK targeting `.NET 6+`, MVVM-friendly structure, `DispatcherTimer` reserved for future UI timers, manual console test harness with no external NuGet packages.

---

### File Structure

- Create: `TowerDefense.sln` solution tying the app, core library, and tests together.
- Create: `src/TowerDefense/TowerDefense.csproj` WPF app shell.
- Create: `src/TowerDefense.Core/TowerDefense.Core.csproj` testable game logic.
- Create: `tests/TowerDefense.Core.Tests/TowerDefense.Core.Tests.csproj` console test harness.
- Create: `src/TowerDefense.Core/Core/GameSession.cs` for stage, chapter, HP multiplier, and seen-enemy tracking.
- Create: `src/TowerDefense.Core/Core/WaveManager.cs` for stage/wave lifecycle and active enemy tracking.
- Create: `src/TowerDefense.Core/Data/WavePlan.cs` for stage classification and enemy plans.
- Create: `src/TowerDefense.Core/Data/EnemyDatabase.cs` for enemy display/spec lookup.
- Create: `src/TowerDefense.Core/Data/StageIntroBuilder.cs` for new/returning enemy intro data.
- Create: `src/TowerDefense.Core/Enemies/*.cs` for enemy base types, concrete enemies, factory, damage types, and lightweight sprite contract.
- Create: `tests/TowerDefense.Core.Tests/Program.cs` with behavior tests for multiplier, enemy damage, wave spawn, factory, and stage intro.

### Task 1: Scaffold Projects

- [ ] Run `dotnet new sln -n TowerDefense`.
- [ ] Run `dotnet new classlib -n TowerDefense.Core -o src/TowerDefense.Core --framework net10.0`.
- [ ] Run `dotnet new wpf -n TowerDefense -o src/TowerDefense --framework net10.0-windows`.
- [ ] Run `dotnet new console -n TowerDefense.Core.Tests -o tests/TowerDefense.Core.Tests --framework net10.0`.
- [ ] Add all projects to the solution and add project references from app/tests to core.
- [ ] Remove template placeholder files that are not needed.

### Task 2: Write Failing Core Tests

- [ ] Replace `tests/TowerDefense.Core.Tests/Program.cs` with manual assertions that reference the intended public API.
- [ ] Run `dotnet run --project tests/TowerDefense.Core.Tests/TowerDefense.Core.Tests.csproj`.
- [ ] Expected result: fail to compile because core domain types do not exist yet.

### Task 3: Implement Core Domain

- [ ] Add `GameSession`, `WaveManager`, `WavePlan`, enemy types, factory, database, and stage intro builder.
- [ ] Keep implementation simple and connected: no rendering code, no thread/task usage, no speculative features.
- [ ] Run the test harness until all tests pass.

### Task 4: Connect WPF Shell

- [ ] Replace the generated shell with a minimal Korean UI showing project title and start context.
- [ ] Keep WPF dependency one-way: app references core, core does not reference WPF.
- [ ] Build the full solution.

### Task 5: Verification

- [ ] Run `dotnet test TowerDefense.sln` if test discovery is applicable.
- [ ] Run `dotnet run --project tests/TowerDefense.Core.Tests/TowerDefense.Core.Tests.csproj`.
- [ ] Run `dotnet build TowerDefense.sln`.
- [ ] Report exact verification status and any limitations.
