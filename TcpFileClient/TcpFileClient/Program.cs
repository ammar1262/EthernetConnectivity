using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpFileClient
{
    class Program
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
                        while (totalBytesRead < fileSize)
                        {
                            bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
                            await fs.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                        }
                    }

                    Console.WriteLine("File received and saved.");
                }
            }
        }
    }
}
