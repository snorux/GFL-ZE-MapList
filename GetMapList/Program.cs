using Renci.SshNet;

namespace GetMapList
{
    public class Program
    {
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
            string[] output = streamReader.ReadToEnd().Split("\n").Where(x => !x.StartsWith("//")).OrderBy(x => x).ToArray();

            Parallel.ForEach(output, map =>
            {
                var attributes = client.GetAttributes($"./maps/{map}.bsp");
                Console.WriteLine($"{map} - {attributes.Size} - {Environment.CurrentManagedThreadId}");
            });
        }
    }
}