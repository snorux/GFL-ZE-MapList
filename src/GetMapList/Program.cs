using GetMapList.Models;
using Newtonsoft.Json;
using Renci.SshNet;
using System.Collections.Concurrent;

namespace GetMapList
{
    public class Program
    {
        const long MaxSize = 157286400;

        static void Main(string[] args)
        {
            // We standardize the args
            // 0 - Server Address
            // 1 - Port
            // 2 - Username
            // 3 - Key
            var privateKey = new PrivateKeyFile(args[3]);
            using var client = new SftpClient(args[0], Int32.Parse(args[1]), args[2], new[] { privateKey });
            client.Connect();
            client.ChangeDirectory("csgo");

            using var memoryStream = new MemoryStream();
            using var streamReader = new StreamReader(memoryStream);

            client.DownloadFile("./mapcycle_zeronom.txt", memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            string[] output = streamReader.ReadToEnd().Split("\n").Where(x => !x.StartsWith("//")).ToArray();
            Console.WriteLine($"output contains: {output.Length} maps");

            ConcurrentBag<MapModel> maps = new();
            Parallel.ForEach(output, map =>
            {
                var attributes = client.GetAttributes($"./maps/{map}.bsp");

                maps.Add(new MapModel()
                {
                    MapName = map,
                    IsMoreThan150MB = attributes.Size >= MaxSize,
                    FileSize = attributes.Size,
                });
            });

            var results = maps.OrderBy(x => x.MapName);
            Console.WriteLine($"result contains: {results.Count()}");

            File.WriteAllText("maps.json", JsonConvert.SerializeObject(results, Formatting.Indented));
            client.Dispose();
        }
    }
}