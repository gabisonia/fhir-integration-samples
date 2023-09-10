﻿using Hapi.Samples;

Console.WriteLine("Hello, Hapi Server!");

var organizationManager = new OrganizationManager();
await organizationManager.GetAllOrganizationsAsync();


var practitionerManager = new PractitionerManagement();
// test organizationId 37751
await practitionerManager.GetAllPractitionersAsync();

var scheduleManager = new ScheduleManager();
// test practitionerId 37751
await scheduleManager.FetchSchedulesAndTimeSlotsForPractitionerAsync("1566020");