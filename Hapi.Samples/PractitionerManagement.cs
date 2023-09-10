using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;

namespace Hapi.Samples;

public class PractitionerManagement
{
    private const string HapiServerBaseUrl = "https://hapi.fhir.org/baseR4";

    private readonly FhirClient _client;

    public PractitionerManagement()
    {
        _client = new FhirClient(HapiServerBaseUrl);
    }
    
    /// <summary>
    /// Prints all Practitioners
    /// </summary>
    public async Task GetAllPractitionersAsync(DateTime? lastUpdatedTime = null, string? organizationId = null)
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

            if (!string.IsNullOrEmpty(organizationId))
            {
                searchParams.Add($"organization=Organization/{organizationId}");
            }

            var practitionerRoleBundle = await _client.SearchAsync<PractitionerRole>(searchParams.ToArray());

            // Check if there are any practitioner role in the current page
            if (practitionerRoleBundle?.Entry.Count == 0)
            {
                break; // No more practitioner role, exit the loop
            }

            foreach (var practitionerRole in practitionerRoleBundle.Entry.Select(item => (PractitionerRole)item.Resource))
            {
                var practitioner = await _client.ReadAsync<Practitioner>(practitionerRole.Practitioner?.Url);
                
                Console.WriteLine(
                    $"Id : {practitioner.Id} IsActive : {practitioner.Active} Gender : {practitioner.Gender}");
            }

            page++; // Move to the next page
        }
    }
}