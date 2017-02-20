using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using LTDBot.Modules.Clients;
using LTDBot.Modules.Logger;
using LTDBot.Modules.Models.ApiAi;
using LTDBot.Modules.Models.ApiModels;

namespace LTDBot.Data
{
    // Api.ai has lists of events and venues to be able to recognize them. This class prepare list with synonyms and send it to api.ai.
    // Synonyms may be part of the name, or full name without "theatre" etc.
    public static class ApiAiEntityUpdater
    {
        public static void UpdateEntities(List<Event> events, List<Venue> venues)
        {
            try
            {
                events = events.Where(e => e.Performances?.Count > 0).OrderBy(e => e.Name).ToList();  // use just events with some performances
                venues = venues.Where(v => events.Any(e => e.VenueId == v.Id)).OrderBy(e => e.Name).ToList();  // use just venues with some event with performances
                ApiAiClient.SendEntity(GenerateEvents(events));
                ApiAiClient.SendEntity(GenerateVenues(venues));
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log($"Exception while updating apiai entities: {e.Message}");
            }
        }

        private static Entity GenerateEvents(IReadOnlyCollection<Event> events)
        {
            var words = CreateWordCount(events.Select(e => e.Name));
            var result = new Entity { Id = ConfigurationManager.AppSettings.Get("ApiAiEventsKey"), Entries = new List<Entry>(), Name = "Event" };
            foreach (var e in events)
            {
                var entry = CreateEntry(e.Name);
                var split = ExtractWords(e.Name);

                AddWordsAsSynonyms(split, words, entry);
                result.Entries.Add(entry);
            }
            return result;
        }

        private static Entity GenerateVenues(IReadOnlyCollection<Venue> venues)
        {
            var words = CreateWordCount(venues.Select(v => v.Name));
            var result = new Entity {Id = ConfigurationManager.AppSettings.Get("ApiAiVenuesKey"), Entries = new List<Entry>(), Name = "Venues"};
            foreach (var venue in venues)
            {
                var entry = CreateEntry(venue.Name);
                var split = ExtractWords(venue.Name);

                if (split.Length > 2 && venue.Name.ToLower().Contains("theatre")) // "Duke of Yorks Theatre" -> "duke of yorks"
                {
                    var nameWithoutTheatre = venue.Name.ToLower().Replace("theatre", "").Trim();
                    entry.Synonyms.Add(WithoutParentheses(nameWithoutTheatre));
                }

                AddWordsAsSynonyms(split, words, entry);
                result.Entries.Add(entry);
            }
            return result;
        }

        private static Entry CreateEntry(string name)
        {
            return new Entry { Value = name, Synonyms = new List<string> { WithoutParentheses(name) } };
        }

        private static string[] ExtractWords(string name)
        {
            var fixedInput = Regex.Replace(name, "[^a-zA-Z ]", string.Empty);
            return fixedInput.ToLower().Split(' ');
        }

        private static void AddWordsAsSynonyms(string[] split, IReadOnlyDictionary<string, int> words, Entry entry)
        {
            // do not allow words that can be mistaken for date, time or another important words for intent recognition
            var blackList = new List<string> { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday",
                "week", "month", "this", "next", "book", "afternoon", "evening", "morning", "noon", "night", "tonight" };

            if (split.Length <= 1) return; // one word names was already added
            foreach (var word in split)
                if (word.Length > 3 && words[word] == 1 && !blackList.Contains(word)) // Word with multiple occurences cannot be used as a synonym. 
                    entry.Synonyms.Add(word);                                         // It would be confusing for entity recognition
        }

        // Count all words.
        private static Dictionary<string, int> CreateWordCount(IEnumerable<string> names)
        {
            var words = new Dictionary<string, int>();

            foreach (var name in names)
            {
                var split = ExtractWords(name);
                foreach (var word in split)
                {
                    if (words.ContainsKey(word))
                        words[word]++;
                    else
                        words.Add(word, 1);
                }
            }
            return words;
        }

        // Synonyms cannot contain parentheses. (Name of entry can)
        private static string WithoutParentheses(string name)
        {
            return name.ToLower().Replace("(", "").Replace(")", "");
        }
    }
}