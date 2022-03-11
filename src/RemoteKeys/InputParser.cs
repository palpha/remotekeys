using InputSimulatorStandard.Native;

namespace RemoteKeys;

// ReSharper disable CheckNamespace
public abstract record Instruction;
public record Delay( int Milliseconds ) : Instruction;
public record TextEntry( string Text ) : Instruction;
public record ModifiedKeyPress( IEnumerable<VirtualKeyCode> Modifiers, VirtualKeyCode Key ) : Instruction;
public record KeyPress( VirtualKeyCode Key ) : Instruction;

public class InputParser
{
	private VirtualKeyCode ParseKey( string str ) =>
		str.ToUpperInvariant() switch
			{
				var x when x.Length == 1 => Enum.Parse<VirtualKeyCode>( $"VK_{x}" ),
				"WIN" => VirtualKeyCode.LWIN,
				"ESC" => VirtualKeyCode.ESCAPE,
				"ALT" => VirtualKeyCode.MENU,
				"CTRL" => VirtualKeyCode.CONTROL,
				var x => Enum.Parse<VirtualKeyCode>( x )
			};

	private IEnumerable<VirtualKeyCode> ParseKeys( IEnumerable<string> keys ) => keys.Select( ParseKey );

	private enum Mode
	{
		Normal,
		Text,
		Delay
	}

	public IEnumerable<Instruction> Parse( IEnumerable<string> arguments )
	{
		var mode = Mode.Normal;

		foreach ( var argument in arguments )
		{
			if ( mode == Mode.Text )
			{
				yield return new TextEntry( argument );
				mode = Mode.Normal;
				continue;
			}

			if ( argument == "text" )
			{
				mode = Mode.Text;
				continue;
			}

			if ( mode == Mode.Delay )
			{
				if ( int.TryParse( argument, out var pause ) && pause is > 0 and <= 10000 )
				{
					yield return new Delay( pause );
				}

				mode = Mode.Normal;
				continue;
			}

			if ( argument == "delay" )
			{
				mode = Mode.Delay;
				continue;
			}

			var parts = argument.Split( '+' );
			if ( parts.Length == 1 )
			{
				var key = ParseKey( parts[0] );
				yield return new KeyPress( key );
			}
			else
			{
				var modifiers = ParseKeys( parts[..^1] );
				var key = ParseKey( parts[^1] );
				yield return new ModifiedKeyPress( modifiers, key );
			}
		}
	}
}