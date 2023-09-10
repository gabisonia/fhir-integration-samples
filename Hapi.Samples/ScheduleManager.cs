using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;

namespace Hapi.Samples;

public class ScheduleManager
{
    private const string HapiServerBaseUrl = "https://hapi.fhir.org/baseR4";

    private readonly FhirClient _client;

    public ScheduleManager()
    {
        _client = new FhirClient(HapiServerBaseUrl);
    }

    /// <summary>
    /// Prints all organizations
    /// </summary>
    public async Task FetchSchedulesAndTimeSlotsForPractitionerAsync(string practitionerId)
    {
        var page = 1;
        var pageSize = 100;

        while (true)
        {
            var searchParams = new List<string>
            {
                $"_count={pageSize}",
                $"actor=Practitioner/{practitionerId}",
                $"_getpagesoffset={pageSize * (page - 1)}"
            };

            var scheduleBundle = await _client.SearchAsync<Schedule>(searchParams.ToArray());

            // Check if there are any schedules in the current page
            if (scheduleBundle?.Entry.Count == 0)
            {
                break; // No more schedules, exit the loop
            }

            foreach (var schedule in scheduleBundle.Entry.Select(item => (Schedule)item.Resource))
            {
                if (schedule.Actor != null)
                {
                    foreach (var actor in schedule.Actor)
                    {
                        Console.WriteLine(actor.Url);
                    }
                }
                else
                {
                    Console.WriteLine("No Actor found");
                }
                
                var slotPage = 1; 
                var slotPageSize = 100;
                
                while (true)
                {
                    var slotBundle = await _client.SearchAsync<Slot>(new[]
                    {
                        $"_count={slotPageSize}",
                        $"schedule={schedule.Id}",
                        $"_getpagesoffset={slotPageSize * (slotPage - 1)}",
                    });

                    if (slotBundle.Entry.Count == 0)
                    {
                        break;
                    }

                    foreach (var slot in slotBundle.Entry.Select(slotItem => (Slot)slotItem.Resource))
                    {
                        Console.WriteLine($"Start : {slot.Start} End : {slot.End}");
                    }

                    slotPage++; // Move to the next page
                }
            }

            page++; // Move to the next page
        }
    }
}