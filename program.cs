/* THIS PROGRAM REQUIRES EXTERNAL PACKAGES 
PLEASE OPEN Visual studios then Tools > NuGet Package Manager > Microsoft.Azure.CognitiveServices.Vision.ComputerVision 
*/

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

using System;
using System.IO;
using System.Threading.Tasks;

namespace ExtractText
{
    class Program
    {
        public static int temp = 0;

        // EXAMPLE==> subscriptionKey = "3f067cb729dac486ea531338ffff4b42"
        private const string subscriptionKey = "YOUR_KEY_HERE"; // U CAN GET BY VISITING https://azure.microsoft.com/en-in/services/cognitive-services/computer-vision/  and click on "TRY COMPUTER VISION API"

        private const TextRecognitionMode textRecognitionMode =
            TextRecognitionMode.Handwritten;

        public static string pathOne = @"PATH_OF_IMAGE"; // EXAMPLE==>  D:\HandwrittenImage\image.jpg

        public static int fileCount = Directory.GetFiles(pathOne, "*.*", SearchOption.AllDirectories).Length;

        public static int waitTime = fileCount * 15000;

        private const string remoteImageUrl = "";

        private const int numberOfCharsInOperationId = 36;

        static void Main(string[] args)
        {
            string[] array1 = Directory.GetFiles(@"PATH_OF_IMAGE");
            Console.WriteLine(fileCount);

            foreach (string name in array1)
            {
                Console.WriteLine(name);
                string localImagePath = name;


                ComputerVisionClient computerVision = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });
            
            computerVision.Endpoint = "https://westcentralus.api.cognitive.microsoft.com"; // This is default for Trail Versions kindly change if required
            
            Console.WriteLine("Images are being analyzed please wait...");
            
            var t2 = ExtractLocalTextAsync(computerVision, localImagePath);

                Task.WhenAll(t2).Wait(50000);
                //Console.ReadLine();

            }
        }

        
        // Recognize text from a local image
        private static async Task ExtractLocalTextAsync(
            ComputerVisionClient computerVision, string imagePath)
        {

            if (!File.Exists(imagePath))
            {
                Console.WriteLine(
                    "\nUnable to open or read localImagePath:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                // Start the async process to recognize the text
                RecognizeTextInStreamHeaders textHeaders =
                    await computerVision.RecognizeTextInStreamAsync(
                        imageStream, textRecognitionMode);

                await GetTextAsync(computerVision, textHeaders.OperationLocation);
            }
            Console.WriteLine(imagePath);
        }

        // Retrieve the recognized text
        private static async Task GetTextAsync(
            ComputerVisionClient computerVision, string operationLocation)
        {
            // Retrieve the URI where the recognized text will be
            // stored from the Operation-Location header
            string operationId = operationLocation.Substring(
                operationLocation.Length - numberOfCharsInOperationId);

            Console.WriteLine("\nCalling GetHandwritingRecognitionOperationResultAsync()");
            TextOperationResult result =
                await computerVision.GetTextOperationResultAsync(operationId);

            // Wait for the operation to complete
            int i = 0;
            int maxRetries = 10;
            while ((result.Status == TextOperationStatusCodes.Running ||
                    result.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries)
            {
                Console.WriteLine(
                    "Server status: {0}, waiting {1} seconds...", result.Status, i);
                await Task.Delay(1000);

                result = await computerVision.GetTextOperationResultAsync(operationId);
            }

            // Write to text file instead of console
            // EXAMPLE==> string path = @"D:\ouput\outfile"+temp+".txt"
            string path = @"OUTPUT_PATH"+temp+".txt";  

            temp = temp + 1;

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();

            }

            if (File.Exists(path))
            {
                foreach (Line line in result.RecognitionResult.Lines) using (var tw = new StreamWriter(path, true))

                    {
                        tw.WriteLine(line.Text);
                    }

            }
            Console.WriteLine("Successfully extracted");
            await Task.Delay(1000);
            //Environment.Exit(0);
        }
    }
}