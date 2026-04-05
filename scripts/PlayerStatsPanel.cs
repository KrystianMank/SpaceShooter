using Godot;
using GameEnums;
using System;
using System.Linq;
using StaticClasses;
public partial class PlayerStatsPanel : CanvasLayer
{
	private PlayerStats _playerStats;
    private StyleBoxFlat styleBox, statsStyleBox;
    private Theme customTheme, customStatsTheme;
	public void OnReady(Player player, bool show)
    {
        _playerStats = player.playerStats;
		Visible = show;
    }

    public override void _Ready()
    {
        styleBox = new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0f),
            BorderColor = new Color("#cf5d5d"),
        };
        styleBox.SetBorderWidthAll(5);
        customTheme = new Theme();
        customTheme.SetColor("font_color", "Label", Colors.Red);
        customTheme.SetStylebox("normal", "Label", styleBox);

        statsStyleBox = new StyleBoxFlat
        {
            BgColor = new Color("#0c2b54"),
        };
        statsStyleBox.SetCornerRadiusAll(90);
        customStatsTheme = new();
        customStatsTheme.SetStylebox("panel", "HBoxContainer", statsStyleBox);
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
       
        // Update weapon stats to get current values
        _playerStats.UpdateWeaponStats();
        
        // Cleanup
        var grid = GetNode<GridContainer>("PanelContainer/MarginContainer/GridContainer");
		grid.GetChildren().ToList().ForEach(x => x.QueueFree());

        // Information label - Current weapon      
        var label = new Label{
            Text = $"Weapon Stats: {_playerStats.PlayerWeapon.CurrentWeaponType}",
            Theme = customTheme
        };
        grid.AddChild(label);

        var weaponSpecificLabel = new Label();
        string weaponIconLabelText = "";
        switch (_playerStats.PlayerWeapon.CurrentWeaponType)
        {
            case WeaponTypes.MaschineGun:
                {
                    weaponSpecificLabel.Text = $":  {_playerStats.PlayerStatsList["BulletSpeed"]}";
                    weaponIconLabelText = "BulletSpeed";
                    break;
                }
            case WeaponTypes.RocketLauncher:
                {
                    weaponSpecificLabel.Text = $":  {_playerStats.PlayerStatsList["ExplosionRadius"]}";
                    weaponIconLabelText = "ExplosionRadius";
                    break;
                }
            case WeaponTypes.Laser:
                {
                    weaponSpecificLabel.Text = $":  {_playerStats.PlayerStatsList["LaserWidth"]}";
                    weaponIconLabelText = "LaserWidth";
                    break;
                }
        }
        var  weaponIconPath = $"res://sprites/stats upgrade panel/{weaponIconLabelText}.png";
        var weaponTextureBox = new TextureRect()
        {
            Texture = Icons.GetIcon(weaponIconPath),
            Size = new(25f, 25f)
        };

        var weaponPanel = new PanelContainer()
        {
            Theme = customStatsTheme
        };
        var weaponHBox = new HBoxContainer();
        weaponPanel.AddChild(weaponHBox);
        weaponHBox.AddChild(weaponTextureBox);
        weaponHBox.AddChild(weaponSpecificLabel);
        grid.AddChild(weaponPanel);

        for(int i = 3; i<_playerStats.PlayerStatsList.Count; i++)
        {
            // Information label - player 
            if(i == 5)
            {
                 var playerLabel = new Label{
                    Text = $"Player Stats",
                    Theme = customTheme
                };
                grid.AddChild(playerLabel);
            }

            // Ignore Health and MaxHealth stats
            if(_playerStats.PlayerStatsList.ElementAt(i).Key == nameof(_playerStats.MaxHealth) 
                || _playerStats.PlayerStatsList.ElementAt(i).Key == nameof(_playerStats.Health))
            {
                continue;
            }

            var statLabel = new Label();
            if(_playerStats.PlayerStatsList[_playerStats.PlayerStatsList.ElementAt(i).Key].GetType() == typeof(int))
			    statLabel.Text = $" {_playerStats.PlayerStatsList[_playerStats.PlayerStatsList.ElementAt(i).Key]}";
            else
                statLabel.Text = $" {_playerStats.PlayerStatsList[_playerStats.PlayerStatsList.ElementAt(i).Key]:0.00}";
			
            string iconPath = $"res://sprites/stats upgrade panel/{_playerStats.PlayerStatsList.ElementAt(i).Key}.png";

            var textureBox = new TextureRect()
            {
                Texture = Icons.GetIcon(iconPath),
                Size = new(25f, 25f)
            };

            var panel = new PanelContainer()
            {
                Theme = customStatsTheme
            };
            var hBox = new HBoxContainer();
            panel.AddChild(hBox);
            hBox.AddChild(textureBox);
            hBox.AddChild(statLabel);
            
            grid.AddChild(panel);
        }
    }
}
