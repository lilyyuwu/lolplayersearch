﻿using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    // Set your API key here
    const string API_KEY = "APIKEYHERE";

    // Set the base URL for the API
    const string BASE_URL = "https://euw1.api.riotgames.com";

    static async Task<HttpResponseMessage> GetAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Riot-Token", API_KEY);

            HttpResponseMessage response = await client.GetAsync(url);

            return response;
        }
    }

    static async Task<dynamic> GetSummonerByName(string summonerName)
    {
        // Format the URL for the API call
        string url = $"{BASE_URL}/lol/summoner/v4/summoners/by-name/{summonerName}";

        // Make the API call
        HttpResponseMessage response = await GetAsync(url);

        // Check if the API call was successful
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            return null;
        }

        // Return the summoner data
        return await response.Content.ReadFromJsonAsync<dynamic>();
    }

    static async Task<HttpResponseMessage> GetRankedStatsBySummonerId(string summonerId)
    {
        // Format the URL for the API call
        string url = $"{BASE_URL}/lol/league/v4/entries/by-summoner/{summonerId}";

        // Set the headers for the API call
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Riot-Token", API_KEY);

        // Make the API call
        HttpResponseMessage response = await client.GetAsync(url);

        // Check if the API call was successful
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            return null;
        }

        // Return the response
        return response;
    }





static async Task Main(string[] args)
{
    // Prompt the user to enter a summoner name
    Console.Write("Enter a summoner name: ");
    string summonerName = Console.ReadLine();

    // Call the 'GetSummonerByName' function with the given summoner name and await its response
    dynamic summonerData = await GetSummonerByName(summonerName);

    // Check if the summoner data was successfully retrieved
    if (summonerData.ValueKind != JsonValueKind.Null)
    {
        // Retrieve the summoner name from the summoner data object
        string name;
        if (summonerData.TryGetProperty("name", out JsonElement nameElement))
        {
            name = nameElement.GetString();
            Console.WriteLine($"Summoner name: {name}");
        }
        else
        {
            Console.WriteLine("Summoner name not found.");
        }

        // Retrieve the summoner level from the summoner data object
        int summonerLevel;
        if (summonerData.TryGetProperty("summonerLevel", out JsonElement levelElement))
        {
            summonerLevel = levelElement.GetInt32();
            Console.WriteLine($"Summoner level: {summonerLevel}");
        }
        else
        {
            Console.WriteLine("Summoner level not found.");
        }

        // Retrieve the summoner ID from the summoner data object and use it to get ranked stats
        string summonerId;
        if (summonerData.TryGetProperty("id", out JsonElement idElement))
        {
            summonerId = idElement.GetString();
            HttpResponseMessage rankedStatsResponse = await GetRankedStatsBySummonerId(summonerId);
            if (rankedStatsResponse != null)
            {
                // Deserialize the JSON response and loop through the ranked stats data
                string rankedStatsContent = await rankedStatsResponse.Content.ReadAsStringAsync();
                JsonElement[] rankedStatsData = JsonSerializer.Deserialize<JsonElement[]>(rankedStatsContent);

                foreach (JsonElement stat in rankedStatsData)
                {
                    // Retrieve the queue type, tier, rank, and LP from each ranked stat
                    string queueType;
                    if (stat.TryGetProperty("queueType", out JsonElement queueTypeElement))
                    {
                        queueType = queueTypeElement.GetString();
                    }
                    else
                    {
                        queueType = "Queue type not found.";
                    }

                    string tier;
                    if (stat.TryGetProperty("tier", out JsonElement tierElement))
                    {
                        tier = tierElement.GetString();
                    }
                    else
                    {
                        tier = "Tier not found.";
                    }

                    string rank;
                    if (stat.TryGetProperty("rank", out JsonElement rankElement))
                    {
                        rank = rankElement.GetString();
                    }
                    else
                    {
                        rank = "Rank not found.";
                    }

                    int leaguePoints;
                    if (stat.TryGetProperty("leaguePoints", out JsonElement lpElement))
                    {
                        leaguePoints = lpElement.GetInt32();
                    }
                    else
                    {
                        leaguePoints = -1;
                    }

                    // Display the ranked stats for the current queue type
                    Console.WriteLine($"{queueType}: {tier} {rank} ({leaguePoints} LP)");
                }
            }
            else
            {
                Console.WriteLine("Ranked stats not found.");
            }
        }
        else
        {
            Console.WriteLine("Summoner ID not found.");
        }
    }
}


}
