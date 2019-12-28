using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace Cutera
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var application = new Application();
            Console.Title = "Cutera";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter input file name for zip codes to be checked(including extension)");
            string inputFileName = Console.ReadLine();
            Console.WriteLine("Enter output file name");
            string outputFileName = Console.ReadLine();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("The application is running. Please do not close this window.");
            Console.ForegroundColor = ConsoleColor.White;
            try
            {
                application.Run(inputFileName, outputFileName).GetAwaiter().GetResult();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (GeocodeException ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Geocode API failed!");
                Console.WriteLine("Geocode=" + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("InputFileNotFound!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Please make sure none of the mentioned files is open or being used by another process!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Unexpected Error Occured! Check debug file for more details.");
                File.WriteAllText("debug.txt", "Type=" + ex.GetType().ToString() + Environment.NewLine + "Message=" + ex.Message + Environment.NewLine + "StackTrace=" + ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
            }
            string successFile = "success.txt";
            File.WriteAllText(successFile, JsonConvert.SerializeObject(application.processedZipcodes));
            Console.WriteLine("All successfully processed zipcodes saved to " + successFile);
            Console.WriteLine("Press any key to exit");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
        }
    }
}
