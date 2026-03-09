using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;

namespace CSharpEditor;

public partial class MainWindow : Window
{
    private string? _currentFilePath;
    private Process? _runningProcess;
    private CancellationTokenSource? _cts;

    private static readonly string TempProjectDir =
        Path.Combine(Path.GetTempPath(), "CSharpEditorTemp");

    public MainWindow()
    {
        InitializeComponent();
        LoadSyntaxHighlighting();
        SetDefaultCode();

        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("Run", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.F5) }),
            (_, _) => BtnRun_Click(this, new RoutedEventArgs())));
        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("Save", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) }),
            (_, _) => BtnSave_Click(this, new RoutedEventArgs())));
        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("Open", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) }),
            (_, _) => BtnOpen_Click(this, new RoutedEventArgs())));
        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("New", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) }),
            (_, _) => BtnNew_Click(this, new RoutedEventArgs())));
    }

    private void LoadSyntaxHighlighting()
    {
        var xshdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSharpSyntax.xshd");
        if (!File.Exists(xshdPath)) return;

        using var reader = new XmlTextReader(xshdPath);
        var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        CodeEditor.SyntaxHighlighting = highlighting;
    }

    private void SetDefaultCode()
    {
        CodeEditor.Text = """
            using System;

            namespace MeuPrograma
            {
                class Program
                {
                    static void Main(string[] args)
                    {
                        Console.WriteLine("Olá, mundo!");
                        Console.WriteLine("Bem-vindo ao C# Code Editor!");
                    }
                }
            }
            """;
    }

    // ── File Operations ──

    private void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        CodeEditor.Text = string.Empty;
        _currentFilePath = null;
        TxtStatus.Text = "Novo arquivo";
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*",
            DefaultExt = ".cs"
        };

        if (dialog.ShowDialog() == true)
        {
            CodeEditor.Text = File.ReadAllText(dialog.FileName);
            _currentFilePath = dialog.FileName;
            TxtStatus.Text = $"Aberto: {Path.GetFileName(_currentFilePath)}";
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFilePath != null)
        {
            File.WriteAllText(_currentFilePath, CodeEditor.Text);
            TxtStatus.Text = $"Salvo: {Path.GetFileName(_currentFilePath)}";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*",
            DefaultExt = ".cs",
            FileName = "Program.cs"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, CodeEditor.Text);
            _currentFilePath = dialog.FileName;
            TxtStatus.Text = $"Salvo: {Path.GetFileName(_currentFilePath)}";
        }
    }

    // ── Terminal ──

    private void AppendTerminal(string text)
    {
        Dispatcher.Invoke(() =>
        {
            TerminalOutput.AppendText(text);
            TerminalOutput.ScrollToEnd();
        });
    }

    private void BtnClearTerminal_Click(object sender, RoutedEventArgs e)
    {
        TerminalOutput.Clear();
    }

    // ── Compile & Run ──

    private async void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        BtnRun.IsEnabled = false;
        BtnStop.IsEnabled = true;
        TerminalOutput.Clear();
        _cts = new CancellationTokenSource();

        try
        {
            TxtStatus.Text = "Preparando projeto...";
            var projectDir = PrepareTemporaryProject();

            TxtStatus.Text = "Compilando...";
            AppendTerminal("── Compilando... ──\n");

            var buildSuccess = await RunProcessAsync(
                "dotnet", $"build \"{projectDir}\" -c Release --nologo",
                _cts.Token);

            if (!buildSuccess)
            {
                TxtStatus.Text = "Erro de compilação";
                AppendTerminal("\n── Compilação falhou ──\n");
                return;
            }

            AppendTerminal("\n── Compilação bem-sucedida ──\n\n");
            TxtStatus.Text = "Executando...";
            AppendTerminal("── Executando... ──\n");

            var outputDir = Path.Combine(projectDir, "bin", "Release", "net8.0");
            var dllPath = Path.Combine(outputDir, "TempProject.dll");

            await RunProcessAsync("dotnet", $"\"{dllPath}\"", _cts.Token);

            AppendTerminal("\n── Execução finalizada ──\n");
            TxtStatus.Text = "Pronto";
        }
        catch (OperationCanceledException)
        {
            AppendTerminal("\n── Execução cancelada ──\n");
            TxtStatus.Text = "Cancelado";
        }
        catch (Exception ex)
        {
            AppendTerminal($"\n── Erro: {ex.Message} ──\n");
            TxtStatus.Text = "Erro";
        }
        finally
        {
            BtnRun.IsEnabled = true;
            BtnStop.IsEnabled = false;
            _runningProcess = null;
            _cts = null;
        }
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _cts?.Cancel();
            if (_runningProcess is { HasExited: false })
            {
                _runningProcess.Kill(entireProcessTree: true);
            }
        }
        catch { }
    }

    private string PrepareTemporaryProject()
    {
        if (Directory.Exists(TempProjectDir))
            Directory.Delete(TempProjectDir, true);

        Directory.CreateDirectory(TempProjectDir);

        var csproj = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net8.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """;

        File.WriteAllText(Path.Combine(TempProjectDir, "TempProject.csproj"), csproj);
        File.WriteAllText(Path.Combine(TempProjectDir, "Program.cs"), CodeEditor.Text);

        return TempProjectDir;
    }

    private async Task<bool> RunProcessAsync(string fileName, string arguments, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = TempProjectDir
        };

        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _runningProcess = process;

        process.Start();

        var readOut = Task.Run(async () =>
        {
            var buffer = new char[256];
            int count;
            while ((count = await process.StandardOutput.ReadAsync(buffer, ct)) > 0)
                AppendTerminal(new string(buffer, 0, count));
        }, ct);

        var readErr = Task.Run(async () =>
        {
            var buffer = new char[256];
            int count;
            while ((count = await process.StandardError.ReadAsync(buffer, ct)) > 0)
                AppendTerminal(new string(buffer, 0, count));
        }, ct);

        await Task.WhenAll(readOut, readErr);
        await process.WaitForExitAsync(ct);

        return process.ExitCode == 0;
    }
}
