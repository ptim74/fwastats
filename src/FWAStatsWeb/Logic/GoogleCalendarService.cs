using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FWAStatsWeb.Logic
{
    public interface IGoogleCalendarService
    {
        Task<IList<Event>> GetEvents(string calendarId, DateTime startDate, DateTime endTime);
    }
    public class GoogleCalendarService : GoogleBaseService, IGoogleCalendarService
    {
        public GoogleCalendarService(
            IOptions<GoogleServiceOptions> googleOptions) : base(googleOptions)
        {
        }

        private CalendarService calendarService = null;

        protected CalendarService Service
        {
            get
            {
                calendarService ??= new CalendarService(Initializer(new[] { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarEvents }));
                return calendarService;
            }
        }

        protected async Task EnsureCalendarExists(string calendarId)
        {
            try
            {
                await Service.Calendars.Get(calendarId).ExecuteAsync();
            }
            catch(Exception)
            {
                await Service.CalendarList.Insert(new CalendarListEntry { Id = calendarId }).ExecuteAsync();
            }
        }

        public async Task<IList<Event>> GetEvents(string calendarId, DateTime startDate, DateTime endTime)
        {
            var events = new List<Event>();
            await EnsureCalendarExists(calendarId);
            string nextPageToken = null;
            do
            {
                var eventRequest = Service.Events.List(calendarId);
                eventRequest.TimeMinDateTimeOffset = startDate;
                eventRequest.TimeMaxDateTimeOffset = endTime;
                eventRequest.PageToken = nextPageToken;
                var eventResponse = await eventRequest.ExecuteAsync();
                events.AddRange(eventResponse.Items);
                nextPageToken = eventResponse.NextPageToken;
            }
            while (nextPageToken != null);
            return events;
        }
    }
}
