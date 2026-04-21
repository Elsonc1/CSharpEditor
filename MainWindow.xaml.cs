using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using CSharpEditor.Compiler;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;

namespace CSharpEditor;

public partial class MainWindow : Window
{
    private string? _currentFilePath;

    public MainWindow()
    {
        InitializeComponent();
        LoadSyntaxHighlighting();
        SetDefaultCode();
        RegisterKeyBindings();
    }

    private void RegisterKeyBindings()
    {
        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("LexicalAnalysis", typeof(MainWindow),
                new InputGestureCollection { new KeyGesture(Key.F5) }),
            (_, _) => BtnLexical_Click(this, new RoutedEventArgs())));

        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("SemanticAnalysis", typeof(MainWindow),
                new InputGestureCollection { new KeyGesture(Key.F6) }),
            (_, _) => BtnSemantic_Click(this, new RoutedEventArgs())));

        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("Save", typeof(MainWindow),
                new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) }),
            (_, _) => BtnSave_Click(this, new RoutedEventArgs())));

        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("Open", typeof(MainWindow),
                new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) }),
            (_, _) => BtnOpen_Click(this, new RoutedEventArgs())));

        CommandBindings.Add(new CommandBinding(
            new RoutedCommand("New", typeof(MainWindow),
                new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) }),
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
        CodeEditor.Text =
@"// Exemplo de código da linguagem
import utils;

public class Animal {
    private string nome;
    private int idade;

    public void main() {
        string saudacao = ""Olá, mundo!"";
        int x = 10;
        double y = 3.14;
        boolean ativo = true;

        if (x > 5) {
            x = x * 2;
        } else {
            x -= 1;
        }

        /* Loop for com incremento */
        for (int i = 0; i < 10; i++) {
            x = x + i;
        }

        int contador = 0;
        while (contador < 3) {
            contador++;
        }

        switch (x) {
            case 1:
                y = 1.0;
            break;
            case 2:
                y = 2.0;
            break;
        }

        var resultado = new Animal();
    }
}";
    }

    // ── File Operations ──

    private void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        CodeEditor.Text = string.Empty;
        _currentFilePath = null;
        ClearOutput();
        TxtStatus.Text = "Novo arquivo";
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Arquivos da Linguagem (*.cl)|*.cl|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            DefaultExt = ".cl"
        };

        if (dialog.ShowDialog() == true)
        {
            CodeEditor.Text = File.ReadAllText(dialog.FileName);
            _currentFilePath = dialog.FileName;
            ClearOutput();
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
            Filter = "Arquivos da Linguagem (*.cl)|*.cl|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            DefaultExt = ".cl",
            FileName = "programa.cl"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, CodeEditor.Text);
            _currentFilePath = dialog.FileName;
            TxtStatus.Text = $"Salvo: {Path.GetFileName(_currentFilePath)}";
        }
    }

    // ── Output Panel ──

    private void ShowTokensTab()
    {
        TokenGrid.Visibility = Visibility.Visible;
        ErrorOutput.Visibility = Visibility.Collapsed;
        TabTokens.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
        TabTokens.FontWeight = FontWeights.SemiBold;
        TabErrors.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
        TabErrors.FontWeight = FontWeights.Normal;
    }

    private void ShowErrorsTab()
    {
        TokenGrid.Visibility = Visibility.Collapsed;
        ErrorOutput.Visibility = Visibility.Visible;
        TabErrors.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
        TabErrors.FontWeight = FontWeights.SemiBold;
        TabTokens.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
        TabTokens.FontWeight = FontWeights.Normal;
    }

    private void ClearOutput()
    {
        TokenGrid.ItemsSource = null;
        ErrorOutput.Clear();
    }

    private void TabTokens_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => ShowTokensTab();
    private void TabErrors_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => ShowErrorsTab();
    private void BtnClearOutput_Click(object sender, RoutedEventArgs e) => ClearOutput();

    // ── Lexical Analysis ──

    private void BtnLexical_Click(object sender, RoutedEventArgs e)
    {
        ClearOutput();
        var source = CodeEditor.Text;

        if (string.IsNullOrWhiteSpace(source))
        {
            TxtStatus.Text = "Editor vazio";
            return;
        }

        var lexer = new Lexer(source);
        var result = lexer.Tokenize();

        var displayTokens = result.Tokens
            .Where(t => t.Type != TokenType.EndOfFile)
            .ToList();

        TokenGrid.ItemsSource = displayTokens;
        ShowTokensTab();

        if (result.HasErrors)
        {
            ErrorOutput.Text = "── Erros Léxicos ──\n\n" +
                               string.Join("\n", result.Errors);
            TxtStatus.Text = $"Análise léxica: {displayTokens.Count} tokens, {result.Errors.Count} erro(s)";
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44747"));
        }
        else
        {
            var legacy = LegacyLexicalFormatter.Format(result);
            ErrorOutput.Text =
                $"Análise léxica concluída com sucesso.\n{displayTokens.Count} tokens encontrados. Nenhum erro léxico.\n\n" +
                "--- Formato legado (referência Java / Unifacvest) ---\n" +
                legacy;
            TxtStatus.Text = $"Análise léxica: {displayTokens.Count} tokens, 0 erros";
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EC9B0"));
        }
    }

    // ── Semantic Analysis ──

    private void BtnSemantic_Click(object sender, RoutedEventArgs e)
    {
        ClearOutput();
        var source = CodeEditor.Text;

        if (string.IsNullOrWhiteSpace(source))
        {
            TxtStatus.Text = "Editor vazio";
            return;
        }

        var lexer = new Lexer(source);
        var lexResult = lexer.Tokenize();

        var displayTokens = lexResult.Tokens
            .Where(t => t.Type != TokenType.EndOfFile)
            .ToList();
        TokenGrid.ItemsSource = displayTokens;

        var errors = new List<string>();

        if (lexResult.HasErrors)
        {
            errors.Add("── Erros Léxicos ──\n");
            errors.AddRange(lexResult.Errors);
            errors.Add("");
        }

        var parser = new Parser(lexResult.Tokens);
        var parseResult = parser.Parse();

        if (parseResult.HasErrors)
        {
            errors.Add("── Erros Sintáticos ──\n");
            errors.AddRange(parseResult.Errors);
            errors.Add("");
        }

        if (parseResult.Program != null)
        {
            var analyzer = new SemanticAnalyzer();
            var semResult = analyzer.Analyze(parseResult.Program);

            if (semResult.HasErrors)
            {
                errors.Add("── Erros Semânticos ──\n");
                errors.AddRange(semResult.Errors);
                errors.Add("");
            }

            if (semResult.Warnings.Count > 0)
            {
                errors.Add("── Avisos ──\n");
                errors.AddRange(semResult.Warnings);
                errors.Add("");
            }
        }

        if (errors.Count > 0)
        {
            ErrorOutput.Text = string.Join("\n", errors);
            ShowErrorsTab();
            int errorCount = errors.Count(line => line.StartsWith("Linha"));
            TxtStatus.Text = $"Análise semântica: {errorCount} erro(s) encontrado(s)";
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44747"));
        }
        else
        {
            ErrorOutput.Text = "Análise semântica concluída com sucesso!\n\n" +
                               $"Tokens: {displayTokens.Count}\n" +
                               "Erros léxicos: 0\n" +
                               "Erros sintáticos: 0\n" +
                               "Erros semânticos: 0\n\n" +
                               "O código está correto!";
            ShowErrorsTab();
            TxtStatus.Text = "Análise semântica: código correto";
            TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EC9B0"));
        }
    }
}
