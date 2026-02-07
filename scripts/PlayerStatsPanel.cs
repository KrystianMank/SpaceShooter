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
        var i = 0;
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
		
		foreach(var key in _playerStats.PlayerStatsList.Keys)
        {
            if(i == 0){
                
                var label = new Label{
                    Text = "Weapon Stats:",
                    Theme = customTheme
                };
                grid.AddChild(label);
            }
            if(i == 3){
                var label = new Label{
                    Text = "Player Stats:",
                    Theme = customTheme
                };
                grid.AddChild(label);
            }

            var statLabel = new Label();
            if(_playerStats.PlayerStatsList[key].GetType() == typeof(int))
			    statLabel.Text = $"{key}:  {_playerStats.PlayerStatsList[key]}";
            else
                statLabel.Text = $"{key}:  {_playerStats.PlayerStatsList[key]:0.00}";
			grid.AddChild(statLabel);
            i++;
        }
        // var healthLabel = GetParent().GetNode<Label>("PlayerHealthBar/TextureProgressBar/Label");
        // healthLabel.Text = $"{(_playerStats.Health.GetHP().Value <= 0 ? 0 : _playerStats.Health.GetHP().Value)}/{_playerStats.MaxHealth.Value}";

        // var children = grid.GetChildren().ToList();
        // for (int i = 0; i < children.Count; i++)
        // {
        //     if (i < TresholdValues.TresholdValuesList.Count)
        //     {
        //         Label label = (Label)children[i];
        //         label.Text += "    Max : " + TresholdValues.TresholdValuesList[i].ToString();
        //     }
        // }
    }
}
