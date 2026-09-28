using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Uinsure.Api.Persistence;
using Uinsure.Domain.Policies;

namespace Uinsure.FullStack;

internal static class Runner
{
    private const string Image = "mcr.microsoft.com/mssql/server:2022-CU27-ubuntu-22.04@sha256:4402d880dd4c34bfa7d8705e56a86cd6c88da80a1f6bbbe741f999e76264a090";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<int> Main(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(root, "Uinsure.slnx")))
            throw new InvalidOperationException("Run from the assessment repository root.");
        AssertPortAvailable(5081);
        AssertPortAvailable(5174);
        using var stop = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var runDirectory = Path.Combine(root, "TestResults", "fullstack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(runDirectory);
        var receiptsDirectory = Path.Combine(runDirectory, "receipts");
        Directory.CreateDirectory(receiptsDirectory);
        var password = $"T!{Convert.ToHexString(RandomNumberGenerator.GetBytes(24))}a1";
        await using var sql = new MsSqlBuilder(Image).WithPassword(password).Build();
        var processes = new List<Process>();
        try
        {
            Console.WriteLine("Starting isolated SQL Server and applying migrations.");
            await sql.StartAsync(stop.Token);
            var options = new DbContextOptionsBuilder<UinsureDbContext>()
                .UseSqlServer(sql.GetConnectionString()).Options;
            await using (var db = new UinsureDbContext(options))
            {
                await db.Database.MigrateAsync(stop.Token);
                var fixtures = new Dictionary<string, Fixture>();
                foreach (var browser in new[] { "desktop-chromium", "mobile-chromium" })
                {
                    foreach (var kind in new[] { "paid", "manual", "day15", "claims", "leap" })
                    {
                        var start = kind switch
                        {
                            "paid" or "manual" => today.AddYears(-1).AddDays(15),
                            "leap" => new DateOnly(2028, 2, 29),
                            _ => today.AddDays(-14)
                        };
                        var reference = $"POL-{Guid.NewGuid():N}"[..32].ToUpperInvariant();
                        // Historical fixtures make renewal reachable without weakening the production clock.
                        var policy = Policy.Sell(reference, new SellPolicyData(
                            InsuranceType.Household, start, 365m, kind == "claims", kind != "manual",
                            [new PolicyholderData("Ada", "Example", new DateOnly(1990, 1, 1))],
                            new PropertyData("1 Synthetic Road", "Second line", "Third line", null, "M1 1AA"),
                            PaymentMethod.Card), start, new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
                        db.Policies.Add(policy);
                        var term = policy.Terms.Single();
                        fixtures[$"{browser}-{kind}"] = new Fixture(reference, term.Id, term.StartDate, term.EndDate);
                    }
                }
                await db.SaveChangesAsync(stop.Token);
                await File.WriteAllTextAsync(Path.Combine(runDirectory, "manifest.json"),
                    JsonSerializer.Serialize(new { today, fixtures }, Json), stop.Token);
            }

            var apiDll = Path.Combine(root, "src", "Uinsure.Api", "bin", "Release", "net10.0", "Uinsure.Api.dll");
            var api = Start("dotnet", [apiDll, "--urls", "http://127.0.0.1:5081"], root,
                new() { ["ConnectionStrings__Uinsure"] = sql.GetConnectionString(), ["ASPNETCORE_ENVIRONMENT"] = "Production" }, "api");
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            await Ready(http, "http://127.0.0.1:5081/health", api, stop.Token);
            var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(runDirectory, "manifest.json"), stop.Token));
            var referenceToCheck = manifest.RootElement.GetProperty("fixtures").EnumerateObject().First()
                .Value.GetProperty("reference").GetString();
            using var persisted = await http.GetAsync($"http://127.0.0.1:5081/api/policies/{referenceToCheck}", stop.Token);
            persisted.EnsureSuccessStatusCode();
            var webRoot = Path.Combine(root, "web");
            var vite = Start("node", ["node_modules/vite/bin/vite.js", "--host", "127.0.0.1", "--port", "5174"], webRoot,
                new() { ["UINSURE_API_PROXY"] = "http://127.0.0.1:5081" }, "vite");
            await Ready(http, "http://127.0.0.1:5174", vite, stop.Token);
            Console.WriteLine("Isolated application ready at http://127.0.0.1:5174");
            if (args.Contains("--serve", StringComparer.Ordinal))
            {
                Console.WriteLine(await File.ReadAllTextAsync(Path.Combine(runDirectory, "manifest.json"), stop.Token));
                Console.WriteLine("Synthetic demo only. Ctrl+C stops the processes and disposes this database.");
                await Task.Delay(Timeout.Infinite, stop.Token);
            }
            else
            {
                var browser = Start("node", ["node_modules/@playwright/test/cli.js", "test", "--config", "playwright.fullstack.config.ts"], webRoot,
                    new()
                    {
                        ["UINSURE_E2E_MANIFEST"] = Path.Combine(runDirectory, "manifest.json"),
                        ["UINSURE_E2E_RECEIPTS"] = receiptsDirectory
                    }, "playwright");
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop.Token);
                deadline.CancelAfter(TimeSpan.FromMinutes(5));
                await browser.WaitForExitAsync(deadline.Token);
                if (browser.ExitCode != 0) throw new InvalidOperationException("Full-stack browser checks failed; see artifacts.");
                if (DateOnly.FromDateTime(DateTime.UtcNow) != today)
                    throw new InvalidOperationException("UTC date changed during the run. Rerun with fresh fixtures.");
                await VerifyReceipts(options, receiptsDirectory, stop.Token);
                Console.WriteLine("Full-stack browser journeys and fresh SQL financial/history assertions passed.");
            }
            return 0;
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested)
        {
            return args.Contains("--serve", StringComparer.Ordinal) ? 0 : 1;
        }
        finally
        {
            foreach (var process in processes.AsEnumerable().Reverse())
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                process.Dispose();
            }
            Console.WriteLine("Owned application processes stopped; disposing isolated SQL Server.");
        }

        Process Start(string executable, string[] arguments, string workingDirectory, Dictionary<string, string> environment, string name)
        {
            var info = new ProcessStartInfo(executable)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var argument in arguments) info.ArgumentList.Add(argument);
            foreach (var (key, value) in environment) info.Environment[key] = value;
            var process = new Process { StartInfo = info };
            var logLock = new object();
            void Log(object sender, DataReceivedEventArgs e)
            {
                if (e.Data is null) return;
                var line = e.Data.Replace(password, "[redacted]", StringComparison.Ordinal);
                lock (logLock) File.AppendAllText(Path.Combine(runDirectory, name + ".log"), line + Environment.NewLine);
                if (name == "playwright") Console.WriteLine(line);
            }
            process.OutputDataReceived += Log;
            process.ErrorDataReceived += Log;
            process.Start();
            processes.Add(process);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }
    }

    private static void AssertPortAvailable(int port)
    {
        var listener = new TcpListener(IPAddress.Loopback, port);
        try { listener.Start(); }
        catch (SocketException e) { throw new InvalidOperationException($"Port {port} is busy; no existing server will be reused.", e); }
        finally { listener.Stop(); }
    }

    private static async Task Ready(HttpClient http, string url, Process process, CancellationToken token)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (process.HasExited) throw new InvalidOperationException($"Server exited before readiness at {url}.");
            try
            {
                using var response = await http.GetAsync(url, token);
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) when (!token.IsCancellationRequested) { }
            await Task.Delay(200, token);
        }
        throw new TimeoutException($"Server did not become ready at {url}.");
    }

    private static async Task VerifyReceipts(DbContextOptions<UinsureDbContext> options, string directory, CancellationToken token)
    {
        var files = Directory.GetFiles(directory, "*.json");
        Require(files.Length == 8, "Expected four completed journeys for each browser viewport.");
        foreach (var file in files)
        {
            var receipt = JsonSerializer.Deserialize<Receipt>(await File.ReadAllTextAsync(file, token), Json)!;
            await using var db = new UinsureDbContext(options);
            var policy = await db.Policies.Include(p => p.Terms).ThenInclude(t => t.Property)
                .Include(p => p.Terms).ThenInclude(t => t.Payment)
                .Include(p => p.Terms).ThenInclude(t => t.Cancellation).ThenInclude(c => c!.Refund)
                .SingleAsync(p => p.Reference == receipt.Reference, token);
            var initial = policy.Terms.Single(t => t.PredecessorTermId is null);
            Require(initial.Payment?.Amount == 365m, "Original payment must persist.");
            Require(initial.Property.AddressLine3 == "Third line", "Address Line 3 must persist.");
            if (receipt.Kind == "sale") Require(policy.Terms.Count == 1, "Sale must create one term.");
            else if (receipt.Kind == "cancel")
            {
                Require(initial.Cancellation?.Refund is { Amount: > 0, Method: PaymentMethod.Card }, "Cancellation must persist a same-method refund.");
                Require(initial.Cancellation!.RefundAmount == receipt.Refund, "Browser and stored refund must agree.");
                Require(initial.Cancellation.Refund!.PaymentId == initial.Payment!.Id, "Refund must reference the original payment.");
                Require(policy.MutationRevision == 1, "Cancellation must increment the policy revision once.");
            }
            else
            {
                Require(policy.Terms.Count == 2, "Renewal must create exactly one successor.");
                var successor = policy.Terms.Single(t => t.PredecessorTermId == initial.Id);
                Require(successor.StartDate == initial.EndDate.AddDays(1), "Renewal must preserve continuous history.");
                Require(successor.Property.AddressLine3 == "Third line", "Renewal must copy Address Line 3.");
                Require(!successor.HasClaims, "New term starts without claims.");
                Require(receipt.Kind == "paid" ? successor.Payment is { Method: PaymentMethod.DirectDebit, Amount: 365m }
                    : successor.Payment is null, "Renewal payment must match the auto-renew choice.");
                Require(policy.MutationRevision == 1, "Renewal must increment the policy revision once.");
            }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed record Fixture(string Reference, Guid TermId, DateOnly StartDate, DateOnly EndDate);
    private sealed record Receipt(string Reference, string Kind, decimal? Refund);
}
