using System;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace device_upload
{
    class Program
    {

		static string DeviceConnectionString = "";
		static DeviceClient Client = null;

        static async Task Main(string[] args)
        {
            Console.WriteLine("*************************************************");
			Console.WriteLine("Welcome to the Azure IoT Hub Device Upload Tester");
			Console.WriteLine();
			Console.WriteLine("Author: Pete Gallagher");
			Console.WriteLine("Twitter: @pete_codes");
			Console.WriteLine("Date: 22nd December 2020");
			Console.WriteLine();
            Console.WriteLine("*************************************************");
			Console.WriteLine();

            try
            {
                Console.WriteLine("Enter the Device Connection String");
				DeviceConnectionString = Console.ReadLine();
				
				InitClient();

                await Client.SetReceiveMessageHandlerAsync(OnReceiveMessage, null);

                await UploadFile();
                
            }
            catch (System.Exception ex)
            {                
                Console.WriteLine();
				Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        public static void InitClient()
		{
			try
			{
				Console.WriteLine("Connecting to hub");
				Client = DeviceClient.CreateFromConnectionString(DeviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
			}
			catch (Exception ex)
			{
				Console.WriteLine();
				Console.WriteLine("Error in sample: {0}", ex.Message);
			}
		}

        public static async Task UploadFile() {

            Console.WriteLine("Uploading test.csv to the IoT Hub");

            var fileName = "test.csv";

            using var fileContentsStream = new FileStream(fileName, FileMode.Open);

            var fileUploadSasUriRequest = new FileUploadSasUriRequest();
            fileUploadSasUriRequest.BlobName = fileName;

            Console.WriteLine("Retrieving SAS URI for File Upload from the IoT Hub");
            FileUploadSasUriResponse sasUploadUri = await Client.GetFileUploadSasUriAsync(fileUploadSasUriRequest);

            Uri fileUploadUri = new Uri($"https://{sasUploadUri.HostName}/{sasUploadUri.ContainerName}/{Uri.EscapeDataString(sasUploadUri.BlobName)}{sasUploadUri.SasToken}");

            Console.WriteLine($"Successfully retrieved SAS URI ({fileUploadUri}) for File upload from IoT Hub");

            try
            {
                Console.WriteLine($"Uploading file {fileName} using the Azure Storage SDK and the retrieved SAS URI for authentication");

                var blob = new CloudBlockBlob(fileUploadUri);
                await blob.UploadFromStreamAsync(fileContentsStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File Upload Failed {ex.ToString()} - Sending Failure Notification to IoT Hub");

                var failedUploadNotification = new FileUploadCompletionNotification();

                failedUploadNotification.CorrelationId = sasUploadUri.CorrelationId;
                failedUploadNotification.IsSuccess = false;
                failedUploadNotification.StatusCode = 500;
                failedUploadNotification.StatusDescription = ex.Message;

                await Client.CompleteFileUploadAsync(failedUploadNotification);
            }

            Console.WriteLine("File Uploaded Successfully - Sending Success Notification to IoT Hub");

            var successfulUploadNotification = new FileUploadCompletionNotification();

            successfulUploadNotification.CorrelationId = sasUploadUri.CorrelationId;
            successfulUploadNotification.IsSuccess = false;
            successfulUploadNotification.StatusCode = 200;
            successfulUploadNotification.StatusDescription = "Success";

            await Client.CompleteFileUploadAsync(successfulUploadNotification);
            
        }

        private static async Task GetFileUploadUri() {

        }


        private static async Task OnReceiveMessage(Message message, object userContext)
        {
            var reader = new StreamReader(message.BodyStream);
            var messageContents = reader.ReadToEnd();
            Console.WriteLine($"Message Contents: {messageContents}");

            Console.WriteLine("Message Propeties:");

            foreach (var property in message.Properties)
            {
                Console.WriteLine($"Key: {property.Key}, Value: {property.Value}");
            }

            await Client.CompleteAsync(message);            
        }
    }
}
