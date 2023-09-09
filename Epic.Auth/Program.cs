using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// OAuth tutorial : https://fhir.epic.com/Documentation?docId=oauth2

// Sandbox Test Data : https://fhir.epic.com/Documentation?docId=testpatients

const string tokenUrl = "https://fhir.epic.com/interconnect-fhir-oauth/oauth2/token";
const string clientId = "your_client_id";
const string pathToPrivateKey = "path_to_your_private_key";
const string fhirBaseUrl = "https://fhir.epic.com/interconnect-fhir-oauth/api/FHIR/R4";

var accessTokenResponse = await GetAccessToken();

if (accessTokenResponse != null)
{
    await PrintPatientInformation(accessTokenResponse.AccessToken);
}

async Task<AccessTokenResponse?> GetAccessToken()
{
    const string header = "{\"alg\": \"RS384\",\"typ\": \"JWT\"}";

    var currentDateTime = DateTimeOffset.UtcNow;
    var unixTime = (currentDateTime.AddMinutes(3)).ToUnixTimeSeconds();

    var payload =
        "{\"iss\": \"" + clientId +
        "\",\"sub\": \"" + clientId +
        "\",\"aud\": \"https://fhir.epic.com/interconnect-fhir-oauth/oauth2/token\",\"jti\": \"" + Guid.NewGuid() +
        "\",\"exp\": " + unixTime +
        "}";

    var inputString = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(header)).Replace('+', '-')
        .Replace('/', '_')
        .TrimEnd('=')}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).Replace('+', '-')
        .Replace('/', '_')
        .TrimEnd('=')}";

    var privateKeyText =
        File.ReadAllText(pathToPrivateKey);
    var rsa = new RSACryptoServiceProvider();

    rsa.ImportFromPem(privateKeyText);
    var inputBytes = Encoding.UTF8.GetBytes(inputString);

    var sha384 = SHA384.Create();
    var hashBytes = sha384.ComputeHash(inputBytes);

// Sign the hash using the private key
    var signatureBytes = rsa.SignHash(hashBytes, CryptoConfig.MapNameToOID("SHA384"));

// Convert the signature to a base64 string
    var signatureBase64 = Convert.ToBase64String(signatureBytes).Replace('+', '-')
        .Replace('/', '_')
        .TrimEnd('=');


    var jwt = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(header)).Replace('+', '-')
        .Replace('/', '_')
        .TrimEnd('=')}.{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).Replace('+', '-')
        .Replace('/', '_')
        .TrimEnd('=')}.{signatureBase64}";

    using var client = new HttpClient();
    var content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("grant_type", "client_credentials"),
        new KeyValuePair<string, string>("client_assertion_type",
            "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
        new KeyValuePair<string, string>("client_assertion", jwt)
    });

    var response = await client.PostAsync(tokenUrl, content);

    if (response.IsSuccessStatusCode)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        var accessTokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseBody);
        Console.WriteLine(accessTokenResponse.AccessToken);

        return accessTokenResponse;
    }

    Console.WriteLine("Request failed with status code: " + response.StatusCode);
    return default;
}

async Task PrintPatientInformation(string accessToken)
{
    using var client = new HttpClient();
    // Set the authorization header with the access token
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/fhir+json"));
    client.DefaultRequestHeaders.TryAddWithoutValidation("Prefer", "respond-async");
    
    var response = await client.GetAsync($"{fhirBaseUrl}/Patient?_id=erXuFYUfucBZaryVksYEcMg3");
    
    if (response.IsSuccessStatusCode)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
    }
    else
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
    }
}