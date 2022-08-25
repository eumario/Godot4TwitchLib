using System;
using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TestTwitch.lib.BotInterface;

public class Bot
{
	Settings? BotSettings = null;
	TwitchAPI? API = null;
	TwitchClient? Client = null;
	PubSub? _ps = null;
	GodotLogger? _log = null;

	public Bot(Settings settings, Godot.RichTextLabel label) {
		BotSettings = settings;
		_ps = new PubSub(settings,label);
        _log = new GodotLogger(typeof(Bot), label);
		API = new TwitchAPI();
		API.Settings.AccessToken = BotSettings.OAuthToken;
		API.Settings.ClientId = BotSettings.ClientId;

		ConnectionCredentials creds = new ConnectionCredentials(BotSettings.BotName, BotSettings.ChatToken);
		var clientOptions = new ClientOptions
		{
			MessagesAllowedInPeriod = 750,
			ThrottlingPeriod = TimeSpan.FromSeconds(20)
		};

		var customClient = new WebSocketClient(clientOptions);
		Client = new TwitchClient(customClient);
		Client.Initialize(creds, BotSettings.Channel);

		SetupClientEvents();
	}

	public void Connect() {
		Client!.Connect();
		_ps!.Connect();
	}

	void SetupClientEvents() {
		Client!.OnLog += (sender, e) => _log!.LogInformation($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
		Client!.OnConnected += (sender, e) => _log!.LogInformation($"Connected to {e.AutoJoinChannel}");
		Client!.OnJoinedChannel += (sender, e) => Client.SendMessage(e.Channel, "Wolfpack bot online!");

		Client!.OnMessageReceived += (sender, e) => _log!.LogInformation($"{DateTime.Now.ToString()} {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
		Client!.OnWhisperReceived += (sender, e) => _log!.LogInformation($"{DateTime.Now.ToString()} [{e.WhisperMessage.DisplayName}]: {e.WhisperMessage.Message}");

		Client!.OnNewSubscriber += (sender, e) => _log!.LogInformation($"{DateTime.Now.ToString()} {e.Subscriber.DisplayName} just subscribed!");
		Client!.OnReSubscriber += (sender, e) => _log!.LogInformation($"{DateTime.Now.ToString()} {e.ReSubscriber.DisplayName} just re-subscribed!");

		Client!.OnGiftedSubscription += (sender, e) => _log!.LogInformation($"{DateTime.Now.ToString()} {e.GiftedSubscription.DisplayName} just gifted a sub to {e.GiftedSubscription.MsgParamRecipientDisplayName}");
		Client!.OnPrimePaidSubscriber += (sender, e) => _log!.LogInformation($"{DateTime.Now.ToString()} {e.PrimePaidSubscriber.DisplayName} just subscribed with their Prime Subscription!");

		Client!.OnContinuedGiftedSubscription += (sender, e) => _log!.LogInformation($"{DateTime.Now.ToString()} {e.ContinuedGiftedSubscription.DisplayName} is continuing their subscription they got from {e.ContinuedGiftedSubscription.MsgParamSenderName}.");
		Client!.OnRaidNotification += (sender, e) => _log!.LogInformation($"{e.RaidNotification.MsgParamDisplayName} just raided us with {e.RaidNotification.MsgParamViewerCount} viewers");
	}

}
