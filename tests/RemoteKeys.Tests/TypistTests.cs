using Microsoft.Extensions.Logging;

namespace RemoteKeys.Tests;

public class TypistTests
{
	private Mock<IKeyboardSimulator> MockKeyboard { get; } = new();
	private Typist Sut { get; }

	public TypistTests() => Sut = new( MockKeyboard.Object, Mock.Of<ILogger<Typist>>() );

	[Fact]
	public void When_fed_instruction()
	{
		Sut.Feed( new[] { new KeyPress( VirtualKeyCode.VK_A ) } );

		MockKeyboard.Verify( x => x.KeyPress( VirtualKeyCode.VK_A ), Times.Once() );
	}

	private record StrangeInstruction : Instruction;

	[Fact]
	public void When_fed_strange_instruction() =>
		Sut.Invoking( x => x.Feed( new[] { new StrangeInstruction() } ) )
			.Should().Throw<UnfamiliarInstructionException>()
			.Which.Instruction.Should().BeOfType<StrangeInstruction>();

	[Fact]
	public void When_fed_many_instructions()
	{
		Sut.Feed( new Instruction[]
			{
				new KeyPress( VirtualKeyCode.VK_A ),
				new KeyPress( VirtualKeyCode.VK_B ),
				new KeyPress( VirtualKeyCode.VK_C ),
				new Delay( 100 ),
				new ModifiedKeyPress(
					new[] { VirtualKeyCode.BACK, VirtualKeyCode.ACCEPT },
					VirtualKeyCode.BROWSER_BACK ),
				new TextEntry( "foobar" ),
				new KeyPress( VirtualKeyCode.VK_A )
			} );

		MockKeyboard.Verify( x => x.KeyPress( VirtualKeyCode.VK_A ), Times.Exactly( 2 ) );
		MockKeyboard.Verify( x => x.KeyPress( VirtualKeyCode.VK_B ), Times.Once() );
		MockKeyboard.Verify( x => x.KeyPress( VirtualKeyCode.VK_C ), Times.Once() );
		MockKeyboard.Verify( x => x.Sleep( 100 ), Times.Once() );
		MockKeyboard.Verify( x => x.ModifiedKeyStroke(
			new[] { VirtualKeyCode.BACK, VirtualKeyCode.ACCEPT },
			VirtualKeyCode.BROWSER_BACK ), Times.Once() );
		MockKeyboard.Verify( x => x.TextEntry( "foobar" ), Times.Once );
	}
}