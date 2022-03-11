public record Config( string Prefix, bool EnableText )
{
	public static Config Read() =>
		System.Text.Json.JsonSerializer.Deserialize<Config>(
			File.OpenRead( "appsettings.json" ) )
		?? throw new InvalidOperationException( "Unable to read config." );
}