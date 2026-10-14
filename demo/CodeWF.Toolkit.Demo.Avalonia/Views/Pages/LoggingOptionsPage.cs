using DemoResources = global::CodeWF.Toolkit.Demo.Avalonia.I18n.ResourceKeys;
using Ui = global::CodeWF.Avalonia.Controls.Presentation.StudioUi;
using L = global::CodeWF.Avalonia.Lang.I18nManager;
using Avalonia.Controls;
using CodeWF.Avalonia.TreeDataGrid.PropertyGrid;
using CodeWF.Toolkit.Demo.Avalonia.ViewModels;
using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Demo.Avalonia.Views.Pages;

/// <summary>
/// 日志字典：LoggerOptions / FileLogOptions 参数全览，配 Logger.Health 与 Logger.Events 现场演示。
/// 也说明与 CodeWF.Avalonia.Log 的对接点（LogView 订阅的就是 Logger.Events 的 UI 事件流）。
/// </summary>
public sealed class LoggingOptionsPage : DemoPageBase
{
    private const string XamlSnippet = """
        <ItemGroup>
          <PackageReference Include="CodeWF.Toolkit.Logging" Version="1.0.0.1" />
        </ItemGroup>
        """;

    private static string CSharpSnippet => L.Instance.GetResource(DemoResources.Content.LoggingOptions.Source.CSharp);

    private readonly SelectableTextBlock _output;

    public LoggingOptionsPage()
    {
        _output = Ui.Code("@Demo.Content.LoggingOptions.Content.ReadyClickAButtonToViewDiagnosticsOr", output: true);

        Content = Ui.Page(
            Ui.Heading(I18n(DemoKeys.LoggingOptionsExampleTitle)),
            Ui.Sub(I18n(DemoKeys.DescLoggingOptions)),
            Ui.Card("@Demo.Content.LoggingOptions.Content.OptionsOverview", BuildOptionsTable()),
            Ui.Card("@Demo.Content.LoggingOptions.Content.HealthAndEventStream",
                Ui.Row(
                    Ui.Button("@Demo.Content.LoggingOptions.Content.LoggerHealthDiagnostics", ShowHealth, primary: true),
                    Ui.Button("@Demo.Content.LoggingOptions.Content.LoggerEventsSnapshot", ShowFeed),
                    Ui.Button("@Demo.Content.LoggingOptions.Content.WriteThreeSampleLogs", WriteSamples)),
                _output,
                Ui.Hint("@Demo.Content.LoggingOptions.Content.EventCallbacksRunOnABackgroundThreadUI")),
            Ui.Card("@Demo.Content.LoggingOptions.Content.IntegrationWithCodeWFAvalonia",
                Ui.Hint("@Demo.Content.LoggingOptions.Content.CodeWFAvaloniaLogLogViewSubscribesDirectlyToThis"
                        + L.Instance.GetResource(DemoResources.Content.LoggingOptions.Content.TheWorkspaceLogPanelIsAnInstanceOf))));
    }

    public override string TitleKey => DemoKeys.LoggingOptionsExampleTitle;

    public override string DescriptionKey => DemoKeys.DescLoggingOptions;

    public override string XamlCode => XamlSnippet;

    public override string CSharpCode => CSharpSnippet;

    protected override IReadOnlyList<PropertyItem> BuildProperties() =>
    [
        new PropertyItem
        {
            Id = "logging.level",
            Name = L.Instance.GetResource(DemoResources.Content.LoggingOptions.Properties.MinimumLevel),
            Kind = PropertyItemKind.Choice,
            Options = ["Trace", "Debug", "Information", "Warning", "Error"],
            Value = "Information",
            DefaultValue = "Information",
            Description = L.Instance.GetResource(DemoResources.Content.LoggingOptions.Properties.DiscardLogsBelowThisLevel),
        },
        new PropertyItem
        {
            Id = "logging.rotating",
            Name = L.Instance.GetResource(DemoResources.Content.LoggingOptions.Properties.FileRotation),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.LoggingOptions.Properties.RotateFilesAtTheSizeLimitAndRemove),
        },
        new PropertyItem
        {
            Id = "logging.feed",
            Name = L.Instance.GetResource(DemoResources.Content.LoggingOptions.Properties.UIEventStream),
            Kind = PropertyItemKind.Boolean,
            Value = true,
            DefaultValue = true,
            Description = L.Instance.GetResource(DemoResources.Content.LoggingOptions.Properties.EnableTheEventStreamForDesktopLogControls),
        },
    ];

    private static Control BuildOptionsTable()
    {
        var rows = new (string Option, string Default, string Note)[]
        {
            ("MinimumLevel", "Information", L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.DiscardLogsBelowThisLevel)),
            ("EnableConsole", "true", L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.ConsoleOutputOptionalForDesktopApplications)),
            ("File.DirectoryPath", "BaseDirectory/logs", L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.LogDirectory)),
            ("File.MaxFileSizeBytes", "32 MB", L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.PerFileSizeLimitBeforeRotation)),
            ("File.RetentionDays", "30", L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.RetentionDays)),
            ("LineTemplate", "DefaultTemplate", L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.CustomizeTheOutputTemplateWithLineTemplateController)),
            ("LocalizedLevelFormatter", "zh / en", L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.LocalizedLevelNames)),
        };

        return Ui.Table([L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.Parameter), L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.Default), L.Instance.GetResource(DemoResources.Content.LoggingOptions.BuildOptionsTable.Description)], rows.Select(r => new[] { r.Option, r.Default, r.Note }).ToArray());
    }

    private void ShowHealth()
    {
        var snapshot = Logger.Health.GetSnapshot();
        var byLevel = snapshot.DroppedByLevel.Count == 0
            ? L.Instance.GetResource(DemoResources.Content.LoggingOptions.ShowHealth.NoneDropped)
            : string.Join(Environment.NewLine, snapshot.DroppedByLevel.Select(p => $"  {p.Key}: {p.Value}"));

        _output.Text = $"Logger.Health.GetSnapshot()" + Environment.NewLine
            + $"  DroppedCount = {snapshot.DroppedCount}" + Environment.NewLine
            + "  DroppedByLevel:" + Environment.NewLine + byLevel;
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingOptions.ShowHealth.LogHealthEntriesDroppedFormat), snapshot.DroppedCount));
    }

    private void ShowFeed()
    {
        var recent = Logger.Events.GetRecentEvents();
        _output.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingOptions.ShowFeed.LoggerEventsGetRecentEventsEntriesFormat), recent.Count) + Environment.NewLine
            + string.Join(Environment.NewLine, recent.Take(8).Select(e => $"  [{e.Level}] {e.Message}"));
        Logger.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture, L.Instance.GetResource(DemoResources.Content.LoggingOptions.ShowFeed.UIEventSnapshotEntriesFormat), recent.Count));
    }

    private void WriteSamples()
    {
        Logger.Info(L.Instance.GetResource(DemoResources.Content.LoggingOptions.WriteSamples.DemoInformationLog));
        Logger.Warn(L.Instance.GetResource(DemoResources.Content.LoggingOptions.WriteSamples.DemoWarningLog));
        Logger.Error(L.Instance.GetResource(DemoResources.Content.LoggingOptions.WriteSamples.DemoErrorLog), new InvalidOperationException(L.Instance.GetResource(DemoResources.Content.LoggingOptions.WriteSamples.SampleException)));
        ShowFeed();
    }

    private static string I18n(string key) => CodeWF.Avalonia.Lang.I18nManager.Instance.GetResource(key);
}
