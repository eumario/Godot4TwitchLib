using Newtonsoft.Json;
using TestTwitch.lib;
using TestTwitch.lib.BotInterface;
using TestTwitch.lib.MiniHttpServer;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Enums;

namespace TestTwitch.lib;

public class TwitchBase
{
	private readonly List<AuthScopes> Scopes = new List<AuthScopes>() {
		AuthScopes.Helix_Bits_Read,							// Leaderboard Bits
		AuthScopes.Helix_Channel_Manage_Broadcast,			// Modify Channel Information, Create Stream Marker, Replace Stream Tags
		AuthScopes.Helix_Channel_Manage_Moderators,			// Add/Remove Channel Moderators
		AuthScopes.Helix_Channel_Manage_Polls, 				// Create/End Poll
		AuthScopes.Helix_Channel_Manage_Predictions,		// Create/End Channel Prediction
		AuthScopes.Helix_Channel_Manage_Redemptions, 		// Create/Delete/Update/UpdateRedemptionStatus Custom Rewards (Bot Only, No User)
		AuthScopes.Helix_Channel_Read_Editors, 				// Get Channel Editors
		AuthScopes.Helix_Channel_Read_Goals,				// Get Creator Goals
		AuthScopes.Helix_Channel_Read_Hype_Train, 			// Hype Train Events
		AuthScopes.Helix_Channel_Read_Polls, 				// Get Polls
		AuthScopes.Helix_Channel_Read_Predictions,			// Get Channel Points Predictions
		AuthScopes.Helix_Channel_Read_Subscriptions, 		// List Subscribers, Check Subscriber Status
		AuthScopes.Helix_Channel_Read_VIPs, 				// List of VIPs
		AuthScopes.Helix_Channel_Manage_VIPs,				// Get/Add/Remove VIPs
		AuthScopes.Helix_Clips_Edit, 						// Create Clip
		AuthScopes.Helix_Moderation_Read, 					// Check Automod Status, Get Banned Users, Get Moderators
		AuthScopes.Helix_Moderator_Manage_Announcements,	// Send Chat Announcements
		AuthScopes.Helix_Moderator_Manage_Automod, 			// Manage Held AutoMod Messages
		AuthScopes.Helix_Moderator_Manage_Banned_Users, 	// Ban/Unban Users
		AuthScopes.Helix_moderator_Manage_Chat_Messages,	// Delete Chat Messages
		AuthScopes.Helix_Moderator_Read_Chat_Settings,		// Get Chat Settings
		AuthScopes.Helix_Moderator_Manage_Chat_Settings, 	// Update Chat Settings
		AuthScopes.Helix_User_Read_Email, 					// Get user's Authenticated Email
		AuthScopes.Helix_User_Manage_Whispers				// 
	};

    private GodotLogger? _log;
    private Bot? BotInstance;
    private Settings? BotSettings;
    private TwitchReplyServer? _trs;
    private ApiSettings? _settings;
    private TwitchAPI? _api;
    private Godot.RichTextLabel? _label;

    public TwitchBase(Godot.RichTextLabel label) {
        _log = new GodotLogger(typeof(TwitchBase), label);
        _label = label;
        string? data = File.ReadAllText("secrets.json");
        if (data is null) {
            _log.LogError("[MISSING JSON]: Please update secrets.json with Default Configuration for Channel, Server Password, ClientId and ClientSecret");
            return;
        }
        BotSettings = JsonConvert.DeserializeObject<Settings>(data);
        if (BotSettings is null)
        {
            _log.LogError("[INVALID JSON]: Please update secrets.json with Default Configuration for Channel, Server Password, ClientId and ClientSecret");
            return;
        }

        _api = new TwitchAPI();
        _api.Settings.ClientId = BotSettings.ClientId;
        _api.Settings.Secret = BotSettings.ClientSecret;

        if (BotSettings.BotName == String.Empty &&
            BotSettings.OAuthToken == String.Empty &&
            BotSettings.RefreshToken == String.Empty) {
            
            RunSetup();
        } else {
            RunBot();
        }
    }

    public async void RunBot(bool validateToken = true) {
        if (validateToken) {
            _log!.LogInformation("Validating Token for Chat...");
            var res = await ValidateToken(BotSettings!.ChatToken);
            if (!res) {
                _log!.LogInformation("Attempting refresh token for Chat...");
                var tokens = await RefreshToken(BotSettings.ChatRefreshToken);
                if (tokens.Item1 == "" && tokens.Item2 == "")
                {
                    _log!.LogCritical("Chat Bot Token has been invalidated, This needs to be resetup!");
                    return;
                }
                BotSettings.ChatToken = tokens.Item1;
                BotSettings.ChatRefreshToken = tokens.Item2;
            }
            _log!.LogInformation("Validating Token for Streamer...");
            res = await ValidateToken(BotSettings.OAuthToken);
            if (!res) {
                _log!.LogInformation("Attempting to refresh token for Streamer...");
                var tokens = await RefreshToken(BotSettings.RefreshToken);
                if (tokens.Item1 == "" && tokens.Item2 == "")
                {
                    _log!.LogWarning("Streamer token has been invalidated, Re-Running Setup...");
                    RunSetup();
                    return;
                } else {
                    _log!.LogInformation("Streamer Token Refreshed");
                    BotSettings.OAuthToken = tokens.Item1;
                    BotSettings.RefreshToken = tokens.Item2;
                }
            }
        }

        _log!.LogInformation("Starting up Bot...");
        BotInstance = new Bot(BotSettings!, _label!);
        BotInstance.Connect();
    }

    private async Task<Tuple<string,string>> RefreshToken(string token) {
        var apiSettings = new ApiSettings() {
            ClientId = BotSettings!.ClientId,
            Scopes = new List<AuthScopes>() { AuthScopes.Any }
        };
        var api = new TwitchAPI(settings: apiSettings);

        Tuple<string, string> tokens = new Tuple<string, string>("","");
        var res = await api.Auth.RefreshAuthTokenAsync(token, BotSettings.ClientSecret);
        if (res is null) {
            _log!.LogError("Failed to refresh token.");
        } else {
            tokens = new Tuple<string, string>(res.AccessToken, res.RefreshToken);
        }
        return tokens;
    }

    private async Task<bool> ValidateToken(string token) {
        var apiSettings = new ApiSettings() {
            ClientId = BotSettings!.ClientId,
            AccessToken = token,
            Scopes = new List<AuthScopes>() { AuthScopes.Any }
        };
        var api = new TwitchAPI(settings: apiSettings);
        var res = await api.Auth.ValidateAccessTokenAsync();
        if (res is null) {
            _log!.LogInformation("Result is null, Token is expired.");
            return false;
        }
        if (TimeSpan.FromSeconds(res.ExpiresIn) < TimeSpan.FromMinutes(15)) {
            _log!.LogInformation("Token Expiration is less then 15 minutes");
            return false;
        } else {
            return true;
        }
    }


    private void RunSetup() {
        SetupWebServer();
        var authUrl = _api!.Auth.GetAuthorizationCodeUrl(redirectUri: "http://localhost:8080", scopes: Scopes, forceVerify: true);
        _log!.LogInformation($"Generated URL: {authUrl}");
        _log!.LogInformation($"Opening Twitch Authentication URL....");
        Godot.OS.ShellOpen(authUrl);
    }

    private async void ContinueSetup(string code) {
        _log!.LogInformation("Received code back, authenticating code...");
        var res = await _api!.Auth.GetAccessTokenFromCodeAsync(code, BotSettings!.ClientSecret, "http://localhost:8080");
        if (res is null) {
            _log!.LogInformation("Authentication failed, closing out.");
            return;
        }
        BotSettings.OAuthToken = res.AccessToken;
        BotSettings.RefreshToken = res.RefreshToken;
        var tokens = await RefreshToken(BotSettings.ChatRefreshToken);
        BotSettings.ChatToken = tokens.Item1;
        BotSettings.ChatRefreshToken = tokens.Item2;

        _log!.LogInformation("Fetching Bot and Channel Information...");
        SetNameAndIdByOAuthedUser().Wait();
        _log!.LogInformation("Bot and Channel information fetched.");
        File.WriteAllText("secrets.json", JsonConvert.SerializeObject(BotSettings));
        RunBot(false);
    }

    private async Task SetNameAndIdByOAuthedUser() {
        var api = new TwitchAPI();
        api.Settings.ClientId = BotSettings!.ClientId;
        api.Settings.AccessToken = BotSettings!.ChatToken;
        api.Settings.Secret = BotSettings!.ClientSecret;

        var oauthedUser = await api.Helix.Users.GetUsersAsync();
        BotSettings.BotName = oauthedUser.Users[0].DisplayName;
        
        //api.Settings.AccessToken = BotSettings.OAuthToken;

        var channelUser = await api.Helix.Users.GetUsersAsync(logins: new List<string> { BotSettings.Channel });
        BotSettings.ChannelId = channelUser.Users[0].Id;
    }

    private void SetupWebServer() {
        _trs = new TwitchReplyServer(System.Net.IPAddress.Loopback, 8080);
        _trs.TwitchCode += ContinueSetup;
        _trs.Start();
    }
}
