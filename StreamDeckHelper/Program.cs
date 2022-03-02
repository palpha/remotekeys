using System.Net;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;

Config ReadConfig() =>
	System.Text.Json.JsonSerializer.Deserialize<Config>(
		File.OpenRead( "appsettings.json" ) )
	?? throw new InvalidOperationException( "Unable to read config." );

InputSimulator input = new();
// ReSharper disable once UseDeconstruction
// ReSharper disable once SuggestVarOrType_SimpleTypes
Config config = ReadConfig();

VirtualKeyCode ParseKey( string str ) =>
	str.ToUpperInvariant() switch
		{
			var x when x.Length == 1 => Enum.Parse<VirtualKeyCode>( $"VK_{x}" ),
			"WIN" => VirtualKeyCode.LWIN,
			"ESC" => VirtualKeyCode.ESCAPE,
			"ALT" => VirtualKeyCode.MENU,
			"CTRL" => VirtualKeyCode.CONTROL,
			var x => Enum.Parse<VirtualKeyCode>( x )
		};

IEnumerable<VirtualKeyCode> ParseKeys( IEnumerable<string> keys ) => keys.Select( ParseKey );

void KeyPress( VirtualKeyCode key )
{
	Console.WriteLine( $"Key: {key}." );
	input.Keyboard.KeyPress( key );
}

void ModifiedKeyStroke( IEnumerable<VirtualKeyCode> modifiers, VirtualKeyCode key )
{
	var modifierKeyCodes = modifiers.ToList();
	Console.WriteLine( $"Modifiers: {string.Join( ", ", modifierKeyCodes )}; key: {key}." );
	input.Keyboard.ModifiedKeyStroke( modifierKeyCodes, key );
}

bool TextEntry( string str )
{
	Console.WriteLine( $"Text entry: {str}." );
	input.Keyboard.TextEntry( str );
	return true;
}

bool ParseKeyPresses( string[] keyPresses )
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
			if ( config.EnableText == false )
			{
				throw new InvalidOperationException( "Text input not allowed." );
			}

			var key = ParseKey( parts[0] );
			sequence.Add( () => KeyPress( key ) );
		}
		else
		{
			var modifiers = ParseKeys( parts[..^1] );
			var key = ParseKey( parts[^1] );
			sequence.Add( () => ModifiedKeyStroke( modifiers, key ) );
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
			"text" when config.EnableText => TextEntry( command[1] ),
			"keys" => ParseKeyPresses( command[1..] ),
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