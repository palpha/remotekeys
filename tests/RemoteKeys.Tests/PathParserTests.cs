namespace RemoteKeys.Tests;

// ReSharper disable StringLiteralTypo
public class PathParserTests
{
	private PathParser Sut { get; } = new();

	[Fact]
	public void When_fed_valid_path()
	{
		var result = Sut.Parse( "/ctrl+s" );

		result.Should().BeEquivalentTo( "ctrl+s" );
	}

	private const string GARBAGE_CHARS = "abcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+/\\";
	private static Regex Valid { get; } = new( @"^[a-z\d+/]+$" );

	[Fact]
	public void When_fed_specific_garbage()
	{
		const string GARBAGE = "kj5i0z+obqg";
		var result = Sut.Parse( GARBAGE );
		result.Should().NotBeEmpty();
	}

	[Theory]
	[MemberData( nameof( GenerateGarbage ), 100 )]
	public void When_fed_garbage( string garbage )
	{
		var result = Sut.Parse( garbage );

		if ( Valid.IsMatch( garbage ) )
		{
			result.Should().NotBeEmpty();
		}
		else
		{
			result.Should().BeEmpty();
		}
	}

	private static Random Rnd { get; } = new();

	private static IEnumerable<object[]> GenerateGarbage( int tests )
	{
		while ( tests-- > 0 )
		{
			var length = Rnd.Next( 1, 100 );
			yield return new object[]
				{
					new string(
						Enumerable.Repeat( GARBAGE_CHARS, length )
							.Select( x => x[Rnd.Next( x.Length )] )
							.ToArray() )
				};
		}
	}
}