using System;
using System.IO;
using System.Net;
using System.Threading;
public static class Program
{
    public static void Main(string[] args)
    {
        args = new string[1] { "HTTPFileHost.exe.config" };

        if (args.Length != 1)
        {
            Console.WriteLine("Usage: HTTPFileHost.exe <file_path>");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        // Set up the HTTP listener to bind to the host's IP address
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add($"http://{GetHostIPAddress()}:8080/"); // Change the port as needed
        listener.Start();

        Console.WriteLine($"Hosting file at http://{GetHostIPAddress()}:8080/{Path.GetFileName(filePath)}");
        Console.WriteLine("Press any key to stop the server...");

        // Start a background thread to handle incoming requests
        ThreadPool.QueueUserWorkItem((state) =>
        {
            while (true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerResponse response = context.Response;

                    // Serve the file
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        response.ContentLength64 = fs.Length;
                        response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                        response.AddHeader("Content-Disposition", $"attachment; filename=\"{Path.GetFileName(filePath)}\"");

                        byte[] buffer = new byte[64 * 1024];
                        int bytesRead;
                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            response.OutputStream.Write(buffer, 0, bytesRead);
                        }
                    }

                    response.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        });

        // Wait for a key press to exit
        Console.ReadKey();

        // Stop the listener
        listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}