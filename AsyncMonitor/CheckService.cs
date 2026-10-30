using System.Diagnostics;
namespace AsyncMonitor;
public sealed class CheckService
{
    public async Task<CheckResult> CheckServiceAsync(string name, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        // TODO 1: Отклонить пустое имя проверки.
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Имя проверки не может быть пустым.", nameof(name));
        }
        // TODO 2: Отклонить отрицательную длительность через ArgumentOutOfRangeException.
        if (delay == TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay));
        }
        var sw = Stopwatch.StartNew();
        // TODO 3: Использовать await Task.Delay(delay, cancellationToken), а не Thread.Sleep.
        await Task.Delay(delay, cancellationToken);
        sw.Stop();
        // TODO 4: Вернуть корректный CheckResult.
        return new CheckResult(name, true, sw.Elapsed, string.Empty);
    }
    public async Task<IReadOnlyList<CheckResult>> RunSequentialAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        var results = new List<CheckResult>();
        // TODO 5: Выполнить проверки по одной, ожидая каждую через await.
        foreach (var check in checks)
        {
            var result = await CheckServiceAsync(check.Name, check.Delay, token);

            results.Add(result);
        }

        // List<T> неявно приводится к IReadOnlyList<T>, поэтому возвращаем его напрямую
        return results;
    }
    public async Task<IReadOnlyList<CheckResult>> RunParallelAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        // TODO 6: Сначала создать задачи для всех проверок.
        IEnumerable<Task<CheckResult>> tasks = checks.Select(check =>
            CheckServiceAsync(check.Name, check.Delay, token));

        // TODO 7: Совместно дождаться завершения всех задач через Task.WhenAll.
        CheckResult[] results = await Task.WhenAll(tasks);
        return results;
        
    }
}
