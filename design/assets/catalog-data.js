"use strict";
window.CodeWFDemoCatalog = [
  {
    "titleKey": "Demo.Group.Home",
    "titleZh": "首页",
    "titleEn": "Home",
    "id": "home",
    "items": [
      {
        "titleKey": "Demo.Item.Welcome",
        "page": "WelcomePage",
        "api": "",
        "package": "",
        "prototypeId": "welcome",
        "titleZh": "介绍",
        "titleEn": "Introduction",
        "description": ""
      },
      {
        "titleKey": "Demo.Item.About",
        "page": "AboutPage",
        "api": "",
        "package": "",
        "prototypeId": "about",
        "titleZh": "关于",
        "titleEn": "About",
        "description": ""
      }
    ]
  },
  {
    "titleKey": "Demo.Group.Extensions",
    "titleZh": "核心扩展",
    "titleEn": "Core Extensions",
    "id": "extensions",
    "items": [
      {
        "titleKey": "Demo.Item.StringExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"StringExtensions\"))",
        "api": "StringExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "stringextensions",
        "titleZh": "StringExtensions",
        "titleEn": "StringExtensions",
        "description": "StringExtensions",
        "input": "one,,two,three",
        "code": "input.Split(',').Join(\" / \", removeEmptyEntry: true);"
      },
      {
        "titleKey": "Demo.Item.RegexExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"RegexExtensions\"))",
        "api": "RegexExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "regexextensions",
        "titleZh": "RegexExtensions",
        "titleEn": "RegexExtensions",
        "description": "RegexExtensions",
        "input": "item42\nitem##",
        "code": "text.IsMatch(pattern);\npattern.ConvertPattern();"
      },
      {
        "titleKey": "Demo.Item.DoubleExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"DoubleExtensions\"))",
        "api": "DoubleExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "doubleextensions",
        "titleZh": "DoubleExtensions",
        "titleEn": "DoubleExtensions",
        "description": "DoubleExtensions",
        "input": "1048576",
        "code": "double.Parse(input).FormatBytes();"
      },
      {
        "titleKey": "Demo.Item.NumberExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"NumberExtension\"))",
        "api": "NumberExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "numberextension",
        "titleZh": "NumberExtension",
        "titleEn": "NumberExtension",
        "description": "NumberExtension",
        "input": "123.45",
        "code": "number.ToChineseNumber();\nNumberExtension.ToChineseMoney(number);"
      },
      {
        "titleKey": "Demo.Item.DateTimeExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"DateTimeExtension\"))",
        "api": "DateTimeExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "datetimeextension",
        "titleZh": "DateTimeExtension",
        "titleEn": "DateTimeExtension",
        "description": "DateTimeExtension",
        "input": "2026-10-13T12:00:00Z",
        "code": "date.GetUnixTimeSeconds();\ndate.GetUnixTimeMilliseconds();"
      },
      {
        "titleKey": "Demo.Item.TimeSpanExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"TimeSpanExtensions\"))",
        "api": "TimeSpanExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "timespanextensions",
        "titleZh": "TimeSpanExtensions",
        "titleEn": "TimeSpanExtensions",
        "description": "TimeSpanExtensions",
        "input": "3661234",
        "code": "milliseconds.FormatMilliseconds();"
      },
      {
        "titleKey": "Demo.Item.RandomExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"RandomExtension\"))",
        "api": "RandomExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "randomextension",
        "titleZh": "RandomExtension",
        "titleEn": "RandomExtension",
        "description": "RandomExtension",
        "input": "1,100",
        "code": "RandomExtension.GetInt(min, max);"
      },
      {
        "titleKey": "Demo.Item.IConvertibleExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"IConvertibleExtensions\"))",
        "api": "IConvertibleExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "iconvertibleextensions",
        "titleZh": "IConvertibleExtensions",
        "titleEn": "IConvertibleExtensions",
        "description": "IConvertibleExtensions",
        "input": "42",
        "code": "input.ConvertTo<int>();\ntype.IsNumeric();"
      },
      {
        "titleKey": "Demo.Item.ObjectExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"ObjectExtension\"))",
        "api": "ObjectExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "objectextension",
        "titleZh": "ObjectExtension",
        "titleEn": "ObjectExtension",
        "description": "ObjectExtension",
        "input": "",
        "code": "input.IsNullOrEmpty();\ntype.IsPrimitive();"
      },
      {
        "titleKey": "Demo.Item.LinqExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"LinqExtension\"))",
        "api": "LinqExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "linqextension",
        "titleZh": "LinqExtension",
        "titleEn": "LinqExtension",
        "description": "LinqExtension",
        "input": "3",
        "code": "positive.And(value => value < limit).Compile(preferInterpretation: true);"
      },
      {
        "titleKey": "Demo.Item.EnumExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"EnumExtension\"))",
        "api": "EnumExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "enumextension",
        "titleZh": "EnumExtension",
        "titleEn": "EnumExtension",
        "description": "EnumExtension",
        "input": "",
        "code": "typeof(DayOfWeek).GetDictionary();"
      },
      {
        "titleKey": "Demo.Item.AssemblyExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AssemblyExtensions\"))",
        "api": "AssemblyExtensions",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "assemblyextensions",
        "titleZh": "AssemblyExtensions",
        "titleEn": "AssemblyExtensions",
        "description": "AssemblyExtensions",
        "input": "",
        "code": "assembly.Title();\nassembly.Version();\nassembly.Company();"
      },
      {
        "titleKey": "Demo.Item.ValidationExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"ValidationExtension\"))",
        "api": "ValidationExtension",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "validationextension",
        "titleZh": "ValidationExtension",
        "titleEn": "ValidationExtension",
        "description": "ValidationExtension",
        "input": "",
        "code": "new ValidationSample { Name = input }.TryValidateObject(out var errors);"
      },
      {
        "titleKey": "Demo.Item.DateTimeConverter",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"DateTimeConverter\"))",
        "api": "DateTimeConverter",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "datetimeconverter",
        "titleZh": "DateTimeConverter",
        "titleEn": "DateTimeConverter",
        "description": "DateTimeConverter",
        "input": "2026-10-13T12:00:00Z",
        "code": "DateTimeConverter.IsISO8601DateTimeString(input);\nDateTimeConverter.DateToExcelFormat(date);"
      }
    ]
  },
  {
    "titleKey": "Demo.Group.System",
    "titleZh": "系统与文件",
    "titleEn": "System & Files",
    "id": "system",
    "items": [
      {
        "titleKey": "Demo.Item.ActionHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"ActionHelper\"))",
        "api": "ActionHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "actionhelper",
        "titleZh": "ActionHelper",
        "titleEn": "ActionHelper",
        "description": "ActionHelper",
        "input": "20,100",
        "code": "ActionHelper.CheckOvertime(() => Thread.Sleep(delayMilliseconds), timeoutMilliseconds);\n// true = completed within the timeout; false = timed out."
      },
      {
        "titleKey": "Demo.Item.IpHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"IpHelper\"))",
        "api": "IpHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "iphelper",
        "titleZh": "IpHelper",
        "titleEn": "IpHelper",
        "description": "IpHelper",
        "input": "127.0.0.1:39561",
        "code": "IpHelper.TrySplitIpPort(input, out var ip, out var port);"
      },
      {
        "titleKey": "Demo.Item.NativeLibraryHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"NativeLibraryHelper\"))",
        "api": "NativeLibraryHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "nativelibraryhelper",
        "titleZh": "NativeLibraryHelper",
        "titleEn": "NativeLibraryHelper",
        "description": "NativeLibraryHelper",
        "input": "GetTickCount64",
        "code": "NativeLibraryHelper.GetMethod(Environment.SystemDirectory, \"kernel32\", \"\", \"\", method, out var address, out var error);"
      },
      {
        "titleKey": "Demo.Item.FileHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"FileHelper\"))",
        "api": "FileHelper",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "filehelper",
        "titleZh": "FileHelper",
        "titleEn": "FileHelper",
        "description": "FileHelper",
        "input": "Hello, CodeWF!",
        "code": "await FileHelper.SafeWriteAllTextAsync(path, input);\nawait FileHelper.SafeReadAllTextAsync(path);"
      },
      {
        "titleKey": "Demo.Item.AppDataMigrator",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AppDataMigrator\"))",
        "api": "AppDataMigrator",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "appdatamigrator",
        "titleZh": "AppDataMigrator",
        "titleEn": "AppDataMigrator",
        "description": "AppDataMigrator",
        "input": "demo.json",
        "code": "AppDataMigrator.ResolveAndMigrate(appName, fileName, baseDirectory, portableMarkerFileName);"
      },
      {
        "titleKey": "Demo.Item.UpdateChecker",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"UpdateChecker\"))",
        "api": "UpdateChecker",
        "package": "CodeWF.Toolkit.Core",
        "prototypeId": "updatechecker",
        "titleZh": "UpdateChecker",
        "titleEn": "UpdateChecker",
        "description": "UpdateChecker",
        "input": "dotnet9/CodeWF.Toolkit",
        "code": "await new UpdateChecker(httpClient, owner, repository).CheckAsync(currentVersion);"
      },
      {
        "titleKey": "Demo.Item.FileExtension",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"FileExtension\"))",
        "api": "FileExtension",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "fileextension",
        "titleZh": "FileExtension",
        "titleEn": "FileExtension",
        "description": "FileExtension",
        "input": "..",
        "code": "FileExtension.IsPathInUpperTwoLevelsOfCurrentDir(input);"
      },
      {
        "titleKey": "Demo.Item.SevenZipCompressor",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"SevenZipCompressor\"))",
        "api": "SevenZipCompressor",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "files-archive",
        "titleZh": "SevenZipCompressor",
        "titleEn": "SevenZipCompressor",
        "description": "SevenZipCompressor",
        "input": "Hello archive",
        "code": "compressor.Zip(sourceDirectory, zipPath);\ncompressor.Decompress(zipPath, destinationDirectory);"
      },
      {
        "titleKey": "Demo.Item.AppConfigHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AppConfigHelper\"))",
        "api": "AppConfigHelper",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "appconfighelper",
        "titleZh": "AppConfigHelper",
        "titleEn": "AppConfigHelper",
        "description": "AppConfigHelper",
        "input": "42",
        "code": "AppConfigHelper.SetOrAdd(path, \"Sample\", input);\nAppConfigHelper.TryGet<string>(path, \"Sample\", out var value);"
      }
    ]
  },
  {
    "titleKey": "Demo.Group.Data",
    "titleZh": "数据与图像",
    "titleEn": "Data & Images",
    "id": "data",
    "items": [
      {
        "titleKey": "Demo.Item.JsonExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"JsonExtensions\"))",
        "api": "JsonExtensions",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "json",
        "titleZh": "JsonExtensions",
        "titleEn": "JsonExtensions",
        "description": "JsonExtensions",
        "input": "Ada",
        "code": "model.ToJson(jsonTypeInfo, out var json, out var error);\njson.FromJson(jsonTypeInfo, out var model, out var error);"
      },
      {
        "titleKey": "Demo.Item.YamlExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"YamlExtensions\"))",
        "api": "YamlExtensions",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "yaml",
        "titleZh": "YamlExtensions",
        "titleEn": "YamlExtensions",
        "description": "YamlExtensions",
        "input": "Ada",
        "code": "model.ToYaml(jsonTypeInfo, out var yaml, out var error);\nyaml.FromYaml(jsonTypeInfo, out var model, out var error);"
      },
      {
        "titleKey": "Demo.Item.DataTableExtensions",
        "page": "DataTableDemoPage()",
        "api": "DataTableExtensions",
        "package": "CodeWF.Toolkit.Files",
        "prototypeId": "datatable",
        "titleZh": "DataTableExtensions",
        "titleEn": "DataTableExtensions",
        "description": "DataTableExtensions",
        "input": "Ada,42\nLin,24",
        "code": "table.ExportToCsv(csvPath, Encoding.UTF8, out var error);\ncsvPath.ImportFromCsv(out error, out var csvTable);\ntable.ExportToXlsx(excelPath, out error);\nexcelPath.ImportFromExcel(out error, out var excelTable);"
      },
      {
        "titleKey": "Demo.Item.ImageIcon",
        "page": "ImageIconPage",
        "api": "ImageHelper",
        "package": "CodeWF.Toolkit.Image",
        "prototypeId": "image-icon",
        "titleZh": "ImageHelper",
        "titleEn": "ImageHelper",
        "description": "ImageHelper"
      },
      {
        "titleKey": "Demo.Item.ImageQr",
        "page": "ImageQrPage",
        "api": "QrCodeGenerator",
        "package": "CodeWF.Toolkit.Image",
        "prototypeId": "image-qr",
        "titleZh": "QrCodeGenerator",
        "titleEn": "QrCodeGenerator",
        "description": "QrCodeGenerator"
      }
    ]
  },
  {
    "titleKey": "Demo.Group.Events",
    "titleZh": "事件与集成",
    "titleEn": "Events & Integration",
    "id": "events",
    "items": [
      {
        "titleKey": "Demo.Item.Bus",
        "page": "EventBusPage",
        "api": "EventBus",
        "package": "CodeWF.Toolkit.EventBus",
        "prototypeId": "bus",
        "titleZh": "EventBus",
        "titleEn": "EventBus",
        "description": "EventBus"
      },
      {
        "titleKey": "Demo.Item.PrismExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"PrismExtensions\"))",
        "api": "Prism.EventBusExtensions",
        "package": "CodeWF.Toolkit.EventBus.Prism",
        "prototypeId": "prism",
        "titleZh": "Prism.EventBusExtensions",
        "titleEn": "Prism.EventBusExtensions",
        "description": "PrismExtensions",
        "input": "",
        "code": "registry.AddEventBus(assembly);\ncontainer.UseEventBus(assembly);\ncontainer.Resolve<IEventBus>().Query(query);"
      },
      {
        "titleKey": "Demo.Item.AspNetCoreExtensions",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"AspNetCoreExtensions\"))",
        "api": "AspNetCore.EventBusExtensions",
        "package": "CodeWF.Toolkit.EventBus.AspNetCore",
        "prototypeId": "aspnet",
        "titleZh": "AspNetCore.EventBusExtensions",
        "titleEn": "AspNetCore.EventBusExtensions",
        "description": "AspNetCoreExtensions",
        "input": "",
        "code": "services.AddEventBus(assembly);\napplicationBuilder.UseEventBus(assembly);\nprovider.GetRequiredService<IEventBus>().Query(query);"
      },
      {
        "titleKey": "Demo.Item.EventServer",
        "page": "TransportDemoPage(\"EventServer\")",
        "api": "EventServer",
        "package": "CodeWF.Toolkit.EventBus.Socket",
        "prototypeId": "event-server",
        "titleZh": "EventServer",
        "titleEn": "EventServer",
        "description": "EventServer",
        "input": "Hello broker",
        "code": "await broker.StartAsync(\"127.0.0.1\", port);\n// Local clients validate broker routing.\nbroker.Stop();"
      },
      {
        "titleKey": "Demo.Item.EventClient",
        "page": "TransportDemoPage(\"EventClient\")",
        "api": "EventClient",
        "package": "CodeWF.Toolkit.EventBus.Socket",
        "prototypeId": "event-client",
        "titleZh": "EventClient",
        "titleEn": "EventClient",
        "description": "EventClient",
        "input": "Hello subscriber",
        "code": "await client.ConnectAsync(\"127.0.0.1\", port);\nclient.Subscribe<string>(subject, receive);\nawait publisher.PublishAsync(subject, input);"
      }
    ]
  },
  {
    "titleKey": "Demo.Group.Network",
    "titleZh": "协议与网络",
    "titleEn": "Protocol & Network",
    "id": "network",
    "items": [
      {
        "titleKey": "Demo.Item.NetPacket",
        "page": "NetPacketPage",
        "api": "SerializeHelper",
        "package": "CodeWF.Toolkit.NetWeaver",
        "prototypeId": "net-packet",
        "titleZh": "SerializeHelper",
        "titleEn": "SerializeHelper",
        "description": "SerializeHelper"
      },
      {
        "titleKey": "Demo.Item.TcpSocketServer",
        "page": "TransportDemoPage(\"TcpSocketServer\")",
        "api": "TcpSocketServer",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketserver",
        "titleZh": "TcpSocketServer",
        "titleEn": "TcpSocketServer",
        "description": "TcpSocketServer",
        "input": "Hello TCP",
        "code": "await server.StartAsync(mark, \"127.0.0.1\", port);\nserver.RegisterCommandHandler(receive);\nawait server.StopAsync();"
      },
      {
        "titleKey": "Demo.Item.TcpSocketClient",
        "page": "TransportDemoPage(\"TcpSocketClient\")",
        "api": "TcpSocketClient",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketclient",
        "titleZh": "TcpSocketClient",
        "titleEn": "TcpSocketClient",
        "description": "TcpSocketClient",
        "input": "Hello TCP",
        "code": "await client.ConnectAsync(mark, \"127.0.0.1\", port);\nawait client.SendCommandAsync(packet);\nawait client.StopAsync();"
      },
      {
        "titleKey": "Demo.Item.UdpSocketServer",
        "page": "TransportDemoPage(\"UdpSocketServer\")",
        "api": "UdpSocketServer",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "udpsocketserver",
        "titleZh": "UdpSocketServer",
        "titleEn": "UdpSocketServer",
        "description": "UdpSocketServer",
        "input": "239.1.1.1",
        "code": "server.Start(mark, systemId, group, port, localIp);\nclient.Received += Receive;\nawait client.ConnectAsync(mark, group, port, endpoint, systemId);\nawait server.SendCommandAsync(packet, DateTimeOffset.UtcNow);"
      },
      {
        "titleKey": "Demo.Item.UdpSocketClient",
        "page": "TransportDemoPage(\"UdpSocketClient\")",
        "api": "UdpSocketClient",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "udpsocketclient",
        "titleZh": "UdpSocketClient",
        "titleEn": "UdpSocketClient",
        "description": "UdpSocketClient",
        "input": "239.1.1.1",
        "code": "server.Start(mark, systemId, group, port, localIp);\nclient.Received += Receive;\nawait client.ConnectAsync(mark, group, port, endpoint, systemId);\nawait server.SendCommandAsync(packet, DateTimeOffset.UtcNow);"
      },
      {
        "titleKey": "Demo.Item.NetHelper",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"NetHelper\"))",
        "api": "NetHelper",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "nethelper",
        "titleZh": "NetHelper",
        "titleEn": "NetHelper",
        "description": "NetHelper",
        "input": "",
        "code": "NetHelper.GetTaskId();\nNetHelper.GetIgnoreTask();"
      },
      {
        "titleKey": "Demo.Item.TcpSocketClientFileSystemFeature",
        "page": "FileSystemDemoPage(false)",
        "api": "TcpSocketClientFileSystemFeature",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketclientfilesystemfeature",
        "titleZh": "TcpSocketClientFileSystemFeature",
        "titleEn": "TcpSocketClientFileSystemFeature",
        "description": "TcpSocketClientFileSystemFeature",
        "input": "",
        "code": "feature.FileTransferProgress += OnProgress;\nfeature.FileTransferOutcome += OnOutcome;\nawait feature.BrowseFileSystemAsync(root);\nawait feature.CreateDirectoryAsync(path);\nawait feature.UploadFileAsync(localFile, remoteFile, cancellationToken);\nawait feature.DownloadFileAsync(remoteFile, localDirectory, cancellationToken);\nawait feature.DeletePathAsync(path, true);"
      },
      {
        "titleKey": "Demo.Item.TcpSocketServerFileSystemFeature",
        "page": "FileSystemDemoPage(true)",
        "api": "TcpSocketServerFileSystemFeature",
        "package": "CodeWF.Toolkit.NetWrapper",
        "prototypeId": "tcpsocketserverfilesystemfeature",
        "titleZh": "TcpSocketServerFileSystemFeature",
        "titleEn": "TcpSocketServerFileSystemFeature",
        "description": "TcpSocketServerFileSystemFeature",
        "input": "",
        "code": "using var feature = new TcpSocketServerFileSystemFeature(server) { RootDirectory = isolatedRoot };\nawait server.StartAsync(mark, \"127.0.0.1\", 0);\nawait server.StopAsync();"
      }
    ]
  },
  {
    "titleKey": "Demo.Group.Logging",
    "titleZh": "日志与观测",
    "titleEn": "Logging & Diagnostics",
    "id": "logging",
    "items": [
      {
        "titleKey": "Demo.Item.LogCore",
        "page": "LoggingCorePage",
        "api": "Logger",
        "package": "CodeWF.Toolkit.Logging",
        "prototypeId": "log-core",
        "titleZh": "Logger",
        "titleEn": "Logger",
        "description": "Logger"
      },
      {
        "titleKey": "Demo.Item.LogMicrosoft",
        "page": "LoggingMicrosoftPage",
        "api": "CodeWFLoggingBuilderExtensions",
        "package": "CodeWF.Toolkit.Logging.Microsoft",
        "prototypeId": "log-microsoft",
        "titleZh": "CodeWFLoggingBuilderExtensions",
        "titleEn": "CodeWFLoggingBuilderExtensions",
        "description": "CodeWFLoggingBuilderExtensions"
      },
      {
        "titleKey": "Demo.Item.LogEventFeed",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"LogEventFeed\"))",
        "api": "LogEventFeed",
        "package": "CodeWF.Toolkit.Logging",
        "prototypeId": "logeventfeed",
        "titleZh": "LogEventFeed",
        "titleEn": "LogEventFeed",
        "description": "LogEventFeed",
        "input": "Sample log event",
        "code": "Logger.Info(input);\nLogger.Events.GetRecentEvents();"
      },
      {
        "titleKey": "Demo.Item.CodeWFLogHealth",
        "page": "HelperDemoPage(HelperDemoCatalog.Create(\"CodeWFLogHealth\"))",
        "api": "CodeWFLogHealth",
        "package": "CodeWF.Toolkit.Logging",
        "prototypeId": "loggerselfdiagnostics",
        "titleZh": "CodeWFLogHealth",
        "titleEn": "CodeWFLogHealth",
        "description": "CodeWFLogHealth",
        "input": "",
        "code": "Logger.Health.GetSnapshot();"
      }
    ]
  },
  {
    "titleKey": "Demo.Group.Examples",
    "titleZh": "综合示例",
    "titleEn": "Examples",
    "id": "examples",
    "items": [
      {
        "titleKey": "Demo.Item.RuntimeFlow",
        "page": "RuntimeFlowPage",
        "api": "",
        "package": "",
        "prototypeId": "flow-runtime",
        "titleZh": "事件与日志导览",
        "titleEn": "Events and logging tour",
        "description": ""
      },
      {
        "titleKey": "Demo.Item.LabFlow",
        "page": "LabFlowPage",
        "api": "",
        "package": "",
        "prototypeId": "flow-lab",
        "titleZh": "文件处理工作流",
        "titleEn": "File workflow",
        "description": ""
      }
    ]
  }
];
window.CodeWFCatalogText = {
  "zh-CN": {
    "Title": "帮助类目录",
    "Intro": "每个菜单对应一个帮助类；输入参数后调用真实 API。",
    "Examples": "综合示例与交互实验",
    "Source": "查看源码",
    "Input": "输入参数",
    "Run": "运行示例",
    "Output": "运行结果",
    "Ready": "修改输入后运行示例。",
    "Failed": "调用失败：{0}",
    "About": "关于项目",
    "AboutDesc": "开源库的 API 演示。Ribbon 负责分类，Dock 承载演示、属性与日志。",
    "Project": "项目与安装",
    "License": "MIT 开源许可 · .NET 10 · Avalonia 12",
    "Notification": "由 LogNotifications 发出的日志通知。",
    "Notifications": "发送日志通知",
    "Preview": "浏览器交互示意；完整调用结果由 .NET Demo 产生。",
    "WindowsOnly": "此帮助类目前仅支持 Windows。",
    "TableCsv": "CSV 导出并回读",
    "TableExcel": "Excel 导出并回读",
    "TableHint": "每行输入 Name,Value。分别调用 CSV / Excel 导出与导入 API，预览回读数据并校验单元格一致；结果文件保存在显示的临时目录。",
    "TableFormat": "每行须包含两个以逗号分隔的值，且至少有一行数据。",
    "ActionFormat": "输入 操作耗时,等待上限（毫秒），两项均为 0–5000。",
    "ActionCompleted": "true：操作在等待上限内完成。",
    "ActionTimedOut": "false：等待超时，后台操作仍会继续；该方法不会取消操作。",
    "FileSession": "本机文件系统会话",
    "FileOperations": "远程目录与传输",
    "FileStart": "启动并连接",
    "FileStop": "断开并停止",
    "FileBrowse": "浏览目录",
    "FileCreate": "创建 demo-folder",
    "FileDelete": "删除 demo-folder",
    "FileUpload": "上传 4 MB 样例",
    "FileDownload": "下载并校验",
    "FileCancel": "取消传输",
    "FileCancelled": "传输已取消。可再次上传或下载以观察续传。",
    "FileStopped": "连接与服务已停止。",
    "FileStartFirst": "请先启动本机会话。",
    "FileServerHint": "服务端绑定本机随机端口，RootDirectory 限定独立临时目录。下方客户端请求可观察浏览、创建、删除与文件传输响应。关闭文档会停止服务。",
    "FileClientHint": "启动本机配套服务后执行真实请求。先上传再下载，传输完成后比较 SHA256；可取消传输。只操作本次会话的临时目录。",
    "TransportHint": "启动本机配套端点，保持会话以重复发送、观察连接与接收结果；事件总线可切换订阅。关闭文档会停止会话。",
    "TransportStarted": "端点已启动并连接。",
    "TransportSend": "发送并验证接收",
    "TransportConnected": "连接",
    "TransportSubscribe": "订阅 demo.message",
    "TransportNoSubscription": "已发布；当前没有订阅，未等待接收。",
    "RuntimeTourHint": "依次学习总线、注册、发布订阅、查询集成与日志。各步骤是独立示例，拥有各自的状态和会话。",
    "RuntimeTourSteps": "总线基础|依赖注入|发布与订阅|查询与集成|观测日志"
  },
  "en-US": {
    "Title": "Helper catalog",
    "Intro": "Find demos by API name. Ribbon and explorer share the same catalog.",
    "Examples": "Examples and experiments",
    "Source": "View source",
    "Input": "Input",
    "Run": "Run example",
    "Output": "Output",
    "Ready": "Edit the input and run the example.",
    "Failed": "Call failed: {0}",
    "About": "About this project",
    "AboutDesc": "An open source API gallery. Ribbon organizes APIs; Dock hosts demos, properties and logs.",
    "Project": "Project and installation",
    "License": "MIT license · .NET 10 · Avalonia 12",
    "Notification": "A log notification presented by LogNotifications.",
    "Notifications": "Send log notification",
    "Preview": "Browser interaction preview; the .NET demo runs the complete API call.",
    "WindowsOnly": "This helper currently supports Windows only.",
    "TableCsv": "Export / import CSV",
    "TableExcel": "Export / import Excel",
    "TableHint": "Enter Name,Value per line. Export and import CSV or Excel, preview the imported rows and verify every cell. Result files remain in the displayed temporary directory.",
    "TableFormat": "Enter at least one row with two comma-separated values.",
    "ActionFormat": "Enter action duration,timeout in milliseconds, both between 0 and 5000.",
    "ActionCompleted": "true: the action completed within the timeout.",
    "ActionTimedOut": "false: the wait timed out. The action continues in the background; this method does not cancel it.",
    "FileSession": "Local file-system session",
    "FileOperations": "Remote directory and transfers",
    "FileStart": "Start and connect",
    "FileStop": "Disconnect and stop",
    "FileBrowse": "Browse",
    "FileCreate": "Create demo-folder",
    "FileDelete": "Delete demo-folder",
    "FileUpload": "Upload 4 MB sample",
    "FileDownload": "Download and verify",
    "FileCancel": "Cancel transfer",
    "FileCancelled": "Transfer cancelled. Upload or download again to observe resumption.",
    "FileStopped": "Connection and server stopped.",
    "FileStartFirst": "Start the local session first.",
    "FileServerHint": "The server binds a random loopback port with RootDirectory restricted to a fresh temporary directory. Use the local client below to observe real browse, create, delete and transfer responses. Closing the document stops the session.",
    "FileClientHint": "Start the local peer server, then send real requests. Upload before downloading; completed files are checked with SHA256. Transfers can be cancelled. All operations stay inside this session’s temporary directory.",
    "TransportHint": "Start local peer endpoints and keep the session open to send repeatedly and inspect connection and delivery. Toggle subscriptions for the event bus. Closing the document stops the session.",
    "TransportStarted": "Endpoints started and connected.",
    "TransportSend": "Send and verify delivery",
    "TransportConnected": "Connected",
    "TransportSubscribe": "Subscribe demo.message",
    "TransportNoSubscription": "Published without an active subscription; no delivery was awaited.",
    "RuntimeTourHint": "Explore bus basics, registration, pub/sub, integration and logging. Each step is an independent example with its own state and session.",
    "RuntimeTourSteps": "Bus basics|Dependency injection|Publish and subscribe|Queries and integration|Observe logs"
  }
};
window.CodeWFCatalogDescriptions = {
  "zh-CN": {
    "StringExtensions": {
      "Desc": "分割并连接字符串，控制空项处理。"
    },
    "RegexExtensions": {
      "Desc": "将通配模式转为正则表达式，并匹配输入文本。"
    },
    "DoubleExtensions": {
      "Desc": "将字节数格式化为可读的容量。"
    },
    "NumberExtension": {
      "Desc": "将数字转换为中文数值与金额表示。"
    },
    "DateTimeExtension": {
      "Desc": "转换日期与 Unix 秒、毫秒时间戳。"
    },
    "TimeSpanExtensions": {
      "Desc": "将毫秒数格式化为时长。"
    },
    "RandomExtension": {
      "Desc": "在指定范围内生成多个随机整数。"
    },
    "IConvertibleExtensions": {
      "Desc": "执行类型转换并检查数值类型。"
    },
    "ObjectExtension": {
      "Desc": "检测空值、空字符串与基础类型。"
    },
    "LinqExtension": {
      "Desc": "组合筛选表达式，并用解释执行方式运行查询。"
    },
    "EnumExtension": {
      "Desc": "读取枚举值与名称字典。"
    },
    "AssemblyExtensions": {
      "Desc": "读取程序集标题、版本与公司元数据。"
    },
    "ValidationExtension": {
      "Desc": "用数据注解验证对象，查看错误结果。"
    },
    "DateTimeConverter": {
      "Desc": "识别 ISO 8601 日期，转换 Excel 日期数值。"
    },
    "ActionHelper": {
      "Desc": "调整操作耗时与等待上限，观察限时完成和等待超时；超时不取消后台操作。"
    },
    "IpHelper": {
      "Desc": "解析 IP 与端口，列举本机 IPv4 地址。"
    },
    "NativeLibraryHelper": {
      "Desc": "在 Windows 中加载 kernel32 并解析导出函数地址。"
    },
    "FileHelper": {
      "Desc": "在独立临时目录写入并读取文本文件。"
    },
    "AppDataMigrator": {
      "Desc": "使用便携模式标记解析应用配置文件位置。"
    },
    "UpdateChecker": {
      "Desc": "点击运行后查询 GitHub Release，并比较当前版本。"
    },
    "FileExtension": {
      "Desc": "检查路径是否处于当前目录的上两级范围。"
    },
    "SevenZipCompressor": {
      "Desc": "生成 ZIP 归档并解压，读取还原文件。"
    },
    "AppConfigHelper": {
      "Desc": "写入配置项并读取，验证配置往返。"
    },
    "JsonExtensions": {
      "Desc": "通过源生成元数据序列化 JSON，并还原对象。"
    },
    "YamlExtensions": {
      "Desc": "通过源生成元数据序列化 YAML，并还原对象。"
    },
    "DataTableExtensions": {
      "Desc": "同页演示 CSV 与 Excel 导出、导入及真实表格回读校验。"
    },
    "ImageIcon": {
      "Desc": "从图片生成多尺寸 ICO 图标。"
    },
    "ImageQr": {
      "Desc": "从输入内容生成二维码图片。"
    },
    "Bus": {
      "Desc": "订阅、发布和查询消息，查看处理结果。"
    },
    "PrismExtensions": {
      "Desc": "通过 Prism 容器注册、扫描并执行事件总线查询。"
    },
    "AspNetCoreExtensions": {
      "Desc": "通过 ASP.NET Core 依赖注入注册并执行事件总线查询。"
    },
    "EventServer": {
      "Desc": "启动本机消息代理，用客户端验证订阅与投递。"
    },
    "EventClient": {
      "Desc": "连接本机消息代理，订阅并接收发布消息。"
    },
    "NetPacket": {
      "Desc": "序列化数据包并查看编码字节与解码字段。"
    },
    "TcpSocketServer": {
      "Desc": "启动回环 TCP 服务端，接收客户端发送的数据包。"
    },
    "TcpSocketClient": {
      "Desc": "连接回环 TCP 服务端并发送数据包。"
    },
    "UdpSocketServer": {
      "Desc": "启动 UDP 组播发送端，并验证本机订阅端收到的数据包。"
    },
    "UdpSocketClient": {
      "Desc": "订阅 UDP 组播，接收并解码本机发送的数据包。"
    },
    "NetHelper": {
      "Desc": "生成任务 ID，查看特殊任务标识。"
    },
    "TcpSocketClientFileSystemFeature": {
      "Desc": "通过回环 TCP 请求远程文件目录，查看响应。"
    },
    "TcpSocketServerFileSystemFeature": {
      "Desc": "设置独立文件根目录并创建服务端文件系统功能。"
    },
    "LogCore": {
      "Desc": "写入不同级别日志，查看实时日志面板。"
    },
    "LogMicrosoft": {
      "Desc": "将 Microsoft ILogger 接入 CodeWF 日志。"
    },
    "LogEventFeed": {
      "Desc": "订阅日志事件流，写入日志并观察投递。"
    },
    "CodeWFLogHealth": {
      "Desc": "读取日志健康快照，查看队列和诊断状态。"
    }
  },
  "en-US": {
    "StringExtensions": {
      "Desc": "Split and join strings with empty-entry handling."
    },
    "RegexExtensions": {
      "Desc": "Convert wildcard patterns to regular expressions and match input text."
    },
    "DoubleExtensions": {
      "Desc": "Format byte counts as readable sizes."
    },
    "NumberExtension": {
      "Desc": "Convert numbers to Chinese numerals and monetary text."
    },
    "DateTimeExtension": {
      "Desc": "Convert dates to Unix timestamps in seconds and milliseconds."
    },
    "TimeSpanExtensions": {
      "Desc": "Format millisecond values as durations."
    },
    "RandomExtension": {
      "Desc": "Generate random integers in a specified range."
    },
    "IConvertibleExtensions": {
      "Desc": "Convert values and detect numeric types."
    },
    "ObjectExtension": {
      "Desc": "Detect null or empty values and primitive types."
    },
    "LinqExtension": {
      "Desc": "Combine predicate expressions and run queries with interpreted expressions."
    },
    "EnumExtension": {
      "Desc": "Read a dictionary of enum values and names."
    },
    "AssemblyExtensions": {
      "Desc": "Read assembly title, version and company metadata."
    },
    "ValidationExtension": {
      "Desc": "Validate an object with data annotations and inspect validation errors."
    },
    "DateTimeConverter": {
      "Desc": "Recognize ISO 8601 dates and convert Excel date values."
    },
    "ActionHelper": {
      "Desc": "Adjust action duration and timeout to observe completion or timeout; timing out does not cancel the action."
    },
    "IpHelper": {
      "Desc": "Parse IP addresses and ports and list local IPv4 addresses."
    },
    "NativeLibraryHelper": {
      "Desc": "Load kernel32 on Windows and resolve an exported function address."
    },
    "FileHelper": {
      "Desc": "Write and read text in an isolated temporary directory."
    },
    "AppDataMigrator": {
      "Desc": "Resolve application configuration paths with a portable-mode marker."
    },
    "UpdateChecker": {
      "Desc": "Query GitHub releases on demand and compare the current version."
    },
    "FileExtension": {
      "Desc": "Check whether a path is within the upper two levels of the current directory."
    },
    "SevenZipCompressor": {
      "Desc": "Create and extract a ZIP archive and read the restored file."
    },
    "AppConfigHelper": {
      "Desc": "Write and read a configuration entry to verify a round trip."
    },
    "JsonExtensions": {
      "Desc": "Serialize JSON with generated metadata and restore the object."
    },
    "YamlExtensions": {
      "Desc": "Serialize YAML with generated metadata and restore the object."
    },
    "DataTableExtensions": {
      "Desc": "Export and import CSV and Excel, with a real table preview and cell validation."
    },
    "ImageIcon": {
      "Desc": "Generate multi-size ICO icons from an image."
    },
    "ImageQr": {
      "Desc": "Generate a QR code image from input text."
    },
    "Bus": {
      "Desc": "Subscribe, publish and query messages and inspect handler results."
    },
    "PrismExtensions": {
      "Desc": "Register and scan event-bus handlers through Prism and execute a query."
    },
    "AspNetCoreExtensions": {
      "Desc": "Register event-bus handlers with ASP.NET Core DI and execute a query."
    },
    "EventServer": {
      "Desc": "Start a local message broker and verify subscriptions and delivery with clients."
    },
    "EventClient": {
      "Desc": "Connect to a local broker, subscribe and receive published messages."
    },
    "NetPacket": {
      "Desc": "Serialize a packet and inspect encoded bytes and decoded fields."
    },
    "TcpSocketServer": {
      "Desc": "Start a loopback TCP server and receive a packet from its client."
    },
    "TcpSocketClient": {
      "Desc": "Connect to a loopback TCP server and send a packet."
    },
    "UdpSocketServer": {
      "Desc": "Start a UDP multicast sender and verify packet delivery to a local subscriber."
    },
    "UdpSocketClient": {
      "Desc": "Subscribe to UDP multicast and receive and decode a locally sent packet."
    },
    "NetHelper": {
      "Desc": "Generate a task ID and inspect the special task marker."
    },
    "TcpSocketClientFileSystemFeature": {
      "Desc": "Request a remote directory through loopback TCP and inspect the response."
    },
    "TcpSocketServerFileSystemFeature": {
      "Desc": "Set an isolated file root and create the server filesystem feature."
    },
    "LogCore": {
      "Desc": "Write logs at different levels and inspect the live log panel."
    },
    "LogMicrosoft": {
      "Desc": "Connect Microsoft ILogger to CodeWF logging."
    },
    "LogEventFeed": {
      "Desc": "Subscribe to the log event feed and inspect delivery after writing a log."
    },
    "CodeWFLogHealth": {
      "Desc": "Read a logging health snapshot with queue and diagnostic status."
    }
  }
};
