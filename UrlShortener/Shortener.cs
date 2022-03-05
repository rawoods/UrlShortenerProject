using System;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener
{
    public static class Shortener
    {
        //endpoint required method of type Task
        public static Task ShortenLongUrl(HttpContext context)
        {
            //create the instance of our NoSQL db
            var db = context.RequestServices.GetService<ILiteDatabase>();
            var collection = db?.GetCollection<ShortenUrl>(nameof(ShortenUrl));

            
                context.Request.Form.TryGetValue("longUrl", out var formData);
                var longUrl = formData.ToString();

                //find if the url has already been shortened - otherwise create new 
                var entry = collection?.Find(p => p.LongUrl == longUrl).FirstOrDefault();

                if (entry is null)
                {
                    entry = new ShortenUrl
                    {
                        LongUrl = longUrl
                    };
                    collection?.Insert(entry);
                }

                var url = entry.GetUrl();
                
                //getting back our host as the prefix to the shortened url
                var response = $"{context.Request.Scheme}://{context.Request.Host}/{url}";
                context.Response.Redirect($"/#{response}");
                return Task.CompletedTask;
            }
        

        public static Task HandleUrl(HttpContext context)
        {
            //make sure if we hit the default root that we go to index page
            if (context.Request.Path == "/")
            {
                return context.Response.SendFileAsync("wwwroot/index.htm");
            }

            //default to home if entry is null
            var redirect = "/";

            var db = context.RequestServices.GetService<ILiteDatabase>();
            var collection = db?.GetCollection<ShortenUrl>();

            var path = context.Request.Path.ToUriComponent().Trim('/');
            var id = ShortenUrl.GetId(path);
            var entry = collection?.Find(p => p.Id == id).SingleOrDefault();

            if (entry is not null)
            {
                redirect = entry.LongUrl;
            }

            context.Response.Redirect(redirect);
            return Task.CompletedTask;
        }
        public class ShortenUrl
        {
            public int Id { get; set; }
            public string LongUrl { get; set; }

            //generates our shortened urls
            public string GetUrl()
            {
                return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(Id));
            }

            public static int GetId(string url)
            {
                return BitConverter.ToInt32(WebEncoders.Base64UrlDecode(url));
            }


        }
    }
}
