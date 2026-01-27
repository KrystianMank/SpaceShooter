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
		PrintStats();
		Visible = show;
    }

	public void PrintStats()
    {
        var grid = GetNode<GridContainer>("PanelContainer/GridContainer");
		grid.GetChildren().ToList().ForEach(x => x.QueueFree());
		
		foreach(var key in _playerStats.PlayerStatsList.Keys)
        {
            var label = new Label();
            if(_playerStats.PlayerStatsList[key].GetType() == typeof(int))
			    label.Text = $"{key}:  {_playerStats.PlayerStatsList[key]}";
            else
                label.Text = $"{key}:  {_playerStats.PlayerStatsList[key]:0.00}";
			grid.AddChild(label);
        }
        var healthLabel = GetParent().GetNode<Label>("PlayerHealthBar/TextureProgressBar/Label");
        healthLabel.Text = $"{(_playerStats.Health.GetHP().Value <= 0 ? 0 : _playerStats.Health.GetHP().Value)}/{_playerStats.MaxHealth.Value}";

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
