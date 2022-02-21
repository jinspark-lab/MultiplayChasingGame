using System.Net.Http;
using System.Text;

public class WebClient
{
    private static readonly HttpClient client = new HttpClient();

    public delegate void ResponseCallback(string response);

    public static async void SendGet(string url, ResponseCallback callback)
    {
        var response = await client.GetStringAsync(url);

        callback(response);
    }

    public static async void SendPost(string url, string jsonBody, ResponseCallback callback)
    {
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        var responseString = response.Content.ReadAsStringAsync().Result;

        callback(responseString);
    }
}
