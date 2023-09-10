using Hapi.Samples;

Console.WriteLine("Hello, Hapi Server!");

var organizationManager = new OrganizationManager();

await organizationManager.GetAllOrganizationsAsync();
