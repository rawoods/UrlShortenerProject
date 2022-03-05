using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using LiteDB;
using UrlShortener;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder =>
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<ILiteDatabase, LiteDatabase>((sp) => new LiteDatabase("shorten-url.db"));
            services.AddRouting();
        })
        .Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapPost("/shortenUrl", Shortener.ShortenLongUrl);
                endpoints.MapFallback(Shortener.HandleUrl);
            });
            app.UseStaticFiles();
        });
    })
    .Build();

await host.RunAsync();


