using System.Text;
using System.Text.Json;

const string tokenEndpoint =
    "https://authorization.cerner.com/tenants/ec2458f2-1e24-41c8-b71b-0e701af7583d/protocols/oauth2/profiles/smart-v1/token";
const string fhirServerBaseUrl = "https://fhir-open.cerner.com/r4/ec2458f2-1e24-41c8-b71b-0e701af7583d";

// Get credentials from https://code-console.cerner.com/
const string clientId = "your_client_id";
const string secret = "your_client_secret";

var encodedClientIdAndSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));

var requestData =
    "grant_type=client_credentials&scope=system/Patient.read system/Organization.read system/Practitioner.read";

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedClientIdAndSecret);
httpClient.DefaultRequestHeaders.Accept.Add(
    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
var content = new StringContent(requestData, Encoding.UTF8, "application/x-www-form-urlencoded");

var accessTokenResult = await httpClient.PostAsync(tokenEndpoint, content);

if (accessTokenResult.IsSuccessStatusCode)
{
    var responseBody = await accessTokenResult.Content.ReadAsStringAsync();
    var accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseBody);
    Console.WriteLine(accessTokenResponse.AccessToken);

    // After getting Access Token we can fetch resource from the server, In this case Organization
    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Prefer", "respond-async");
    var organizationsResult = await httpClient.GetAsync($"{fhirServerBaseUrl}/Organization");
    if (organizationsResult.IsSuccessStatusCode)
    {
        var organizationsResponseBody = await organizationsResult.Content.ReadAsStringAsync();
        Console.WriteLine(organizationsResponseBody);
    }
    else
    {
        Console.WriteLine(
            $"Could not fetch organizations, Request failed with status code: {organizationsResult.StatusCode}");
    }
}
else
{
    Console.WriteLine("Could not get access token, Request failed with status code: " + accessTokenResult.StatusCode);
}