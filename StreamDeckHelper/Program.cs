using System.Net;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;

Config ReadConfig() =>
	System.Text.Json.JsonSerializer.Deserialize<Config>(
		File.OpenRead( "appsettings.json" ) )
	?? throw new InvalidOperationException( "Unable to read config." );

InputSimulator input = new();
var config = ReadConfig();

IEnumerable<VirtualKeyCode> ParseModifiers( string[] keys )
{
	foreach ( var key in keys )
	{
		yield return key.ToUpperInvariant() switch
			{
				"CONTROL" => VirtualKeyCode.CONTROL,
				"CTRL" => VirtualKeyCode.CONTROL,
				"SHIFT" => VirtualKeyCode.SHIFT,
				"LWIN" => VirtualKeyCode.LWIN,
				"WIN" => VirtualKeyCode.LWIN,
				var x => throw new InvalidOperationException( $"Invalid modifier {x}." )
			};
	}
}

VirtualKeyCode ParseKey( string str ) =>
	str.ToUpperInvariant() switch
		{
			var x when x[0] == 'F' => Enum.Parse<VirtualKeyCode>( x ),
			var x when x.Length == 1 => Enum.Parse<VirtualKeyCode>( $"VK_{x}" ),
			"ESC" => VirtualKeyCode.ESCAPE,
			"ESCAPE" => VirtualKeyCode.ESCAPE,
			var x => throw new InvalidOperationException( $"Invalid key {x}." )
		};

void LoggedKeyPress( VirtualKeyCode key )
{
	Console.WriteLine( $"Key: {key}." );
	input.Keyboard.KeyPress( key );
}

void LoggedModifiedKeyStroke( IEnumerable<VirtualKeyCode> modifiers, VirtualKeyCode key )
{
	var modifierKeyCodes = modifiers.ToList();
	Console.WriteLine( $"Modifiers: {string.Join( ", ", modifierKeyCodes )}; key: {key}." );
	input.Keyboard.ModifiedKeyStroke( modifierKeyCodes, key );
}

bool LoggedTextEntry( string str )
{
	Console.WriteLine( $"Text entry: {str}." );
	input.Keyboard.TextEntry( str );
	return true;
}

bool ParseKeys( string[] keyPresses )
{
	var sequence = new List<Action>();

	foreach ( var keyPress in keyPresses )
	{
		if ( int.TryParse( keyPress, out var pause ) && pause is > 0 and < 10000 )
		{
			Console.WriteLine( $"Pausing {pause} ms." );
			Thread.Sleep( pause );
			continue;
		}

		var parts = keyPress.Split( '+' );
		if ( parts.Length == 1 )
		{
			var key = ParseKey( parts[0] );
			sequence.Add( () => LoggedKeyPress( key ) );
		}
		else
		{
			var modifiers = ParseModifiers( parts[..^1] );
			var key = ParseKey( parts.Last() );
			sequence.Add( () => LoggedModifiedKeyStroke( modifiers, key ) );
		}
	}

	foreach ( var action in sequence )
	{
		action();
	}

	return true;
}

bool Parse( string[] command ) =>
	command[0] switch
		{
			"text" when config.EnableText => LoggedTextEntry( command[1] ),
			"keys" => ParseKeys( command[1..] ),
			_ => false
		};

var listener = new HttpListener();
listener.Prefixes.Add( config.Prefix );
listener.Start();

Console.WriteLine( $"Listening to {config.Prefix}." );

while ( true )
{
	var ctx = await listener.GetContextAsync();
	var path = ctx.Request.Url?.AbsolutePath;
	var statusCode = 400;
	var statusDescription = "Not allowed";

	if ( path is not null )
	{
		var parts = path.Split( '/' )[1..];
		Console.WriteLine( $"Parts: {string.Join( "; ", parts )}" );
		try
		{
			if ( Parse( parts ) )
			{
				statusCode = 200;
				statusDescription = "OK";
			}
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
}

record Config( string Prefix, bool EnableText );