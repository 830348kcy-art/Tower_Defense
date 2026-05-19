# Stage Intro Popup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first real WPF stage intro popup that uses existing core intro data and starts the stage only after the player confirms.

**Architecture:** Keep `TowerDefense.Core` unchanged. Put popup view, popup view model, and window orchestration in the WPF app project. Test the view model behavior from the existing console harness before adding the XAML window.

**Tech Stack:** C# WPF, MVVM-style view models, `ICommand`, no external packages, existing manual console test harness.

---

### Task 1: Test Popup ViewModel Behavior

**Files:**
- Modify: `tests/TowerDefense.Core.Tests/TowerDefense.Core.Tests.csproj`
- Modify: `tests/TowerDefense.Core.Tests/Program.cs`

- [ ] Reference the WPF app project from the test harness.
- [ ] Add tests that expect `StageIntroViewModel` to select the first new enemy by default, update selection through `SelectEnemyCommand`, and raise `RequestClose` through `StartCommand`.
- [ ] Run `dotnet run --project tests/TowerDefense.Core.Tests/TowerDefense.Core.Tests.csproj --artifacts-path <temp>`.
- [ ] Expected: compile failure because `StageIntroViewModel` does not exist.

### Task 2: Implement Popup ViewModel

**Files:**
- Create: `src/TowerDefense/UI/StageIntroViewModel.cs`

- [ ] Add `StageIntroViewModel` with `Data`, `SelectedEnemy`, `SelectEnemyCommand`, `StartCommand`, `SkipAlwaysCommand`, `SkipAlways`, and `RequestClose`.
- [ ] Keep it free of direct `Window` references so it remains testable.
- [ ] Run the console test harness and verify the new tests pass.

### Task 3: Implement Popup Window

**Files:**
- Create: `src/TowerDefense/StageIntroPopup.xaml`
- Create: `src/TowerDefense/StageIntroPopup.xaml.cs`

- [ ] Add `AllowsTransparency=True` and `WindowStyle=None`.
- [ ] Bind new and returning enemy lists, selected enemy details, stage/chapter/multiplier text, and start/skip commands.
- [ ] Wire `RequestClose` to set `DialogResult=True` and close.

### Task 4: Connect Main Window Flow

**Files:**
- Modify: `src/TowerDefense/UI/MainViewModel.cs`
- Modify: `src/TowerDefense/MainWindow.xaml`
- Modify: `src/TowerDefense/MainWindow.xaml.cs`

- [ ] Change the main button to request the intro popup instead of immediately starting the stage.
- [ ] In `MainWindow`, open `StageIntroPopup`; only after it returns true, call `ConfirmStageIntro()`.
- [ ] Keep `WaveManager.StartStage` in the view model after popup confirmation.

### Task 5: Verify

- [ ] Run the console test harness.
- [ ] Run `dotnet build TowerDefense.sln`.
- [ ] Report pass/fail with the exact limitation if WPF runtime display is not launched.
