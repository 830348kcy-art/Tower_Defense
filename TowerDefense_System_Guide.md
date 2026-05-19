# 타워디펜스 시스템 이해 가이드

이 문서는 현재 만들어진 타워디펜스 테스트 시스템을 한 번에 이해하기 위한 요약 파일입니다.
코드를 처음 보는 기준으로, 어디서 실행하고 무엇을 확인하면 되는지 중심으로 정리했습니다.

## 1. 가장 빠른 실행 방법

프로젝트 루트:

```text
C:\Users\Choi hyeong-seon\OneDrive\문서\New project
```

테스트 실행용 배치 파일:

```text
C:\Users\Choi hyeong-seon\OneDrive\문서\New project\RunTowerDefenseTest.cmd
```

배치 파일을 실행하면 WPF 테스트 앱이 켜집니다.
앱 안의 `테스트 플레이` 버튼을 누르면 기본 타워디펜스 샌드박스를 볼 수 있습니다.

게시된 실행 파일 위치:

```text
C:\Users\CodexSandboxOffline\AppData\Local\Temp\TowerDefenseTest\TowerDefense.exe
```

## 2. 프로젝트 구성

전체 구조는 크게 세 부분입니다.

```text
src\TowerDefense.Core
```

게임 규칙이 들어있는 핵심 라이브러리입니다.
적, 타워, 웨이브, 스테이지, 데미지, 슬로우, 분열 규칙이 여기에 있습니다.

```text
src\TowerDefense
```

WPF 테스트 앱입니다.
메인 화면, 스테이지 안내 팝업, 테스트 플레이 샌드박스 화면이 여기에 있습니다.

```text
tests\TowerDefense.Core.Tests
```

콘솔 기반 테스트 하네스입니다.
정식 테스트 프레임워크 대신, 현재는 직접 만든 `Assert` 함수들로 핵심 동작을 검증합니다.

## 3. 핵심 실행 흐름

현재 앱의 큰 흐름은 다음과 같습니다.

1. `MainWindow`가 열립니다.
2. `스테이지 시작`을 누르면 스테이지 안내 팝업이 먼저 뜹니다.
3. 팝업에서 등장 적 정보와 적 이미지를 확인합니다.
4. `스테이지 시작`을 다시 누르면 실제 스테이지가 시작됩니다.
5. `테스트 플레이`를 누르면 별도 샌드박스 창이 열립니다.
6. 샌드박스에서 기본 웨이브, 슬로우, 분열 적을 눈으로 확인합니다.

## 4. 스테이지 안내 팝업

관련 파일:

```text
src\TowerDefense\StageIntroPopup.xaml
src\TowerDefense\StageIntroPopup.xaml.cs
src\TowerDefense\UI\StageIntroViewModel.cs
src\TowerDefense\UI\StageIntroEnemyViewModel.cs
src\TowerDefense\UI\EnemyPreviewImageFactory.cs
src\TowerDefense.Core\Data\StageIntroBuilder.cs
```

팝업의 목적은 스테이지 시작 전에 이번 스테이지에 나오는 적을 알려주는 것입니다.

팝업에서 보여주는 정보:

- 스테이지 번호
- 챕터 번호
- 챕터 체력 배율
- 신규 등장 적
- 재등장 적
- 적 이름
- 적 ID
- HP 배율
- 적 능력
- 적 미리보기 이미지

적 이미지는 아직 외부 PNG 파일을 쓰지 않습니다.
대신 `EnemyPreviewImageFactory`가 WPF `DrawingImage`를 코드로 생성합니다.
그래서 이미지 파일이 없어도 팝업에서 적 이미지가 표시됩니다.

## 5. 챕터와 웨이브 규칙

관련 파일:

```text
src\TowerDefense.Core\Core\GameSession.cs
src\TowerDefense.Core\Data\WavePlan.cs
```

스테이지는 1부터 20까지를 기준으로 합니다.
챕터는 5스테이지 단위입니다.

```text
스테이지 1~5   = 챕터 1
스테이지 6~10  = 챕터 2
스테이지 11~15 = 챕터 3
스테이지 16~20 = 챕터 4
```

챕터가 올라갈수록 적 HP 배율이 증가합니다.

```text
챕터 1 = x1.0
챕터 2 = x1.2
챕터 3 = x1.44
챕터 4 = x1.728
```

보스 스테이지:

```text
5, 10, 15, 20 스테이지
```

중간보스 스테이지:

```text
3, 8, 13, 18 스테이지
```

챕터 3에서는 분열 계열 중간보스와 보스가 등장합니다.

```text
챕터 3 중간보스 = miniboss_split
챕터 3 보스     = boss_split
```

## 6. 적 시스템

관련 파일:

```text
src\TowerDefense.Core\Enemies\EnemyBase.cs
src\TowerDefense.Core\Enemies\NormalEnemies.cs
src\TowerDefense.Core\Enemies\EliteEnemies.cs
src\TowerDefense.Core\Enemies\BossEnemies.cs
src\TowerDefense.Core\Enemies\EnemyFactory.cs
src\TowerDefense.Core\Data\EnemyDatabase.cs
```

모든 적은 `EnemyBase`를 상속합니다.

적이 공통으로 가지는 값:

- `EnemyId`
- `Category`
- `BaseHpPercent`
- `MaxHp`
- `CurrentHp`
- `MoveSpeed`
- `ActualMoveSpeed`
- `GoldReward`
- `SlowFactor`
- `IsInvincible`
- 데미지 면역 여부
- 사망 시 추가 생성할 적 목록

적 생성은 `EnemyFactory.Create(enemyId)`로 합니다.
예를 들어:

```csharp
var enemy = EnemyFactory.Create("boss_split");
```

적 정보 표시는 `EnemyDatabase`가 담당합니다.
팝업은 `EnemyDatabase`의 표시 이름, HP 배율, 능력 설명을 사용합니다.

## 7. 데미지 규칙

관련 파일:

```text
src\TowerDefense.Core\Enemies\DamageType.cs
src\TowerDefense.Core\Enemies\EnemyBase.cs
```

데미지 타입:

```text
Single = 단일 공격
Aoe    = 광역 공격
True   = 방어/무적 우회 공격
```

기본 규칙:

- 적이 죽어 있으면 데미지를 받지 않습니다.
- 무적 상태면 일반 데미지는 막습니다.
- `True` 데미지는 무적을 우회합니다.
- 광역 면역 적은 `Aoe` 데미지를 막습니다.
- 단일 면역 적은 `Single` 데미지를 막습니다.

## 8. 슬로우 시스템

관련 파일:

```text
src\TowerDefense.Core\Enemies\EnemyBase.cs
src\TowerDefense.Core\Towers\TowerBase.cs
src\TowerDefense.Core\Towers\SlowTower.cs
src\TowerDefense\Sandbox\SandboxTower.cs
```

슬로우는 적에게 직접 걸립니다.
타워 영역 자체가 느려지는 방식이 아니라, 적이 슬로우 효과를 맞으면 그 적의 속도 배율이 낮아집니다.

핵심 값:

```csharp
SlowFactor
ActualMoveSpeed
```

실제 이동 속도 계산:

```text
ActualMoveSpeed = MoveSpeed * (1 + SpeedBonus_Aura + SpeedBonus_BossAura) * SlowFactor
```

예시:

```text
기본 속도 160
SlowFactor 0.5
실제 속도 80
```

슬로우 타워 규칙:

- `SlowTower`가 적을 맞추면 슬로우를 적용합니다.
- 현재 설정은 `0.5배 속도`, `2초 지속`입니다.
- 이미 슬로우가 걸린 적보다 아직 느려지지 않은 빠른 적을 우선 타겟팅합니다.
- 무적 적은 슬로우를 무시합니다.

## 9. 분열 시스템

관련 파일:

```text
src\TowerDefense.Core\Enemies\NormalEnemies.cs
src\TowerDefense.Core\Enemies\BossEnemies.cs
src\TowerDefense\Sandbox\SandboxGame.cs
```

분열은 적이 죽을 때 `CreateDeathSpawns()`를 통해 다음 적을 생성하는 방식입니다.

현재 분열 규칙:

```text
boss_split
  -> miniboss_split 2마리

miniboss_split
  -> enemy_split_body 2마리

enemy_split_body
  -> enemy_split_small 3마리

enemy_split_small
  -> 추가 분열 없음
```

즉 챕터 3 분열 보스는 바로 일반 적으로 쪼개지지 않고,
먼저 분열 중간보스로 쪼개진 뒤 그 중간보스가 다시 일반 분열체로 쪼개집니다.

## 10. 테스트 플레이 샌드박스

관련 파일:

```text
src\TowerDefense\Sandbox\SandboxWindow.xaml
src\TowerDefense\Sandbox\SandboxWindow.xaml.cs
src\TowerDefense\Sandbox\SandboxGame.cs
src\TowerDefense\Sandbox\SandboxEnemy.cs
src\TowerDefense\Sandbox\SandboxTower.cs
src\TowerDefense\Sandbox\SandboxPoint.cs
```

샌드박스는 지워도 문제없는 기초 테스트 시스템입니다.
실제 완성 게임이라기보다, 현재 구현한 규칙을 눈으로 확인하는 용도입니다.

샌드박스에 있는 버튼:

```text
기본 웨이브
분열 테스트
리셋
```

기본 웨이브:

```text
enemy_normal
enemy_fast
enemy_split_body
```

분열 테스트:

```text
boss_split
miniboss_split
enemy_split_body
```

샌드박스에 배치된 타워:

```text
초록 타워 = 기본 공격 타워
파란 타워 = 슬로우 타워
```

HUD 표시:

```text
Gold
Lives
Wave
Enemies
Slowed
Split
```

분열 테스트에서 확인할 것:

1. `분열 테스트` 버튼을 누릅니다.
2. 큰 분열 보스, 분열 중간보스, 일반 분열체가 나오는지 봅니다.
3. 보스가 죽으면 중간보스 분열체가 추가되는지 봅니다.
4. 중간보스가 죽으면 일반 분열체가 추가되는지 봅니다.
5. 일반 분열체가 죽으면 작은 분열체가 추가되는지 봅니다.

## 11. 테스트 코드

관련 파일:

```text
tests\TowerDefense.Core.Tests\Program.cs
```

현재 테스트가 확인하는 주요 내용:

- 챕터별 HP 배율 계산
- 적 팩토리 생성
- 데미지 면역 규칙
- 웨이브 플랜
- 웨이브 매니저 스폰/제거
- 스테이지 안내 팝업 데이터
- 팝업 ViewModel 선택/닫기 흐름
- 슬로우 적용/만료
- 무적 적 슬로우 무시
- 슬로우 타워 적중
- 샌드박스 기본 웨이브
- 샌드박스 슬로우 표시
- 샌드박스 빠른 적 우선 슬로우
- 샌드박스 분열 테스트 웨이브
- 보스 사망 시 중간보스 분열체 생성
- 중간보스 사망 시 일반 분열체 생성
- 적 미리보기 이미지 생성

테스트 실행 명령:

```powershell
$env:APPDATA='C:\Users\CodexSandboxOffline\AppData\Roaming'
$env:LOCALAPPDATA='C:\Users\CodexSandboxOffline\AppData\Local'
dotnet run --project tests\TowerDefense.Core.Tests\TowerDefense.Core.Tests.csproj --artifacts-path C:\Users\CodexSandboxOffline\AppData\Local\Temp\TowerDefenseArtifacts
```

## 12. 빌드 명령

OneDrive 경로에서는 `bin`, `obj`, WPF 임시 파일 생성이 꼬일 수 있어서,
현재는 산출물을 임시 폴더로 빼는 방식으로 빌드합니다.

빌드:

```powershell
$env:APPDATA='C:\Users\CodexSandboxOffline\AppData\Roaming'
$env:LOCALAPPDATA='C:\Users\CodexSandboxOffline\AppData\Local'
dotnet build TowerDefense.sln --no-restore --artifacts-path C:\Users\CodexSandboxOffline\AppData\Local\Temp\TowerDefenseArtifacts
```

게시:

```powershell
$env:APPDATA='C:\Users\CodexSandboxOffline\AppData\Roaming'
$env:LOCALAPPDATA='C:\Users\CodexSandboxOffline\AppData\Local'
dotnet publish src\TowerDefense\TowerDefense.csproj -c Release -r win-x64 --self-contained false --artifacts-path C:\Users\CodexSandboxOffline\AppData\Local\Temp\TowerDefenseArtifacts -o C:\Users\CodexSandboxOffline\AppData\Local\Temp\TowerDefenseTest
```

## 13. 새 기능을 추가할 때 볼 위치

새 적 추가:

```text
src\TowerDefense.Core\Enemies
src\TowerDefense.Core\Enemies\EnemyFactory.cs
src\TowerDefense.Core\Data\EnemyDatabase.cs
src\TowerDefense.Core\Data\WavePlan.cs
```

새 타워 추가:

```text
src\TowerDefense.Core\Towers
src\TowerDefense\Sandbox\SandboxTower.cs
```

팝업 표시 변경:

```text
src\TowerDefense\StageIntroPopup.xaml
src\TowerDefense\UI\StageIntroViewModel.cs
src\TowerDefense\UI\StageIntroEnemyViewModel.cs
```

테스트 플레이 화면 변경:

```text
src\TowerDefense\Sandbox
```

새 규칙 검증:

```text
tests\TowerDefense.Core.Tests\Program.cs
```

## 14. 현재 상태 한 줄 요약

현재 시스템은 완성 게임이 아니라, 타워디펜스의 핵심 규칙을 확인하기 위한 MVP 테스트 앱입니다.
스테이지 팝업, 적 이미지 표시, 슬로우 타워, 챕터 3 분열 보스/중간보스 규칙, 샌드박스 전투 확인 기능까지 연결되어 있습니다.
