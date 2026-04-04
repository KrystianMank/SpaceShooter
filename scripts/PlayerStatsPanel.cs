using Godot;
using GameEnums;
using System;
using System.Linq;
using StaticClasses;
public partial class PlayerStatsPanel : CanvasLayer
{
	private PlayerStats _playerStats;
	public void OnReady(Player player, bool show)
    {
        _playerStats = player.playerStats;
		Visible = show;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Visible)
        {
            PrintStats();
        }
    }


	public void PrintStats()
    {
        var styleBox = new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0f),
            BorderColor = new Color(255, 0,0),
        };
        var customTheme = new Theme();
        customTheme.SetColor("font_color", "Label", Colors.Red);
        customTheme.SetStylebox("normal", "Label", styleBox);
        // Update weapon stats to get current values
        _playerStats.UpdateWeaponStats();
        
        var grid = GetNode<GridContainer>("PanelContainer/GridContainer");
		grid.GetChildren().ToList().ForEach(x => x.QueueFree());
                
        var label = new Label{
            Text = $"Weapon Stats: {_playerStats.PlayerWeapon.CurrentWeaponType}",
            Theme = customTheme
        };
        grid.AddChild(label);


        var weaponSpecificLabel = new Label();
        switch (_playerStats.PlayerWeapon.CurrentWeaponType)
        {
            case WeaponTypes.MaschineGun:
                {
                    weaponSpecificLabel.Text = $"{_playerStats.PlayerStatsList.Keys.First(x => x == "BulletSpeed")}:  {_playerStats.PlayerStatsList["BulletSpeed"]}";
                    break;
                }
            case WeaponTypes.RocketLauncher:
                {
                    weaponSpecificLabel.Text = $"{_playerStats.PlayerStatsList.Keys.First(x => x == "ExplosionRadius")}:  {_playerStats.PlayerStatsList["ExplosionRadius"]}";
                    break;
                }
            case WeaponTypes.Laser:
                {
                    weaponSpecificLabel.Text = $"{_playerStats.PlayerStatsList.Keys.First(x => x == "LaserWidth")}:  {_playerStats.PlayerStatsList["LaserWidth"]}";
                    break;
                }
        }
        grid.AddChild(weaponSpecificLabel);

        for(int i = 3; i<_playerStats.PlayerStatsList.Count; i++)
        {
            if(i == 5)
            {
                 var playerLabel = new Label{
                    Text = $"Player Stats",
                    Theme = customTheme
                };
                grid.AddChild(playerLabel);
            }
            var statLabel = new Label();
            if(_playerStats.PlayerStatsList[_playerStats.PlayerStatsList.ElementAt(i).Key].GetType() == typeof(int))
			    statLabel.Text = $"{_playerStats.PlayerStatsList.ElementAt(i).Key}:  {_playerStats.PlayerStatsList[_playerStats.PlayerStatsList.ElementAt(i).Key]}";
            else
                statLabel.Text = $"{_playerStats.PlayerStatsList.ElementAt(i).Key}:  {_playerStats.PlayerStatsList[_playerStats.PlayerStatsList.ElementAt(i).Key]:0.00}";
			grid.AddChild(statLabel);
        }
    }
}
