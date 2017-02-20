using LTDBot.Modules.Models.ApiModels;

namespace LTDBot.Modules.Clients
{
    public class VenueClient : Client
    {
        /// <summary>
        /// Get all venues.
        /// </summary>
        /// <returns></returns>
        public Venues All()
        {
            return Get<Venues>($"{BaseAddress}Venues");
        }
    }
}