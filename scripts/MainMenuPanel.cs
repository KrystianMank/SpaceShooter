using Godot;

public partial class MainMenuPanel : CanvasLayer
{
	[Export]
	public PackedScene[] MenuScenes;
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("close_upgrade_panel"))
        {
            Visible = false;
        }
    }

    public void OnVisibilityChanged()
    {
        
    }

	public void OnCloseButtonPressed()
    {
        Visible = false;
        Engine.TimeScale = 1;
    }

	public void OnPowerupsInfoButtonPressed()
    {
		var scene = MenuScenes[0].Instantiate();

		scene.GetNode<Button>("BackButton").Pressed += () =>
        {
			scene.QueueFree();
        };

        AddChild(scene);
    }
	
}
