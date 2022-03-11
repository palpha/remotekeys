using System.Net;
using InputSimulatorStandard;
using RemoteKeys;

// ReSharper disable once UseDeconstruction
// ReSharper disable once SuggestVarOrType_SimpleTypes
var config = Config.Read();

var listener = new HttpListener();
listener.Prefixes.Add( config.Prefix );
listener.Start();

Console.WriteLine( $"Listening to {config.Prefix}." );

var typist = new Typist( new KeyboardSimulator() );
var parser = new InputParser();

while ( true )
{
	var ctx = await listener.GetContextAsync();
	var path = ctx.Request.Url?.AbsolutePath;
	var statusCode = 400;
	var statusDescription = "Not allowed";
	IEnumerable<Instruction>? instructions = null;

	if ( path is not null )
	{
		var parts = path.Split( '/' )[1..];
		Console.WriteLine( $"Parts: {string.Join( "; ", parts )}" );
		try
		{
			instructions = parser.Parse( parts );

			statusCode = 200;
			statusDescription = "OK";
		}
		catch ( Exception ex )
		{
			Console.WriteLine( $"Error: {ex.Message}" );
			statusCode = 500;
			statusDescription = ex.Message;
		}
	}
	else
	{
		Console.WriteLine( "No path." );
	}

	ctx.Response.StatusCode = statusCode;
	ctx.Response.StatusDescription = statusDescription;
	ctx.Response.ContentType = "text/plain";
	ctx.Response.Close();

	if ( instructions is not null )
	{
		try
		{
			typist.Feed( instructions );
		}
		catch ( Exception ex )
		{
		}
	}
}