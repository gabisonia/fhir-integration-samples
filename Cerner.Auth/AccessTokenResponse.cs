using System.Text.Json.Serialization;

public class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}