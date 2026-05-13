using System.Windows;
using System.Windows.Controls;
using KingdomRushClone.Managers;

namespace KingdomRushClone.Views;

public partial class MainMenuPage : Page
{
    public MainMenuPage()
    {
        InitializeComponent();
        var sd = SaveManager.Current;
        StatsText.Text = $"누적 별 ★ {sd.TotalStars}   (사용 가능 {sd.AvailableStars}★)   클리어 스테이지 {sd.StageStars.Count}/30";
    }

    private void OnStart(object s, RoutedEventArgs e) => MainWindow.Instance!.NavigateTo(new StageSelectPage());
    private void OnTech(object s, RoutedEventArgs e) => MainWindow.Instance!.NavigateTo(new TechTreePage());
    private void OnGuide(object s, RoutedEventArgs e) => MainWindow.Instance!.NavigateTo(new TowerGuidePage());
    private void OnExit(object s, RoutedEventArgs e) => Application.Current.Shutdown();
    private void OnHelp(object s, RoutedEventArgs e)
    {
        MessageBox.Show(
            "조작\n" +
            " • 좌클릭: 건설 슬롯(연한 원) 클릭 → 타워 선택\n" +
            " • 기존 타워 클릭: 업그레이드/분기/판매\n" +
            " • 스페이스: 일시정지\n" +
            " • F: 2배속 토글\n" +
            " • N: 다음 웨이브 조기 호출 (잔여시간 비례 골드 보너스)\n" +
            " • 1: 화포 지원 모드 (맵 클릭으로 시전)\n" +
            " • 2: 지원군 소환 모드\n\n" +
            "팁: 길목(코너)에 폭격/마법 배치, 직선에 아처. 병영으로 차단 + 원거리 누킹.",
            "도움말");
    }
}
