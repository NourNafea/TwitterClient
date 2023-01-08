using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;

namespace RemoveMyTwitterLikes;

public class RemoveMyLikesClient
{
    private RestClient Authenticate()
    {
        var configuration =  new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        var baseUrl = configuration["TwitterApi:BaseURL"];
        var consumerKey = configuration["TwitterApi:ConsumerKey"];
        var consumerSecret = configuration["TwitterApi:ConsumerSecret"];
        var accessToken = configuration["TwitterApi:AccessToken"];
        var accessSecret = configuration["TwitterApi:AccessSecret"];
        
        if (consumerKey != null &&
            consumerSecret != null &&
            accessToken != null &&
            accessSecret != null &&
            baseUrl != null)
        {
            var client = new RestClient();
            var oAuth1 = OAuth1Authenticator.ForAccessToken(
                consumerKey: consumerKey,
                consumerSecret: consumerSecret,
                token: accessToken,
                tokenSecret: accessSecret,
                OAuthSignatureMethod.HmacSha256);

            client.Options.Authenticator = oAuth1;
            return client;
        }

        return null;
    }
    
    private List<string> GetLikedTweets()
    {
        var configuration =  new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var baseUrl = configuration["TwitterApi:BaseURL"];
        var likedTweetsUrl = $"{baseUrl}/liked_tweets";
        var client = Authenticate();
        var request = new RestRequest(likedTweetsUrl);
        request.AddHeader("Content-Type", "application/json");
        var result = client.Execute(request).Content;
        return JObject.Parse(result)["data"]
            .Select(x => x["id"].ToString())
            .ToList();
    }

    public void RemoveMyLikes()
    {
        var client = Authenticate();
        var tweets = GetLikedTweets();
        var configuration =  new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var baseUrl = configuration["TwitterApi:BaseURL"];
        
        foreach (var tweetId in tweets)
        {
            var unlikeRequest = new RestRequest(
                $"{baseUrl}/likes/{tweetId}",
                Method.Delete);
            
            unlikeRequest.AddHeader(
                "Content-Type", 
                "application/json");

            if (client.Execute(unlikeRequest).StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Unliked tweet: " + tweetId);
            }
            else
            {
                Console.WriteLine("Failed to unlike tweet: " + tweetId);
            }
        }
    }
}