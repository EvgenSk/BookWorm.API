using GrainInterfaces;
using Orleans.Providers;
using System;
using System.Threading.Tasks;

namespace Grains
{
    [StorageProvider(ProviderName = "OrleansStorage")]
    public class PersistenceGrain : Orleans.Grain<SimpleClass>, IPersistence
    {
        Task<DateTime> IPersistence.GetCreationTime() => Task.FromResult(State.CreationTime);
    }

    public class SimpleClass
    {
        public DateTime CreationTime { get; private set; }
        public SimpleClass(DateTime creationTime)
        {
            CreationTime = creationTime;
        }

        public SimpleClass(): this(DateTime.UtcNow)
        {
        }
    }
}
