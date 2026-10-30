using AsyncMonitor;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
var service = new CheckService();
var checks = new (string Name, TimeSpan Delay)[]
{
    ("Сеть", TimeSpan.FromMilliseconds(700)),
    ("База данных", TimeSpan.FromMilliseconds(900)),
    ("Файлы", TimeSpan.FromMilliseconds(500))
};

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\n[Получен сигнал отмены (Ctrl+C). Прерываем выполнение...]");
    cts.Cancel();
};

while (true)
{
    Console.WriteLine("\n1 — одна проверка; 2 — последовательно; 3 — параллельно; 0 — выход");
    var cmd = Console.ReadLine();
    if (cmd == "0")
        break;
    try
    {
        if (cmd == "1")
        {
            // TODO 8: Вызвать метод с await, не применять .Wait() или .Result.
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await service.CheckServiceAsync(checks[0].Name, checks[0].Delay, cts.Token);
            sw.Stop();
            Console.WriteLine($"Одиночная проверка '{checks[0].Name}': {sw.ElapsedMilliseconds} мс");
            Print(new[] { result });
        }
        else if (cmd == "2")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunSequentialAsync(checks);
            sw.Stop();
            Console.WriteLine($"Последовательно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "3")
        {
            Console.WriteLine("Запуск длительной проверки (5000 мс). Нажмите Ctrl+C для отмены.");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await service.CheckServiceAsync("Длительная задача", TimeSpan.FromMilliseconds(5000), cts.Token);

            sw.Stop();
            Console.WriteLine($"Длительная проверка завершена: {sw.ElapsedMilliseconds} мс");
            Print(new[] { result });
        }
        else
            Console.WriteLine("Неизвестная команда.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Проверка отменена.");
    }
    catch (Exception ex)
    { // TODO 9: Не скрывать исключение; сообщить тип и причину.
        Console.WriteLine($"[ОШИБКА] Тип исключения: {ex.GetType().Name} | Причина: {ex.Message}");
    }
}
// TODO 10: Добавить режим контроля/отмены длительной проверки и показать,
// что меню не зависает на ожидании Task.Delay.
static void Print(IEnumerable<CheckResult> xs)
{
    Console.WriteLine("--- Результаты проверок ---");
    foreach (var x in xs)
    {
        var status = x.Success ? "Успешно" : "Ошибка";
        Console.WriteLine($" • {x.Name}: {status} за {x.Duration.TotalMilliseconds:F0} мс. {x.Message}");
    }
    Console.WriteLine("---------------------------");
}
