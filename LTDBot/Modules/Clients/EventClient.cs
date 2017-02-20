using System;
using LTDBot.Modules.Models.ApiModels;

namespace LTDBot.Modules.Clients
{
    [Serializable]
    public class EventClient : Client
    {
        /// <summary>
        /// Get all events from London Theatre Direct API.
        /// </summary>
        /// <returns> All events. </returns>
        public Events AllEvents()
        {
            return Get<Events>($"{BaseAddress}Events");
        }

        /// <summary>
        /// Get all tickets in specific date range for event.
        /// </summary>
        /// <param name="id"> Event ID. </param>
        /// <param name="from"> Date from [YYYY-MM-DD]. </param>
        /// <param name="to"> Date to [YYYY-MM-DD]. </param>
        /// <param name="nbOfTickets"> Number of tickets. </param>
        /// <returns>  </returns>
        public AvailableTicketsResult GetAvailableTickets(int id, DateTime from, DateTime to, int nbOfTickets)
        {
            var result =
                Get<AvailableTicketsResult>(
                    $"{BaseAddress}Events/{id}/AvailableTickets",
                    $"?dateFrom={from:yyyy-MM-dd}" +
                    $"&dateTo={to:yyyy-MM-dd}" +
                    $"&nbOfTickets={nbOfTickets}" +
                    "&consecutiveSeatsOnly=true");
            return result;
        }

        /// <summary>
        /// Get all reviews for specific event.
        /// </summary>
        /// <param name="eventId"> Event ID.</param>
        /// <returns>  </returns>
        public ReviewsSummary GetReviews(int eventId)
        {
            return Get<ReviewsSummary>($"{BaseAddress}Events/{eventId}/Reviews");
        }

        /// <summary>
        /// Get event performances.
        /// </summary>
        /// <param name="eventId"> Event ID. </param>
        /// <returns>  </returns>
        public Event GetEvent(int eventId)
        {
            return Get<Event>($"{BaseAddress}Events/{eventId}/Performances");
        }

        /// <summary>
        /// Get all event types.
        /// </summary>
        /// <returns></returns>
        public EventTypes AllTypes()
        {
            return Get<EventTypes>($"{BaseAddress}System/EventTypes");
        }
    }
}