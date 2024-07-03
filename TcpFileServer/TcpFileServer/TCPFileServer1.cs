using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;

class TcpFileServer1
{
    const int port = 9000;
    const int bufferSize = 81920; // 80 KB buffer size

    static async Task Main(string[] args)
    {
        TcpListener listener = null;
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine("Server is listening...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client); // Fire and forget
            }
        }
        finally
        {
            listener?.Stop();
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (NetworkStream ns = client.GetStream())
        {
            Console.WriteLine("Client connected.");

            // Path to the file to be sent
            string filePath = "path/to/your/abc.zip";
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            // Send the file size first
            byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
            await ns.WriteAsync(fileSizeBytes, 0, fileSizeBytes.Length);

            // Send the file data in chunks
            byte[] buffer = new byte[bufferSize];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                int bytesRead;
                Stopwatch stopwatch = Stopwatch.StartNew();
                long totalBytesSent = 0;

                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ns.WriteAsync(buffer, 0, bytesRead);
                    totalBytesSent += bytesRead;

                    // Calculate and display throughput
                    double secondsElapsed = stopwatch.Elapsed.TotalSeconds;
                    double throughputMbps = (totalBytesSent * 8) / (secondsElapsed * 1_000_000);
                    Console.WriteLine($"Throughput: {throughputMbps:F2} Mbps");
                }

                stopwatch.Stop();
            }

            Console.WriteLine("File sent.");
        }
    }
}
