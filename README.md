# Test TwitchLib usage in Godot 4.0 Alpha

This project is an example to show that TwitchLib can work with Godot 4.0 Alpha, with Dotnet 6 Merges added to the code.  Currently used 3rd Party libraries:

- NetCoreServer (Used for HTTP Server to Accept code from Twitch)
- Newtonsoft.Json (For Storing and Loading of JSON data)
- TwitchLib (For communicating with TwitchAPI, PubSub and IRC Client)

## Notes:

secrets.json holds the settings for the project, the main thing that needs to be set in the settings file, before running the project, is Channel, ClientId, ClientSecret, ChatToken and ChatRefreshToken.

ClientId and ClientSecret can be created with https://dev.twitch.tv by adding a new Application.

ChatToken and ChatRefreshToken is used for a separate account to handle Bot running.  You can get this from https://twitchtokengenerator.com

BotName, OAuthToken, RefreshToken and ChannelID are filled in by the Program.  On first run after filling out the required fields in settings.json, the program will startup a Small HTTP Server to get a code back, and open your system web-browser to login to your Streamer Account.

This is used for getting PubSub events for your channel.  If you do not authorize with your Streamer Account, PubSub will not work.  IRC Connection is handled by creating a new Twitch Account, then going to the Twitch Token Generator website, and creating a Chat Token, using the newly created Bot Twitch Account.

Once Authorization is processed, the bot will automatically start the connection sequence to connect to the IRC Server, and connect to PubSub.

Any questions, please feel free to let me know.

## Why do this?

The main reason for doing this, is to show a major feature that we finally get in C# in Godot 4.0.  Currently in the 3.x Tree in Godot, you have to use Godot's networking functions for SSL Connections, such as HTTPS Servers, WSS (WebSocket Secure) connections, cause Mono does not trust any certificate natively installed on the end-user's computer.  Which eliminates many C# libraries that use SSL Connections.

With the work that neikeq on Github has done with porting to Dotnet6, this has solved this major issue, and made it more complete implementation.  This should also show, libraries like Github's API, and various other ones, that SSL will no longer be a blocking issue.