using System.Configuration;

namespace TestTask_1_Delivery
{
    internal static class LoggerWriter
    {
        static readonly string FileNameForSortOrders = ConfigurationManager.AppSettings["LogFileKey"]!;
        public static void WriteLogToFile(string logText)
        {

            using StreamWriter writer = new StreamWriter(FileNameForSortOrders, true);
            writer.WriteLine(logText);
        }
    }
}
