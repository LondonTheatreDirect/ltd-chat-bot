using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ApiAiSDK.Model;
using LTDBot.Modules.Models;
using LTDBot.Modules.Models.ApiModels;
using Microsoft.Bot.Connector;

namespace LTDBot.Helpers
{
    public static class DialogHelper
    {
        public static Response SimpleResponse(string text)
        {
            return new Response { Text = text };
        }

        public static Response EventRequiredResponse()
        {
            return SimpleResponse("Sorry, I couldn't recognize event");
        }

        public static Response DateRequiredResponse()
        {
            return SimpleResponse("When?");
        }

        public static Response NoPerformanceOrTicketResponse(ConversationState state, Event @event)
        {
            return SimpleResponse($"Sorry, there is no performance of {@event.Name} on {FormatHelper.FormatDateFromToTime(state)}"
                                  + " or we have no tickets left");
        }

        public static CardAction CreateCardAction(string value, string title)
        {
            return new CardAction
            {
                Value = value,
                Type = "imBack",
                Title = title
            };
        }

        public static Response ShowAllResponse(string message, string subtitle, string imageUrl, string title, string cardActionValue, string cardActionTitle)
        {
            return new Response
            {
                Attachments = new List<Attachment>
                {
                    new ThumbnailCard
                    {
                        Title = title,
                        Subtitle = subtitle,
                        Images = new List<CardImage>
                        {
                            new CardImage(imageUrl)
                        },
                        Buttons = new List<CardAction>
                        {
                            CreateCardAction(cardActionValue, cardActionTitle),
                            CreateCardAction("Nevermind", "Nevermind")
                        }
                    }.ToAttachment()
                },
                Text = message
            };
        }

        public static Response DescriptionResponse(string name, string description, string url, AIResponse aiResponse)
        {
            if (string.IsNullOrEmpty(description))
                return SimpleResponse($"Sorry I have no description of {name}");

            var descriptionWithoutHtml = RemoveHtmlEntities(description);
            if (aiResponse.Result.ResolvedQuery.Contains("complete") || descriptionWithoutHtml.Length < 500)
                return SimpleResponse(descriptionWithoutHtml);

            var buttons = new List<CardAction>
            {
                CreateCardAction($"Show me complete description of {name}", "Full description"),
                new CardAction
                {
                    Value = url,
                    Type = "openUrl",
                    Title = "Open in browser"
                },
                CreateCardAction("Nevermind", "Nothing")
            };
            return new Response
            {
                Attachments = new List<Attachment>
                {
                    new ThumbnailCard
                    {
                        Title = name,
                        Subtitle = $"Full description is {descriptionWithoutHtml.Length} characters long.",
                        Buttons = buttons
                    }.ToAttachment()
                },
                Text = descriptionWithoutHtml.Substring(0, 500)
            };
        }

        public static string RemoveHtmlEntities(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : HttpUtility.HtmlDecode(Regex.Replace(text, "<[^>]*>", ""));
        }

        public static string GetBooking2Url(int performanceId, DateTime performanceDate, string eventName, string venueName)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            string url = $"https://www.londontheatredirect.com/booking2/{GenerateUrlSafeString(eventName)}-tickets-at-the-{GenerateUrlSafeString(venueName)}-on-{performanceDate:dddd-d}{FormatHelper.GetDaySuffix(performanceDate)}-{performanceDate:MMMM-yyyy}?performanceId={performanceId}";

            return url;
        }

        public static string GenerateUrlSafeString(string text)
        {
            var str = RemoveDiacritics(text);

            var sb = new StringBuilder();
            var wasHyphen = true;

            foreach (var c in str)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLower(c));
                    wasHyphen = false;
                }
                else if (c != '\'' && !wasHyphen)
                {
                    sb.Append('-');
                    wasHyphen = true;
                }
            }

            // Avoid trailing hyphens
            if (wasHyphen && sb.Length > 0)
                sb.Length--;

            return sb.ToString();
        }

        public static string RemoveDiacritics(string text)
        {
            var bytesText = Encoding.GetEncoding("ISO-8859-8").GetBytes(text);
            return Encoding.UTF8.GetString(bytesText);
        }

        // Gets a folder for URL rewriter that is appropriate for URLs of specified EventTypes
        public static string GetEventTypeUrlFolder(string eventTypeName)
        {
            switch (eventTypeName)
            {
                case "Ballet & Dance":
                    return "ballet";
                case "Concerts":
                    return "concert";
                case "Experiences":
                    return "experience";
                default:
                    return eventTypeName.ToLower();
            }
        }
    }
}