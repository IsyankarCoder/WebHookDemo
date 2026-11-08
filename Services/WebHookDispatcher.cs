using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WebHookDemo.Model;
using WebHookDemo.Repositories;

namespace WebHookDemo.Services
{
    public sealed class WebHookDispatcher(HttpClient httpclient,
                                          InMemoryWebHookRepository inMemoryWebHookRepository)
    {
        public async Task DispatchAsync(string eventType, object payload)
        {
            var subscriptions = inMemoryWebHookRepository.GetWebHookByEventType(eventType);
            var payloadTypeInfo = AppJsonSerializerContext.Default.GetTypeInfo(payload.GetType()) as JsonTypeInfo;

            foreach (WebHookSubscription subscription in subscriptions)
            {
                var request = new WebHookEvent(
                    Id: Guid.NewGuid(),
                    EventType: subscription.EventType,
                    SubscriptionId: subscription.Id,
                    TimeStamp: DateTime.UtcNow,
                    Data: JsonSerializer.SerializeToElement(payload, payloadTypeInfo ?? throw new InvalidOperationException($"No source-generated metadata available for '{payload.GetType().FullName}'.")));

                await httpclient.PostAsJsonAsync(subscription.WebHookUrl, request, AppJsonSerializerContext.Default.WebHookEvent);
            }
        }
    }
}
