using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;

class TcpFileClient1s
{
    const string serverIp = "192.168.1.1"; // Replace with server's IP address
    const int port = 9000;
    const int bufferSize = 81920; // 80 KB buffer size

    static async Task Main(string[] args)
    {
        using (TcpClient client = new TcpClient())
        {
            await client.ConnectAsync(serverIp, port);
            using (NetworkStream ns = client.GetStream())
            {
                Console.WriteLine("Connected to server.");

                // Read the file size first
                byte[] fileSizeBytes = new byte[8];
                await ns.ReadAsync(fileSizeBytes, 0, fileSizeBytes.Length);
                long fileSize = BitConverter.ToInt64(fileSizeBytes, 0);

                // Path to save the received file
                string savePath = "path/to/save/abc.zip";
                byte[] buffer = new byte[bufferSize];
                long totalBytesRead = 0;

                using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
                {
                    int bytesRead;
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    while (totalBytesRead < fileSize)
                    {
                        bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
                        await fs.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        // Calculate and display throughput
                        double secondsElapsed = stopwatch.Elapsed.TotalSeconds;
                        double throughputMbps = (totalBytesRead * 8) / (secondsElapsed * 1_000_000);
                        Console.WriteLine($"Throughput: {throughputMbps:F2} Mbps");
                    }

                    stopwatch.Stop();
                }

                Console.WriteLine("File received and saved.");
            }
        }
    }
}
