namespace RemoteKeys.Tests;

public class InputParserTests
{
	[Theory]
	[MemberData( nameof( GenerateInput ) )]
	public void When_fed_valid_input( string[] input, IEnumerable<Instruction> expected )
	{
		var sut = new InputParser();
		var result = sut.Parse( input );
		result.Should().BeEquivalentTo( expected, x => x.RespectingRuntimeTypes() );
	}

	private static IEnumerable<object[]> GenerateInput()
	{
		// simple keys:
		yield return AsArguments( "a", new KeyPress( VirtualKeyCode.VK_A ) );
		yield return AsArguments( "0", new KeyPress( VirtualKeyCode.VK_0 ) );

		// aliases:
		yield return AsArguments( "ctrl", new KeyPress( VirtualKeyCode.CONTROL ) );
		yield return AsArguments( "alt", new KeyPress( VirtualKeyCode.MENU ) );
		yield return AsArguments( "win", new KeyPress( VirtualKeyCode.LWIN ) );
		yield return AsArguments( "esc", new KeyPress( VirtualKeyCode.ESCAPE ) );
		yield return AsArguments( "enter", new KeyPress( VirtualKeyCode.RETURN ) );

		// modifiers:
		yield return AsArguments( "control", new KeyPress( VirtualKeyCode.CONTROL ) );
		yield return AsArguments( "menu", new KeyPress( VirtualKeyCode.MENU ) );

		// modified key presses:
		yield return AsArguments(
			"ctrl+w",
			new ModifiedKeyPress( new[] { VirtualKeyCode.CONTROL }, VirtualKeyCode.VK_W ) );
		yield return AsArguments(
			"ctrl+shift",
			new ModifiedKeyPress( new[] { VirtualKeyCode.CONTROL }, VirtualKeyCode.SHIFT ) );
		yield return AsArguments(
			"ctrl+shift+o",
			new ModifiedKeyPress(
				new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT, },
				VirtualKeyCode.VK_O ) );

		// delays:
		yield return AsArguments( "delay", "-1", Array.Empty<Instruction>() );
		yield return AsArguments( "delay", "0", Array.Empty<Instruction>() );
		yield return AsArguments( "delay", "1", new Delay( 1 ) );
		yield return AsArguments( "delay", "10000", new Delay( 10000 ) );
		yield return AsArguments( "delay", "10001", Array.Empty<Instruction>() );

		// text:
		yield return AsArguments( "text", "foobar", new TextEntry( "foobar" ) );
	}

	private static object[] AsArguments( string input, params Instruction[] expected ) =>
		new object[] { new[] { input }, expected };

	private static object[] AsArguments( string input1, string input2, params Instruction[] expected ) =>
		new object[] { new[] { input1, input2 }, expected };
}