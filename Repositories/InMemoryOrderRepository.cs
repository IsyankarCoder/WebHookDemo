using WebHookDemo.Model;

namespace WebHookDemo.Repositories
{
    public class InMemoryOrderRepository
    {
        private readonly List<Order> _orders = [];
        public InMemoryOrderRepository() { }


        public void AddOrder(Order order)
        {
            _orders.Add(order);
        }

        public IReadOnlyList<Order> GetOrders() { return _orders; }
    }

    public class InMemoryWebHookRepository
    {
        private readonly List<WebHookSubscription> _webhooks = [];
        public InMemoryWebHookRepository() { }


        public void AddWebHook(WebHookSubscription ws)
        {
            _webhooks.Add(ws);
        }

        public IReadOnlyList<WebHookSubscription> GetWebHooks() { return _webhooks; }

        public IReadOnlyList<WebHookSubscription> GetWebHookByEventType(string eventType)
        {
            return _webhooks.Where(x => x.EventType == eventType).ToList().AsReadOnly();
        }
    }
}
