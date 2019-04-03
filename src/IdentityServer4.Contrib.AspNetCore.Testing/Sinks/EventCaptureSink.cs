using System.Threading.Tasks;
using IdentityServer4.Contrib.AspNetCore.Testing.Misc;
using IdentityServer4.Events;
using IdentityServer4.Services;

namespace IdentityServer4.Contrib.AspNetCore.Testing.Sinks
{
    internal class EventCaptureSink : IEventSink
    {
        private readonly IdentityServerEventCaptureStore identityServerEventCaptureStore;

        public EventCaptureSink(IdentityServerEventCaptureStore identityServerEventCaptureStore)
        {
            this.identityServerEventCaptureStore = identityServerEventCaptureStore;
        }

        public Task PersistAsync(Event @event)
        {
            this.identityServerEventCaptureStore.AddEvent(@event);
            return Task.CompletedTask;
        }
    }
}