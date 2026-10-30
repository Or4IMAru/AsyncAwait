using AsyncMonitor;
var passed = 0;
var failed = 0;
void Check(bool ok, string name)
{
    if (ok)
    {
        passed++;
        Console.WriteLine("PASS " + name);
    }
    else
    {
        failed++;
        Console.WriteLine("FAIL " + name);
    }
}
var s = new CheckService();
var one = await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(25));
Check(
    one.Name == "Тест" && one.Success && one.Duration >= TimeSpan.Zero,
    "одиночная проверка возвращает результат"
);
var seq = await s.RunSequentialAsync(
    new[]
    {
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    }
);
Check(
    seq.Count == 3 && seq.Select(x => x.Name).SequenceEqual(new[] { "A", "B", "C" }),
    "последовательный запуск"
);
var all = await s.RunParallelAsync(
    new[]
    {
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    }
);

// Тест на пустое имя (должен выбросить ArgumentException)
bool emptyNameThrows = false;
try
{
    await s.CheckServiceAsync("", TimeSpan.FromMilliseconds(10));
}
catch (ArgumentException)
{
    emptyNameThrows = true;
}
Check(emptyNameThrows, "пустое имя выбрасывает ArgumentException");

// Тест на null имя (должен выбросить ArgumentNullException)
bool nullNameThrows = false;
try
{
    await s.CheckServiceAsync(null, TimeSpan.FromMilliseconds(10));
}
catch (ArgumentNullException)
{
    nullNameThrows = true;
}
Check(nullNameThrows, "null имя выбрасывает ArgumentNullException");
// Тест на отрицательную задержку (должен выбросить ArgumentOutOfRangeException)
bool negativeDelayThrows = false;
try
{
    await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(-10));
}
catch (ArgumentOutOfRangeException)
{
    negativeDelayThrows = true;
}
Check(negativeDelayThrows, "отрицательная задержка выбрасывает ArgumentOutOfRangeException");

// Тест на CancellationToken (должен выбросить OperationCanceledException)
bool cancelThrows = false;
using var cts = new CancellationTokenSource();
cts.Cancel(); // Немедленно запрашиваем отмену
try
{
    await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(500), cts.Token);
}
catch (OperationCanceledException)
{
    cancelThrows = true;
}
Check(cancelThrows, "отмененный CancellationToken выбрасывает OperationCanceledException");

Check(all.Count == 3 && all.All(x => x.Success), "параллельный запуск WhenAll");
// TODO: Добавить тесты на пустое имя, отрицательную задержку и CancellationToken.
Console.WriteLine($"Итого: {passed} пройдено, {failed} ошибок.");
return failed == 0 ? 0 : 1;
