using System.Windows.Media;

namespace TowerDefense.UI;

public sealed class StageIntroEnemyViewModel
{
    public StageIntroEnemyViewModel(EnemyDisplayInfo info)
    {
        Info = info;
        PreviewImage = EnemyPreviewImageFactory.Create(info.EnemyId);
    }

    public EnemyDisplayInfo Info { get; }

    public ImageSource PreviewImage { get; }

    public string EnemyId => Info.EnemyId;

    public string DisplayName => Info.DisplayName;

    public float HpPercent => Info.HpPercent;

    public string[] Abilities => Info.Abilities;
}
