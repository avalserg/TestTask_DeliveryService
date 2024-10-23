using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace TestTask_1_Delivery;

internal static class OrderWriter
{
    /// <summary>
    /// Записать в файл новый заказ
    /// </summary>
    public static void WriteOrdersOnAdd(Order order, string filePath)
    {
        var data = File.ReadAllText(filePath);
        var listOrders = JsonConvert.DeserializeObject<List<Order>>(data) ?? default;

        listOrders?.Add(order);
        using FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
        System.Text.Json.JsonSerializer.Serialize(fs, listOrders, new JsonSerializerOptions() { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic) });

    }

    /// <summary>
    /// Записать отсортированные по времени заказы в файл
    /// </summary>
    public static void WriteOrdersAfterFiltrationByDeliveryTime(List<Order> orders, string fileName, string selectedArea)
    {
        var directory = Directory.CreateDirectory(selectedArea);
        using FileStream fs = new FileStream(directory.FullName + "\\" + fileName, FileMode.OpenOrCreate);
        System.Text.Json.JsonSerializer.Serialize(fs, orders, new JsonSerializerOptions() { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic) });

    }
}