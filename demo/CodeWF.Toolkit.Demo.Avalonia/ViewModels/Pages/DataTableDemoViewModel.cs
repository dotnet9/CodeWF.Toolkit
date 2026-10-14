using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CodeWF.Avalonia.Lang;
using CodeWF.Toolkit.Files.Exports;
using ReactiveUI;

namespace CodeWF.Toolkit.Demo.Avalonia.ViewModels.Pages;

public sealed class DataTableDemoViewModel : ReactiveObject
{
    private string _input = "Ada,42\nLin,24";
    private string _output = "";
    private bool _busy;
    public DataTableDemoViewModel()
    {
        Source = new FlatTreeDataGridSource<TablePreviewRow>(Rows)
        {
            Columns = { new TextColumn<TablePreviewRow, string>("Name", row => row.Name), new TextColumn<TablePreviewRow, string>("Value", row => row.Value) }
        };
        CsvCommand = ReactiveCommand.CreateFromTask(() => RunAsync(false));
        ExcelCommand = ReactiveCommand.CreateFromTask(() => RunAsync(true));
    }
    public string Input { get => _input; set => this.RaiseAndSetIfChanged(ref _input, value); }
    public string Output { get => _output; private set => this.RaiseAndSetIfChanged(ref _output, value); }
    public bool IsBusy { get => _busy; private set => this.RaiseAndSetIfChanged(ref _busy, value); }
    public bool Succeeded { get; private set; }
    public ObservableCollection<TablePreviewRow> Rows { get; } = [];
    public FlatTreeDataGridSource<TablePreviewRow> Source { get; }
    public ICommand CsvCommand { get; }
    public ICommand ExcelCommand { get; }

    public async Task RunAsync(bool excel)
    {
        if (IsBusy) return;
        IsBusy = true;
        Succeeded = false;
        var input = Input;
        try
        {
            var result = await Task.Run(() => RoundTrip(input, excel));
            Rows.Clear();
            foreach (var row in result.Rows) Rows.Add(row);
            Output = $"{result.Path}\nRows = {Rows.Count}\nBytes = {new FileInfo(result.Path).Length}";
            Succeeded = true;
        }
        catch (Exception ex) { Output = string.Format(I18nManager.Instance.GetResource("Demo.Catalog.Failed"), ex.Message); }
        finally { IsBusy = false; }
    }

    private static (string Path, TablePreviewRow[] Rows) RoundTrip(string input, bool excel)
    {
        using var table = new DataTable();
        table.Columns.Add("Name"); table.Columns.Add("Value");
        foreach (var line in input.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var cells = line.TrimEnd('\r').Split(',');
            if (cells.Length != 2) throw new FormatException(I18nManager.Instance.GetResource("Demo.Catalog.TableFormat"));
            table.Rows.Add(cells[0], cells[1]);
        }
        if (table.Rows.Count == 0) throw new FormatException(I18nManager.Instance.GetResource("Demo.Catalog.TableFormat"));
        var directory = Path.Combine(Path.GetTempPath(), "CodeWFToolkitDemo", "tables", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, excel ? "sample.xlsx" : "sample.csv");
        var exported = excel ? table.ExportToXlsx(path, out var error) : table.ExportToCsv(path, Encoding.UTF8, out error);
        if (!exported) throw new InvalidOperationException(error);
        var imported = excel ? path.ImportFromExcel(out error, out var restored) : path.ImportFromCsv(out error, out restored);
        if (!imported || restored is null) throw new InvalidOperationException(error);
        using (restored)
        {
            var rows = restored.Rows.Cast<DataRow>().Select(row => new TablePreviewRow(row[0].ToString()!, row[1].ToString()!)).ToArray();
            if (rows.Length != table.Rows.Count || rows.Where((row, i) => row.Name != table.Rows[i][0].ToString() || row.Value != table.Rows[i][1].ToString()).Any())
                throw new InvalidDataException("Table round-trip changed cell values.");
            return (path, rows);
        }
    }
}

public sealed record TablePreviewRow(string Name, string Value);
