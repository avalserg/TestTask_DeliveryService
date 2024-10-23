using System.Configuration;
using System.Globalization;
using TestTask_1_Delivery;

//путь к файлу для хранения всех заказов
string ordersFileName = ConfigurationManager.AppSettings["OrdersFileNameKey"]!;
string fileNameForSortOrders = ConfigurationManager.AppSettings["FileNameForSortOrdersKey"]!;

const char separatorFileNameForSortOrders = '\\';


try
{
    bool isExit = false;
    while (!isExit)
    {

        Console.WriteLine("Выберите необходимое действие: \n\"O\" - добавить новый заказ\n\"F\" - Отфильтровать заказы по региону и времени\n\"I\" - Показать все заказы \nЛюбую другую клавишу для выхода");
        var choiceAction = Console.ReadKey(true).Key;

        switch (choiceAction)
        {
            case ConsoleKey.O:
                Console.WriteLine();
                AddNewOrder();
                continue;
            case ConsoleKey.F:
                Console.WriteLine();
                FiltrationAreaOrdersByTime();
                continue;
            case ConsoleKey.I:
                Console.WriteLine();
                ShowOrdersInfoWithGrouping();
                continue;
            default:
                isExit = true;
                break;
        }
    }

    Console.WriteLine("Выход из программы");
    LoggerWriter.WriteLogToFile($"{LogLevels.Info}: Выход из программы {DateTime.Now}");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Сбой программы программа аварийно завершена!");
    Console.ResetColor();

    LoggerWriter.WriteLogToFile($"{LogLevels.Error}: Произошло исключение {ex.Message} Время: {DateTime.Now}  \n{ex.StackTrace}");
}

List<Order>? GetOrdersFromFile()
{
    return OrderReader.ReadOrders(ordersFileName);
}


void ShowOrdersInfoWithGrouping()
{
    var orders = OrderReader.ReadOrders(ordersFileName);

    Console.WriteLine("Список всех заказов:");


    if (orders.Any())
    {
        var groupOrdersCount = orders
            .GroupBy(o => o.OrderArea)
            .Select(g => new { AreaName = g.Key, TotalCountDelivery = g.Count(), Order = g.Select(o => o) })
            .OrderByDescending(c => c.TotalCountDelivery);

        foreach (var item in groupOrdersCount)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Имя региона доставки: {item.AreaName}\nКоличество заказов в регионе: {item.TotalCountDelivery}");
            Console.ResetColor();

            foreach (var time in item.Order.OrderBy(p => p.DeliveryTime))
            {
                Console.WriteLine($"№ заказа:{time.OrderId} Время:{time.DeliveryTime:yyyy-MM-dd HH:mm:ss} Вес:{time.Weight}кг");
            }

            Console.WriteLine();
        }
    }
    else
    {
        Console.WriteLine("Нет заказов");
    }
}

void AddNewOrder()
{
    // читаем заказы из файла
    var orders = GetOrdersFromFile();

    var isOrderRepeat = true;

    while (isOrderRepeat)
    {
        LoggerWriter.WriteLogToFile($"{LogLevels.Info}: Начало ввода заказа {DateTime.Now}");

        Console.Write("Введите район заказа (не может быть пустым значением): ");

        var area = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(area))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Название района не может быть пустым");
            LoggerWriter.WriteLogToFile($"{LogLevels.Warning}: Название района не может быть пустым {DateTime.Now}");
            Console.ResetColor();
            continue;
        }

        Console.Write("Введите время доставки (в формате yyyy-MM-dd HH:mm:ss): ");

        if (!DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var deliveryTime))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Введен некорректный формат даты");
            LoggerWriter.WriteLogToFile($"{LogLevels.Warning}: Введен некорректный формат даты {DateTime.Now}");

            Console.ResetColor();
            continue;
        }

        Console.Write("Введите вес (только целочисленное значение): ");

        if (!int.TryParse(Console.ReadLine(), out var weight))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Вес должен иметь целочисленное значение");
            LoggerWriter.WriteLogToFile($"{LogLevels.Warning}: Вес должен иметь целочисленное значение {DateTime.Now}");

            Console.ResetColor();
            continue;
        }

        // Добавляем заказ после проверки валидности всех данных
        OrderWriter.WriteOrdersOnAdd(new Order() { OrderId = orders.Count + 1, OrderArea = area, DeliveryTime = deliveryTime, Weight = weight }, ordersFileName);

        // перечитываем новые данные из файла после добавления
        // orders = GetOrdersFromFile();

        LoggerWriter.WriteLogToFile($"{LogLevels.Info}: Заказ был сохранен в файл {DateTime.Now}");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("****Заказ успешно добавлен****");
        LoggerWriter.WriteLogToFile($"{LogLevels.Info}: ****Заказ успешно добавлен**** {DateTime.Now}");
        Console.ResetColor();

        Console.WriteLine("Сделать еще заказ? Нажмите Y если да и любую другую клавишу для выхода");

        var choice = Console.ReadKey(true).Key;

        switch (choice)
        {
            case ConsoleKey.Y:
                break;
            default:
                isOrderRepeat = false;
                break;

        }
    }
}

void FiltrationAreaOrdersByTime()
{
    //выборка для доставки в конкретный регион города в ближайшие полчаса времени после первого заказа
    LoggerWriter.WriteLogToFile($"{LogLevels.Info}: Выбор региона для фильтрации заказов по времени {DateTime.Now}");

    string areaForTime;

    var isRegionFilterRepeat = true;

    var orders = GetOrdersFromFile();

    while (isRegionFilterRepeat)
    {
        Console.WriteLine("Выберите регион для фильтрации заказов по времени(1-ый заказ + 30мин)");

        areaForTime = Console.ReadLine() ?? string.Empty;

        if (!orders.Any(o => o.OrderArea == areaForTime))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Такого региона доставки не существует. Ввести еще раз Y если да и любую другую клавишу для выхода");
            Console.ResetColor();
            LoggerWriter.WriteLogToFile($"{LogLevels.Warning}: Отсутствие региона в списке существующих {DateTime.Now}");
        }
        else
        {
            orders = GetOrdersFromFile().Where(p => p.OrderArea == areaForTime).ToList();

            var minDate = orders.Min(i => i.DeliveryTime);

            var selectOrders = orders.Select(p => p).Where(area => area.DeliveryTime <= minDate.Add(new TimeSpan(0, 0, 30, 0))).ToList();

            OrderWriter.WriteOrdersAfterFiltrationByDeliveryTime(selectOrders, fileNameForSortOrders, areaForTime);
            Console.WriteLine("Список был сохранен в файл");
            LoggerWriter.WriteLogToFile($"{LogLevels.Info}: Список был сохранен в файл {DateTime.Now}");

            var ordersBy = OrderReader.ReadOrders(areaForTime + separatorFileNameForSortOrders + fileNameForSortOrders);

            foreach (var it in ordersBy)
            {
                Console.WriteLine($"{it.OrderId} {it.OrderArea} {it.DeliveryTime} {it.Weight}кг");
            }


        }

        Console.WriteLine("Нажмите Y чтобы повторить попытку или другую клавишу для выхода");

        var choice = Console.ReadKey(true).Key;
        switch (choice)
        {
            case ConsoleKey.Y:
                continue;
            default:
                isRegionFilterRepeat = false;
                break;

        }
    }
}
