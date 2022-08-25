using Microsoft.Extensions.Logging;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub;
using System;

namespace TestTwitch.lib.BotInterface;

public class PubSub
{
    private Settings? BotSettings;
    private TwitchPubSub? _pubSub;

    GodotLogger? _log;

    public PubSub(Settings settings, Godot.RichTextLabel label) {
        BotSettings = settings;
        _log = new GodotLogger(typeof(PubSub), label);
        _pubSub = new TwitchPubSub();
        SetupEvents();
    }

    private void SetupEvents() {
        _pubSub!.ListenToBitsEventsV2(BotSettings!.ChannelId);
        _pubSub!.ListenToChannelPoints(BotSettings.ChannelId);
        _pubSub!.ListenToFollows(BotSettings.ChannelId);
        _pubSub!.ListenToSubscriptions(BotSettings.ChannelId);
        _pubSub!.ListenToLeaderboards(BotSettings.ChannelId);
        _pubSub!.ListenToPredictions(BotSettings.ChannelId);

        _pubSub!.OnPubSubServiceConnected += OnConnected;
        _pubSub!.OnLog += OnLog;
        _pubSub!.OnChannelPointsRewardRedeemed += OnChannelPoints;
        _pubSub!.OnBitsReceivedV2 += OnBitsReceived;
        _pubSub!.OnFollow += OnFollow;
        _pubSub!.OnChannelSubscription += OnSubscription;
        _pubSub!.OnLeaderboardBits += OnLeaderboardBits;
        _pubSub!.OnLeaderboardSubs += OnLeaderboardSubs;
        _pubSub!.OnPrediction += OnPrediction;
        _pubSub!.OnListenResponse += OnListenResponse;
    }

    private void OnConnected(object? sender, EventArgs e) {
        _log!.LogInformation("Connected to PubSub");
        _pubSub!.SendTopics(BotSettings!.OAuthToken);
        _log!.LogInformation("Sending Topics...");
    }

    private void OnLog(object? sender, OnLogArgs e) => _log!.LogInformation($"Log: {e.Data}");

    private void OnChannelPoints(object? sender, OnChannelPointsRewardRedeemedArgs e) {
        var redemption = e.RewardRedeemed.Redemption;
        _log!.LogInformation($"{redemption.User.DisplayName} just redeemed {redemption.Reward.Title} for {redemption.Reward.Cost}");
    }

    private void OnBitsReceived(object? sender, OnBitsReceivedV2Args e) {
        _log!.LogInformation($"Received: {e.BitsUsed} from {e.UserId} with message: {e.ChatMessage}");
    }

    private void OnFollow(object? sender, OnFollowArgs e) {
        _log!.LogInformation($"Received: {e.DisplayName} follows {e.FollowedChannelId}");
    }

    private void OnSubscription(object? sender, OnChannelSubscriptionArgs e) {
        _log!.LogInformation($"Received: {e.Subscription.DisplayName} subscribed to {e.Subscription.ChannelName} for {e.Subscription.Months} month{(e.Subscription.Months > 1 ? "s" : "")} with tier {e.Subscription.SubscriptionPlanName}");
    }

    private void OnLeaderboardBits(object? sender, OnLeaderboardEventArgs e) {
        var msg = "";
        foreach(var pos in e.TopList) {
            msg += $"{pos.Place}: {pos.UserId} with {pos.Score} bits, ";
        }
        _log!.LogInformation($"Received: {msg} donated");
    }

    private void OnLeaderboardSubs(object? sender, OnLeaderboardEventArgs e) {
        var msg = "";
        foreach(var pos in e.TopList) {
            msg += $"{pos.Place}: {pos.UserId} with {pos.Score} gifted subs, ";
        }
        _log!.LogInformation($"Received: {msg} donated");
    }

    private void OnPrediction(object? sender, OnPredictionArgs e) {
        _log!.LogInformation($"Received: Prediction: {e.Title}, Status: {e.Status}, Outcomes: {e.Outcomes}");
    }

    private void OnListenResponse(object? sender, OnListenResponseArgs e) => _log!.LogInformation($"Topic: {e.Topic}: Success: {e.Successful}, Response: {e.Response.Error}");

    public void Connect() => _pubSub!.Connect();
}
