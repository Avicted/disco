using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Models.Bridge;

class Program
{
    public static ILocalHueClient? client;
    public static IEnumerable<Light>? lights;

    public static List<string> NiceColors = new List<string>() {
        "FF0000",
        "FFFF00",
        "00FF00",
        "00FFFF",
        "FFAA00",
        "55AA33",
        "442299",
    };

    public static string GetNiceColor()
    {
        Random rnd = new Random();
        int index = rnd.Next(NiceColors.Count);
        return NiceColors[index];
    }

    static async Task Main(string[] args)
    {
        await SetupClient();
        await RegisterAppWithBridge();
        await GetAllLights();
        await Disco();
    }

    public static async Task Disco()
    {
        var command = new LightCommand()
        {
            On = true,
            Effect = Effect.None,
        };

        command.SetColor(new RGBColor("FF0000"));
        if (client != null)
        {
            await client.SendCommandAsync(command);

            if (lights != null)
            {
                while (true)
                {
                    command.SetColor(new RGBColor(GetNiceColor()));
                    await client.SendCommandAsync(command, lights.Select(l => l.Id));

                    Thread.Sleep(400);
                }
            }
            else
            {
                Console.WriteLine($"    -> The lights are null");
            }
        }
        else
        {
            Console.WriteLine("     -> The client is null");
        }

    }

    public static async Task SetupClient()
    {
        Console.WriteLine("disco started");

        IBridgeLocator locator = new HttpBridgeLocator(); // Or: LocalNetworkScanBridgeLocator, MdnsBridgeLocator, MUdpBasedBridgeLocator
        IEnumerable<LocatedBridge> bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
        string bridgeIpAddress = "192.168.1.200";

        if (bridgeIpAddress.Length <= 0)
        {
            throw new Exception("Could not find any Hue bridge, is this computer on the same LAN as the Hue bridge?");
        }

        client = new LocalHueClient(bridgeIpAddress);
    }

    public static async Task RegisterAppWithBridge()
    {
        // Make sure the user has pressed the button on the bridge before calling RegisterAsync
        // It will throw an LinkButtonNotPressedException if the user did not press the button
        Console.WriteLine("Press ENTER when you have pressed the button on the bridge.");
        do
        {
            while (!Console.KeyAvailable) { }
        } while (Console.ReadKey(true).Key != ConsoleKey.Enter);

        if (client != null)
        {
            string? appKey = await client.RegisterAsync("disco-hue", "disco-hue-client");
            Console.WriteLine($"appKey: {appKey}");
        }
        else
        {
            Console.WriteLine($"    -> The client is null");
        }

    }

    public static async Task GetAllLights()
    {
        if (client != null)
        {
            lights = await client.GetLightsAsync();
            Console.WriteLine($"Lights found:\n====================================");

        }
        else
        {
            Console.WriteLine($"    -> The client is null");
        }

        if (lights != null)
        {
            foreach (Light light in lights)
            {
                Console.WriteLine($"id: {light.Id}\nname: {light.Name}\nproductId: {light.ProductId}");
                Console.WriteLine("====================================");
            }
        }
        else
        {
            Console.WriteLine($"    -> The lights are null");
        }
    }
}
