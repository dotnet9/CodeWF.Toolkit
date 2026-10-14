using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using CodeWF.Toolkit.Core.ApplicationData;
using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.FileExtensions;
using CodeWF.Toolkit.Core.Helpers;
using CodeWF.Toolkit.Core;
using CodeWF.Toolkit.Core.UpdateChecking;
using CodeWF.Toolkit.Demo.Avalonia.Views.Pages;
using CodeWF.Toolkit.Files.Exports;
using CodeWF.Toolkit.Files.Extensions;
using CodeWF.Toolkit.Files.FileExtensions;
using CodeWF.Toolkit.Files.Helpers;
using CodeWF.Toolkit.Logging;
using CodeWF.Toolkit.NetWrapper.FileSystem;
using CodeWF.Toolkit.NetWrapper.Helpers;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

/// <summary>Explicit API examples, with no runtime type discovery or simulated .NET results.</summary>
public static partial class HelperDemoCatalog
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ValidationSample))]
    public static HelperExample Create(string key) => key switch
    {
        "StringExtensions" => Sync(key, "one,,two,three", "input.Split(',').Join(\" / \", removeEmptyEntry: true);",
            text => text.Split(',').Join(" / ", removeEmptyEntry: true)),
        "RegexExtensions" => Sync(key, "item42\nitem##", "text.IsMatch(pattern);\npattern.ConvertPattern();",
            text => { var lines = text.Split('\n'); var pattern = lines.ElementAtOrDefault(1) ?? "item*"; return $"IsMatch = {lines[0].IsMatch(pattern)}\nRegex = {pattern.ConvertPattern()}"; }),
        "DoubleExtensions" => Sync(key, "1048576", "double.Parse(input).FormatBytes();", text => ParseNumber(text).FormatBytes()),
        "NumberExtension" => Sync(key, "123.45", "number.ToChineseNumber();\nNumberExtension.ToChineseMoney(number);",
            text => $"{ParseNumber(text).ToChineseNumber()}\n{NumberExtension.ToChineseMoney(ParseNumber(text))}"),
        "DateTimeExtension" => Sync(key, "2026-10-13T12:00:00Z", "date.GetUnixTimeSeconds();\ndate.GetUnixTimeMilliseconds();",
            text => { var date = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture); return $"seconds = {date.GetUnixTimeSeconds()}\nmilliseconds = {date.GetUnixTimeMilliseconds()}"; }),
        "TimeSpanExtensions" => Sync(key, "3661234", "milliseconds.FormatMilliseconds();", text => ParseNumber(text).FormatMilliseconds()),
        "RandomExtension" => Sync(key, "1,100", "RandomExtension.GetInt(min, max);",
            text => { var numbers = text.Split(',').Select(int.Parse).ToArray(); return string.Join(", ", Enumerable.Range(0, 5).Select(_ => RandomExtension.GetInt(numbers[0], numbers[1]))); }),
        "IConvertibleExtensions" => Sync(key, "42", "input.ConvertTo<int>();\ntype.IsNumeric();",
            text => $"Int32 = {text.ConvertTo<int>()}\nIsNumeric = {typeof(int).IsNumeric()}"),
        "ObjectExtension" => Sync(key, "", "input.IsNullOrEmpty();\ntype.IsPrimitive();",
            text => $"IsNullOrEmpty = {text.IsNullOrEmpty()}\nString.IsPrimitive = {ObjectExtension.IsPrimitive(typeof(string))}"),
        "LinqExtension" => Sync(key, "3", "positive.And(value => value < limit).Compile(preferInterpretation: true);",
            text => { var limit = int.Parse(text, CultureInfo.InvariantCulture); Expression<Func<int, bool>> positive = number => number > 0; Expression<Func<int, bool>> upper = number => number < limit; var predicate = positive.And(upper).Compile(preferInterpretation: true); return string.Join(", ", Enumerable.Range(-2, 10).Where(predicate)); }),
        "EnumExtension" => Sync(key, "", "typeof(DayOfWeek).GetDictionary();", _ => string.Join('\n', typeof(DayOfWeek).GetDictionary().Select(pair => $"{pair.Key}: {pair.Value}"))),
        "AssemblyExtensions" => Sync(key, "", "assembly.Title();\nassembly.Version();\nassembly.Company();",
            _ => { var assembly = typeof(HelperDemoCatalog).Assembly; return $"{assembly.Title()}\n{assembly.Version()}\n{assembly.Company()}"; }),
        "ValidationExtension" => Sync(key, "", "new ValidationSample { Name = input }.TryValidateObject(out var errors);",
            text => { var valid = new ValidationSample { Name = text }.TryValidateObject(out var errors); if (valid != !string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("Required validation metadata was not applied."); return $"Valid = {valid}\n" + string.Join('\n', errors?.Select(error => error.ErrorMessage) ?? []); }),
        "DateTimeConverter" => Sync(key, "2026-10-13T12:00:00Z", "DateTimeConverter.IsISO8601DateTimeString(input);\nDateTimeConverter.DateToExcelFormat(date);",
            text => $"ISO8601 = {DateTimeConverter.IsISO8601DateTimeString(text)}\nExcel = {DateTimeConverter.DateToExcelFormat(DateTime.Parse(text, CultureInfo.InvariantCulture))}"),
        "ActionHelper" => Async(key, "20,100", "ActionHelper.CheckOvertime(() => Thread.Sleep(delayMilliseconds), timeoutMilliseconds);\n// true = completed in time; false = wait timed out. The action continues running.",
            async text =>
            {
                var values = text.Split(',').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                if (values.Length != 2 || values.Any(value => value < 0 || value > 5000))
                    throw new FormatException(global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Catalog.ActionFormat"));
                var result = await Task.Run(() => ActionHelper.CheckOvertime(() => Thread.Sleep(values[0]), values[1]));
                return $"CheckOvertime = {result}\n" + global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(result ? "Demo.Catalog.ActionCompleted" : "Demo.Catalog.ActionTimedOut");
            }),
        "IpHelper" => Sync(key, "127.0.0.1:39561", "IpHelper.TrySplitIpPort(input, out var ip, out var port);",
            text => { var success = IpHelper.TrySplitIpPort(text, out var ip, out var port); return $"Valid = {success}\nIP = {ip}\nPort = {port}\nLocal IPv4 = {string.Join(", ", IpHelper.GetAllIpV4(true))}"; }),
        "NativeLibraryHelper" => Sync(key, "GetTickCount64", "NativeLibraryHelper.GetMethod(Environment.SystemDirectory, \"kernel32\", \"\", \"\", method, out var address, out var error);",
            NativeMethod, smoke: OperatingSystem.IsWindows()),
        "FileHelper" => Async(key, "Hello, CodeWF!", "await FileHelper.SafeWriteAllTextAsync(path, input);\nawait FileHelper.SafeReadAllTextAsync(path);", async text =>
            { var path = Path.Combine(TemporaryDirectory(), "sample.txt"); await FileHelper.SafeWriteAllTextAsync(path, text); return $"{path}\n{await FileHelper.SafeReadAllTextAsync(path)}"; }),
        "AppDataMigrator" => Sync(key, "demo.json", "AppDataMigrator.ResolveAndMigrate(appName, fileName, baseDirectory, portableMarkerFileName);",
            text => { var directory = TemporaryDirectory(); File.WriteAllText(Path.Combine(directory, "portable.marker"), ""); return AppDataMigrator.ResolveAndMigrate("CodeWFDemo.Catalog." + Guid.NewGuid().ToString("N"), Path.GetFileName(text), directory, "portable.marker"); }),
        "UpdateChecker" => Async(key, "dotnet9/CodeWF.Toolkit", "await new UpdateChecker(httpClient, owner, repository).CheckAsync(currentVersion);", async text =>
            { var parts = text.Trim('/').Split('/'); if (parts.Length != 2) throw new FormatException("Expected owner/repository."); using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) }; var result = await new UpdateChecker(http, parts[0], parts[1]).CheckAsync(new Version(1,0)); return $"Succeeded = {result.Succeeded}\nTag = {result.Update?.Tag}\nError = {result.Error}"; }, smoke: false),
        "FileExtension" => Sync(key, "..", "FileExtension.IsPathInUpperTwoLevelsOfCurrentDir(input);", text => $"IsPathInUpperTwoLevelsOfCurrentDir = {FileExtension.IsPathInUpperTwoLevelsOfCurrentDir(text)}"),
        "SevenZipCompressor" => Sync(key, "Hello archive", "compressor.Zip(sourceDirectory, zipPath);\ncompressor.Decompress(zipPath, destinationDirectory);", Archive),
        "AppConfigHelper" => Sync(key, "42", "AppConfigHelper.SetOrAdd(path, \"Sample\", input);\nAppConfigHelper.TryGet<string>(path, \"Sample\", out var value);", text =>
            { var path = Path.Combine(TemporaryDirectory(), "demo.config"); AppConfigHelper.SetOrAdd(path, "Sample", text); if (!AppConfigHelper.TryGet<string>(path, "Sample", out var value)) throw new InvalidOperationException("Config round trip failed."); return $"{path}\nSample = {value}"; }),
        "JsonExtensions" => Sync(key, "Ada", "model.ToJson(jsonTypeInfo, out var json, out var error);\njson.FromJson(jsonTypeInfo, out var model, out var error);", Json),
        "YamlExtensions" => Sync(key, "Ada", "model.ToYaml(jsonTypeInfo, out var yaml, out var error);\nyaml.FromYaml(jsonTypeInfo, out var model, out var error);", Yaml),
        "DataTableExtensions" => Sync(key, "Ada,42\nLin,24", "table.ExportToCsv(path, Encoding.UTF8, out var error);\npath.ImportFromCsv(out var error, out var restored);", Csv),
        "PrismExtensions" => Sync(key, "", "registry.AddEventBus(assembly);\ncontainer.UseEventBus(assembly);\ncontainer.Resolve<IEventBus>().Query(query);", _ =>
            { var registration = new BusRegistrationViewModel(); if (!registration.RunPrism()) throw new InvalidOperationException(registration.PrismOutput); return registration.PrismOutput; }),
        "AspNetCoreExtensions" => Sync(key, "", "services.AddEventBus(assembly);\napplicationBuilder.UseEventBus(assembly);\nprovider.GetRequiredService<IEventBus>().Query(query);", _ =>
            { var registration = new BusRegistrationViewModel(); if (!registration.RunAspNetCore()) throw new InvalidOperationException(registration.AspNetOutput); return registration.AspNetOutput; }),
        "EventServer" => Async(key, "Hello broker", "await broker.StartAsync(\"127.0.0.1\", port);\n// Local clients validate broker routing.\nbroker.Stop();", EventDelivery),
        "EventClient" => Async(key, "Hello subscriber", "await client.ConnectAsync(\"127.0.0.1\", port);\nclient.Subscribe<string>(subject, receive);\nawait publisher.PublishAsync(subject, input);", EventDelivery),
        "TcpSocketServer" => Async(key, "Hello TCP", "await server.StartAsync(mark, \"127.0.0.1\", port);\nserver.RegisterCommandHandler(receive);\nawait server.StopAsync();", TcpDelivery),
        "TcpSocketClient" => Async(key, "Hello TCP", "await client.ConnectAsync(mark, \"127.0.0.1\", port);\nawait client.SendCommandAsync(packet);\nawait client.StopAsync();", TcpDelivery),
        "UdpSocketServer" => Async(key, "239.1.1.1", "server.Start(mark, systemId, group, port, localIp);\nawait server.SendCommandAsync(packet, DateTimeOffset.UtcNow);\nserver.Stop();", UdpDelivery),
        "UdpSocketClient" => Async(key, "239.1.1.1", "client.Received += Receive;\nawait client.ConnectAsync(mark, group, port, endpoint, systemId);\nclient.Stop();", UdpDelivery),
        "NetHelper" => Sync(key, "", "NetHelper.GetTaskId();\nNetHelper.GetIgnoreTask();", _ => $"TaskId = {NetHelper.GetTaskId()}\nIgnoreTask = {NetHelper.GetIgnoreTask()}"),
        "TcpSocketClientFileSystemFeature" => Async(key, "", "using var feature = new TcpSocketClientFileSystemFeature(client);\nawait feature.BrowseFileSystemAsync(remotePath);", FileSystemClient),
        "TcpSocketServerFileSystemFeature" => Sync(key, "", "using var feature = new TcpSocketServerFileSystemFeature(server);\nfeature.RootDirectory = isolatedDirectory;", _ =>
            { var server = new TcpSocketServer(); using var feature = new TcpSocketServerFileSystemFeature(server) { RootDirectory = TemporaryDirectory() }; return $"RootDirectory = {feature.RootDirectory}\nManagedFileSystem = {feature.ManagedFileSystem.GetType().Name}"; }),
        "LogEventFeed" => Sync(key, "Sample log event", "Logger.Info(input);\nLogger.Events.GetRecentEvents();", text =>
            { Logger.Info(text); return string.Join('\n', Logger.Events.GetRecentEvents().TakeLast(5).Select(entry => entry.ToString())); }),
        "CodeWFLogHealth" => Sync(key, "", "Logger.Health.GetSnapshot();", _ => Logger.Health.GetSnapshot().ToString()),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown helper API.")
    };

    private static HelperExample Sync(string key, string input, string code, Func<string, string> execute, bool smoke = true)
        => new(key, input, code, text => Task.Run(() => execute(text)), smoke);
    private static HelperExample Async(string key, string input, string code, Func<string, Task<string>> execute, bool smoke = true)
        => new(key, input, code, execute, smoke);
    private static double ParseNumber(string input) => double.Parse(input, CultureInfo.InvariantCulture);
    private static string TemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "catalog", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
    private static string NativeMethod(string method)
    {
        if (!OperatingSystem.IsWindows())
            return global::CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource("Demo.Catalog.WindowsOnly");
        if (!NativeLibraryHelper.GetMethod(Environment.SystemDirectory, "kernel32", "", "", method.Trim(), out var address, out var error))
            throw new InvalidOperationException(error);
        return $"Resolved = True\nAddress = {address}";
    }
    private static string Archive(string text)
    {
        var directory = TemporaryDirectory();
        var source = Path.Combine(directory, "source");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "sample.txt"), text);
        var zip = Path.Combine(directory, "sample.zip");
        using var http = new HttpClient();
        var compressor = new SevenZipCompressor(http);
        compressor.Zip(source, zip);
        var output = Path.Combine(directory, "restored");
        compressor.Decompress(zip, output);
        var restored = Directory.GetFiles(output, "sample.txt", SearchOption.AllDirectories).Single();
        return $"{zip}\n{File.ReadAllText(restored)}";
    }
    private static string Json(string text)
    {
        var sample = new Student { Name = text };
        if (!sample.ToJson(OverviewJsonContext.Default.Student, out var output, out var error)) throw new InvalidOperationException(error);
        if (!output.FromJson(OverviewJsonContext.Default.Student, out var restored, out error)) throw new InvalidOperationException(error);
        return $"{output}\nName = {restored?.Name}";
    }
    private static string Yaml(string text)
    {
        var sample = new Student { Name = text };
        if (!sample.ToYaml(OverviewJsonContext.Default.Student, out var output, out var error)) throw new InvalidOperationException(error);
        if (!output.FromYaml(OverviewJsonContext.Default.Student, out var restored, out error)) throw new InvalidOperationException(error);
        return $"{output}\nName = {restored?.Name}";
    }
    private static string Csv(string text)
    {
        using var table = new DataTable();
        table.Columns.Add("Name"); table.Columns.Add("Value");
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries)) { var cells = line.Split(','); table.Rows.Add(cells[0], cells.ElementAtOrDefault(1) ?? ""); }
        var path = Path.Combine(TemporaryDirectory(), "sample.csv");
        if (!table.ExportToCsv(path, Encoding.UTF8, out var error)) throw new InvalidOperationException(error);
        if (!path.ImportFromCsv(out error, out var restored)) throw new InvalidOperationException(error);
        using (restored) return $"{path}\nRows = {restored?.Rows.Count}\n{File.ReadAllText(path)}";
    }

    public sealed class ValidationSample
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
