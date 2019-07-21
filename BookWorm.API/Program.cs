using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using OrleansSimple;
using System.Threading.Tasks;

namespace BookWorm.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            (await CreateWebHostBuilder(args)).Build().Run();
        }

        public static async Task<IWebHostBuilder> CreateWebHostBuilder(string[] args)
        {
            ISiloHost silo = BuildSiloHost();
            await silo.StartAsync();

            var client = silo.Services.GetRequiredService<IClusterClient>();
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IClusterClient>(client);
                    s.AddSingleton<IGrainFactory>(client);
                })
                .UseStartup<Startup>();
        }

        private static ISiloHost BuildSiloHost()
        {
            return new SiloHostBuilder()
                .UseDashboard()
                .UseLocalhostClustering()
                .AddMemoryGrainStorage("OrleansStorage") // TODO: replace with your storage
                .AddWordsAPIClient()
                .AddStanfordNLPClient()
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(ISimple).Assembly).WithReferences();
                    parts.AddApplicationPart(typeof(SimpleGrain).Assembly).WithReferences();
                })
                .Build();
        }
    }
}
