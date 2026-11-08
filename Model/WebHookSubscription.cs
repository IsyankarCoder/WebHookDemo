namespace WebHookDemo.Model
{
    public sealed record WebHookSubscription(Guid Id, string EventType, string WebHookUrl, DateTime CreatedOnUtc);

    public sealed record CreateWebhookRequest(string EventType, string WebHookUrl);
    
    
}
