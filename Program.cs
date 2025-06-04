// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder();
IConfiguration configuration = builder.SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json ")
    .Build();

// initialize variable from config
int port = int.Parse(configuration["ClientConfig:Port"]);
int bufferSize = int.Parse(configuration["ClientConfig:BufferSize"]);
string serverIP = configuration["ClientConfig:ServerIP"];
string AES_Key = configuration["ClientConfig:AES_Key"];
string AES_IV = configuration["ClientConfig:AES_IV"];

EncryptDecrypt en = new EncryptDecrypt(AES_Key, AES_IV);


try
{
    // create tcp client
    TcpClient tcpClient = new(serverIP, port);


    Console.WriteLine($"TCP Client initialized with endpoint {serverIP}:{port}");

    // prompt to enter the code
    Console.Write("Please enter the code - ");
    string code = Console.ReadLine();


    // Get a client stream for reading and writing.
    NetworkStream stream = tcpClient.GetStream();
    byte[] data = en.Encrypt(code);

    stream.Write(data, 0, data.Length);

    data = new Byte[bufferSize];

    // String to store the response ASCII representation.
    string responseData = String.Empty;

    // Read the first batch of the TcpServer response bytes.
    Int32 bytes;
    while ((bytes = await stream.ReadAsync(data, 0, data.Length)) > 0)
    {
        // remove padding
        byte[] actualData = data[..bytes];

        responseData = en.Decrypt(actualData);
        Console.WriteLine($"{responseData}");
    }

    tcpClient.Close();
}
catch (System.IO.IOException IOException)
{
    Console.Write($"Unable to read data {IOException}");
}
catch (Exception e)
{
    Console.WriteLine(e);
}

