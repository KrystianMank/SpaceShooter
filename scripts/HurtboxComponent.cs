using Godot;
using System;

public partial class HurtboxComponent : Area2D
{
	public HealthComponent HealthComponent;

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;
	}

	public void OnAreaEntered(Area2D area2D)
    {
        // GD.Print(area2D.GetParent().GetType());
        // //if(area2D.GetParent().GetType() == GetParent().GetType()) return;
        if(area2D is HitboxComponent hitbox)
        {
            ShowDamageLabel(hitbox.Damage);
            HealthComponent.DealDamage(hitbox.Damage);
            //ShowDamageLabel(hitbox);
            //GD.Print(area2D.GetParent().GetType() + " " + hitbox.GetCollisionLayerValue(5));
        }
    }
    public void ShowDamageLabel(double damage)
    {
        Label label = new()
        {
            Text = $"-{damage}",
			Position = GlobalPosition,
		};
		var customTheme = new Theme();
		customTheme.SetColor("font_color", "Label", Colors.White);
		label.Theme = customTheme;
		GetParent().AddChild(label);

		var timer = new Timer();
		label.AddChild(timer);
		timer.WaitTime = 0.5;
		timer.OneShot = true;
		timer.Timeout += () => 
		{
			if (IsInstanceValid(label))
				label.QueueFree();
		};
		timer.Start();
    }
}
