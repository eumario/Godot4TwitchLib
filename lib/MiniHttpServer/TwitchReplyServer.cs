using System.Net;
using NetCoreServer;

namespace TestTwitch.lib.MiniHttpServer;

public class TwitchReplyServer : HttpServer
{
	public event OnTwitchCode? TwitchCode;

	public TwitchReplyServer(IPAddress address, int port) : base(address, port) { }

	protected override TcpSession CreateSession()
	{
		var session = new TwitchReplySession(this);
		session.TwitchCode += (code) =>
		{
			TwitchCode?.Invoke(code);
		};
		return session;
	}
}
