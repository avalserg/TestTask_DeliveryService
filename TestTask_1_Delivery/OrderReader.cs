using Newtonsoft.Json;
namespace TestTask_1_Delivery;
internal static class OrderReader
{
    public static List<Order>? ReadOrders(string filePath)
    {
        var jsonData = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<Order>>(jsonData) ?? default;
    }
}