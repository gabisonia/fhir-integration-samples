using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;

namespace Hapi.Samples;

public class OrganizationManager
{
    private const string HapiServerBaseUrl = "https://hapi.fhir.org/baseR4";

    private readonly FhirClient _client;

    public OrganizationManager()
    {
        _client = new FhirClient(HapiServerBaseUrl);
    }

    /// <summary>
    /// Prints all organizations
    /// </summary>
    public async Task GetAllOrganizationsAsync(DateTime? lastUpdatedTime = null)
    {
        var page = 1;
        var pageSize = 100;

        while (true)
        {
            var searchParams = new List<string>
            {
                $"_count={pageSize}",
                $"_getpagesoffset={pageSize * (page - 1)}"
            };

            if (lastUpdatedTime.HasValue)
            {
                searchParams.Add($"_lastUpdated=gt{lastUpdatedTime.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")}");
            }

            var organizationsBundle = await _client.SearchAsync<Organization>(searchParams.ToArray());

            // Check if there are any organizations in the current page
            if (organizationsBundle?.Entry.Count == 0)
            {
                break; // No more organizations, exit the loop
            }

            foreach (var organization in organizationsBundle.Entry.Select(item => (Organization)item.Resource))
            {
                Console.WriteLine($"Organization: {organization.Id} Name : {organization.Name}");
            }

            page++; // Move to the next page
        }
    }
}