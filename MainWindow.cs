using Godot;
using System;
using TestTwitch.lib;

public partial class MainWindow : Control
{
	private LineEdit? BotName = null;
	private LineEdit? Channel = null;
	private LineEdit? Streamer = null;
	private RichTextLabel? History = null;

	private TwitchBase? tb;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BotName = GetNode<LineEdit>("%BotName");
		Channel = GetNode<LineEdit>("%Channel");
		Streamer = GetNode<LineEdit>("%Streamer");
		History = GetNode<RichTextLabel>("%History");
		tb = new TwitchBase(History);
	}
}
