using Godot;
using System;

public partial class FpsMonitor : Label
{
	private float updateTimer = 0f;
	private float updateInterval = 0.5f; // Update FPS every 0.5 seconds
	private int frameCount = 0;

	public override void _Process(double delta)
	{
		frameCount++;
		updateTimer += (float)delta;

		if (updateTimer >= updateInterval)
		{
			int fps = (int)(frameCount / updateTimer);
			Text = $"FPS: {fps}";
			frameCount = 0;
			updateTimer = 0f;
		}
	}
}
