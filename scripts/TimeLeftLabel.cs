using System;
using Godot;

public partial class TimeLeftLabel : Label
{
	public Timer SecondsTimer, TextTimeCountdownTimer;
	public AudioStream PowerupEndSound = GD.Load<AudioStream>("res://sounds/lolo_s-error-474088.mp3");
	public AudioStreamPlayer2D AudioStreamPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SecondsTimer = GetNode<Timer>("SecondsTimer");
		TextTimeCountdownTimer = GetNode<Timer>("TextTimeCountdownTimer");
		AudioStreamPlayer = GetNode<AudioStreamPlayer2D>(nameof(AudioStreamPlayer2D));

		AudioStreamPlayer.Stream = PowerupEndSound;
		SecondsTimer.Timeout += OnSecondsTimerTimeout;
	}

    // public override void _Process(double delta)
    // {
	// 	if (!SecondsTimer.IsStopped())
	// 	{
	// 		TextTimeCountdownTimer.Start();
	// 	}
    // }

	public void AssignValues(double timeLeft)
	{
		TextTimeCountdownTimer.WaitTime = timeLeft;
	}

	public void StartTimers()
	{
		SecondsTimer.Start();
		TextTimeCountdownTimer.Start();
	}

    private void OnSecondsTimerTimeout()
    {
        if(TextTimeCountdownTimer.TimeLeft > 0.1)
		{
			Text = ((int)TextTimeCountdownTimer.TimeLeft).ToString();
		}
		else
		{
			Text = string.Empty;
		}

		if(TextTimeCountdownTimer.TimeLeft > 0 && TextTimeCountdownTimer.TimeLeft < 3)
		{
			AudioStreamPlayer.Play();
		}
    }

	
}
