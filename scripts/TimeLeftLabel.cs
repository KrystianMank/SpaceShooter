using System;
using Godot;

public partial class TimeLeftLabel : Label
{
	public Timer TimeLeftTimer;
	public double TimeLeft;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TimeLeftTimer = GetNode<Timer>(nameof(Timer));
		TimeLeftTimer.Timeout += OnTimeLeftTimerTimeout;
	}

    private void OnTimeLeftTimerTimeout()
    {
        if(TimeLeft > 0)
		{
			Text = (--TimeLeft).ToString();
		}
		else
		{
			Text = string.Empty;
		}
    }

	
}
