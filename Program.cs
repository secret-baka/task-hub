using System.Diagnostics;
using System.IO;
using System.Threading.Tasks.Dataflow;

internal sealed class Program
{
    private static async Task Main()
    {
        var links = new[]
        {
            "https://github.com/teniuse/timesheet-costing",
            "https://github.com/FrMks/Tracking-labor-hours-and-the-cost-of-project-work",
            "https://github.com/ilya-vor/TestCase_TimeTrackingCosting",
            "https://github.com/Gryphon999/timesheet",
            "https://github.com/sprait38/timesheet-app",
            "https://github.com/CrispX13/timesheet-app",
            "https://github.com/erikbadalyan4/timesheet",
            "https://github.com/rinat725227/timesheet",
            "https://github.com/i7weet/TestApp",
            "https://github.com/P1zhama/test-test",
            "https://github.com/FoxsM/Test",
            "https://github.com/GolCrash/TestTaskCode",
            "https://github.com/JustKaneri/Test-Fullstack",
            "https://github.com/ROYMartell/TestTaskReact",
            "https://github.com/worthAlgorithmPlayer/PMRost_Test",
            "https://github.com/DymonH/timesheet-test",
            "https://github.com/fet1sov-test-tasks/PM-ROST",
            "https://github.com/Pipotka/fullstack-timesheet-test",
            "https://github.com/PredictorLQ/TestTasks",
            "https://github.com/waleron07/timesheet-test-task",
            "https://github.com/amrrwael/timesheet-test-task",
            "https://github.com/intestal/Fullstack-test-task",
            "https://github.com/xenes-gh/fullstack-test-task",
            "https://github.com/alexander2555/test-task-mprost",
            "https://github.com/h4tikk/TestTask",
            "https://github.com/Juimun/timesheet-test-task",
            "https://github.com/qwirlyx/timesheet-test-task",
            "https://github.com/xomyachok-shaolin/timesheet-test-task",
            "https://github.com/Dimanqe/timesheet-test-task",
            "https://github.com/Regat1ve/timesheet-costs",
            "https://github.com/ElipSide/timesheet-project-costs",
            "https://github.com/Choppik/test-task-accounting-for-labor-costs",
            "https://github.com/Ourobor0s3/test-task-review",
            "https://github.com/Summist/timesheet-cost-tracker"
        };

        var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "repos");
        Directory.CreateDirectory(outputFolder);

        Console.WriteLine($"Клонируем {links.Length} репозиториев в {outputFolder}...\n");

        // Ограничиваем параллелизм (чтобы не положить сеть/диск)
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5
        };

        await Parallel.ForEachAsync(links, options, async (link, ct) =>
        {
            var repoName = GetRepoName(link);
            var repoPath = Path.Combine(outputFolder, repoName);

            if (Directory.Exists(repoPath))
            {
                Console.WriteLine($"[SKIP] {repoName} — уже существует");
                return;
            }

            Console.WriteLine($"[CLONE] {repoName}...");

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone --depth 1 {link} \"{repoPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    Console.WriteLine($"[ERROR] {repoName} — не удалось запустить git");
                    return;
                }

                await process.WaitForExitAsync(ct);

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"[OK] {repoName}");
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync(ct);
                    Console.WriteLine($"[ERROR] {repoName} — {error.Trim()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {repoName} — {ex.Message}");
            }
        });

        Console.WriteLine("\nГотово!");
        Console.WriteLine($"Репозитории в: {outputFolder}");
    }

    private static string GetRepoName(string url)
    {
        // https://github.com/user/repo -> user-repo
        var parts = url.TrimEnd('/').Split('/');
        var user = parts[^2];
        var repo = parts[^1];
        return $"{user}-{repo}";
    }
}