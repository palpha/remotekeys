using System.Net;
using InputSimulatorStandard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RemoteKeys;

public sealed class Server : IDisposable
{
	private IPathParser PathParser { get; }
	private IInputParser InputParser { get; }
	private ITypist Typist { get; }
	private ILogger<Server> Logger { get; }

	public HttpListener Listener { get; } = new();

	public Server(
		IPathParser pathParser,
		IInputParser inputParser,
		ITypist typist,
		Config config,
		ILogger<Server> logger )
	{
		PathParser = pathParser;
		InputParser = inputParser;
		Typist = typist;
		Logger = logger;
		Listener.Prefixes.Add( config.Prefix );
	}

	public async Task Run( CancellationToken cancellationToken )
	{
		Listener.Start();
		Logger.LogInformation( "Listening to {prefix}", Listener.Prefixes.First() );
		try
		{
			await Loop( cancellationToken );
		}
		finally
		{
			Listener.Stop();
		}
	}

	private async Task Loop( CancellationToken cancellationToken )
	{
		while ( true )
		{
			Logger.LogDebug( "Awaiting request..." );

			HttpListenerContext ctx;
			try
			{
				ctx = await Listener.GetContextAsync().WaitAsync( cancellationToken );
				Logger.LogInformation( "Received request: {request}", ctx.Request.RawUrl );
			}
			catch ( TaskCanceledException )
			{
				return;
			}

			var statusCode = 400;
			var statusDescription = "Not allowed";
			IEnumerable<Instruction>? instructions = null;

			try
			{
				var path = ctx.Request.Url?.AbsolutePath;

				if ( path is not null )
				{
					var parts = PathParser.Parse( path );
					instructions = InputParser.Parse( parts );
					statusCode = 200;
					statusDescription = "OK";
				}
			}
			catch ( Exception ex )
			{
				statusCode = 500;
				statusDescription = ex.Message;

				Logger.LogError( ex, "Unable to handle input." );
			}

			ctx.Response.StatusCode = statusCode;
			ctx.Response.StatusDescription = statusDescription;
			ctx.Response.ContentType = "text/plain";
			ctx.Response.Close();

			if ( instructions is not null )
			{
				try
				{
					Typist.Feed( instructions );
				}
				catch ( Exception ex )
				{
					Logger.LogError( ex, "Unable to feed instructions to typist." );
				}
			}

			if ( cancellationToken.IsCancellationRequested )
			{
				return;
			}
		}
	}

	public void Dispose()
	{
		((IDisposable) Listener).Dispose();
	}

	public static Server Compose() =>
		new ServiceCollection()
			.AddLogging( x => x.AddConsole() )
			.AddSingleton( Config.Read() )
			.AddSingleton<ITypist, Typist>()
			.AddSingleton<IInputParser, InputParser>()
			.AddSingleton<IPathParser, PathParser>()
			.AddSingleton<IKeyboardSimulator, KeyboardSimulator>()
			.AddSingleton<Server>()
			.BuildServiceProvider()
			.GetRequiredService<Server>();
}