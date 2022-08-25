using System;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using NetCoreServer;

namespace TestTwitch.lib.MiniHttpServer;


public delegate void OnTwitchCode(string code);

public class TwitchReplySession : HttpSession
{
	public event OnTwitchCode? TwitchCode;

	public TwitchReplySession(HttpServer server) : base(server) { }

	protected override void OnReceivedRequest(HttpRequest request)
	{
		string code = "";

		if (request.Method == "GET") {
			string msg = "";
			string querystring = "";
			int iqs = request.Url.IndexOf("?");
			if (iqs == -1) {
				msg = "Invalid Formed URL, no code present.";
			} else if (iqs > 0) {
				querystring = (iqs < request.Url.Length - 1) ? request.Url.Substring(iqs + 1) : String.Empty;
			}

			if (querystring == "") {
				msg = "Invalid Formed URL, no code present.";
			} else {
				NameValueCollection args = HttpUtility.ParseQueryString(querystring);
				if (args.AllKeys.Any("code".Contains!))
				{
					code = args["code"]!;
					msg = "Received Code from Twitch, you may now close this window.";
				}
			}
			SendResponseAsync(Response.MakeGetResponse(msg));
		}

		if (code != "")
			TwitchCode?.Invoke(code);
	}
}