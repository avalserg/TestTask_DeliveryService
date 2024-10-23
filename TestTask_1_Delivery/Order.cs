namespace TestTask_1_Delivery
{
    internal class Order
    {
        public int OrderId { get; set; }
        public int Weight { get; set; }
        public string OrderArea { get; set; } = string.Empty;
        public DateTime DeliveryTime { get; set; }
    }
}
