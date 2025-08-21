using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using netDxf;
using netDxf.Entities;
using DxfToCSharp.Core;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace DxfToCSharp;

public partial class MainWindow : Window
{
    private readonly TextEditor? _leftTextBox;
    private readonly TextEditor? _rightTextBox;
    private readonly FoldingManager? _leftFoldingManager;
    private readonly FoldingManager? _rightFoldingManager;
    private readonly TextBox? _errorsTextBox;
    private readonly TabControl? _rightTabControl;
    private readonly WindowNotificationManager? _notificationManager;
    private readonly DxfGeneratorOptionsControl? _optionsControl;
    private DxfDocument? _loadedDocument;
    private string? _loadedFilePath;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        _leftTextBox = this.FindControl<TextEditor>("LeftTextBox");
        _rightTextBox = this.FindControl<TextEditor>("RightTextBox");
        _errorsTextBox = this.FindControl<TextBox>("ErrorsTextBox");
        this.FindControl<TabControl>("LeftTabControl");
        _rightTabControl = this.FindControl<TabControl>("RightTabControl");
            
        // Initialize notification manager
        _notificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3
        };
            
        // Configure text editors to prevent TextMate exceptions
        if (_leftTextBox != null)
        {
            _leftTextBox.SyntaxHighlighting = null;
            _leftTextBox.Options.EnableHyperlinks = false;
            _leftTextBox.Options.EnableEmailHyperlinks = false;
                
            // Enable folding for left text editor (DXF content)
            _leftFoldingManager = FoldingManager.Install(_leftTextBox.TextArea);
            UpdateLeftFolding();
        }
            
        if (_rightTextBox != null)
        {
            _rightTextBox.SyntaxHighlighting = null;
            _rightTextBox.Options.EnableHyperlinks = false;
            _rightTextBox.Options.EnableEmailHyperlinks = false;
                
            // Ensure document is initialized with at least one line to prevent TextMate issues
            if (_rightTextBox.Document != null && _rightTextBox.Document.LineCount == 0)
            {
                _rightTextBox.Document.Insert(0, " ");
            }
                
            // Enable folding for right text editor (C# code)
            _rightFoldingManager = FoldingManager.Install(_rightTextBox.TextArea);
            UpdateRightFolding();
        }
            
        // Initialize options control
        _optionsControl = this.FindControl<DxfGeneratorOptionsControl>("OptionsControl");
        
        // Set up event handler for options changes
        if (_optionsControl != null)
        {
            _optionsControl.OptionsChanged += OnOptionsChanged;
        }
            
        // Set up text editor event handler for DXF content changes
        if (_leftTextBox != null)
        {
            _leftTextBox.TextChanged += OnDxfContentChanged;
            _leftTextBox.TextChanged += (_, _) => UpdateLeftFolding();
        }
            
        // Set up text editor event handler for C# code changes
        if (_rightTextBox != null)
        {
            _rightTextBox.TextChanged += (_, _) => UpdateRightFolding();
        }
            
        InitializeTextMate();
    }
    
    private void OnOptionsChanged(object? sender, OptionsChangedEventArgs e)
    {
        RegenerateCodeIfLoaded();
    }
        
    private async void InitializeTextMate()
    {
        if (_rightTextBox != null)
        {
            try
            {
                // Use async/await pattern for better thread safety
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    try
                    {
                        // Additional null check before TextMate operations
                        if (_rightTextBox?.Document != null && _rightTextBox.TextArea != null)
                        {
                            // Ensure the document has at least one line to prevent TMModel.InvalidateLine exceptions
                            if (_rightTextBox.Document.LineCount == 0)
                            {
                                _rightTextBox.Document.Insert(0, " "); // Insert a space to ensure at least one line
                            }
                                
                            var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
                            var textMateInstallation = _rightTextBox.InstallTextMate(registryOptions);
                            var language = registryOptions.GetLanguageByExtension(".cs");
                            if (language != null)
                            {
                                textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(language.Id));
                            }
                        }
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner TextMate error: {innerEx.Message}");
                        // Disable TextMate on error to prevent further issues
                        try
                        {
                            if (_rightTextBox?.Document != null)
                            {
                                _rightTextBox.SyntaxHighlighting = null;
                            }
                        }
                        catch { /* Ignore cleanup errors */ }
                    }
                });
            }
            catch (Exception ex)
            {
                // Log TextMate initialization errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"TextMate initialization error: {ex.Message}");
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnOpenClicked(object? sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Title = "Open DXF",
            AllowMultiple = false,
            Filters =
            {
                new FileDialogFilter { Name = "DXF files", Extensions = { "dxf" } },
                new FileDialogFilter { Name = "All files", Extensions = { "*" } }
            }
        };
        var result = await ofd.ShowAsync(this);
        var path = result?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return;

        // Load text to left panel
        string text = await System.IO.File.ReadAllTextAsync(path);
        if (_leftTextBox != null)
        {
            _leftTextBox.Text = text;
            UpdateLeftFolding();
        }

        // Parse with netDxf and generate C# code
        try
        {
            var doc = DxfDocument.Load(path);
            if (doc == null)
            {
                ShowError("Failed to load DXF document.\n");
                SetRightText(""); // Clear generated code editor
                ShowNotification("Failed to load DXF document", true);
                _loadedDocument = null;
                _loadedFilePath = null;
                return;
            }

            // Store loaded document and path for auto-regeneration
            _loadedDocument = doc;
            _loadedFilePath = path;

            var generator = new DxfCodeGenerator();
            var options = GetOptionsFromUI();
            var generatedCode = generator.Generate(doc, path, null, options);
            SetRightText(generatedCode);
            ClearErrors();
                
            // Check if there are no entities to warn the user
            var allEntities = doc.Entities.All?.ToList() ?? new List<EntityObject>();
            if (allEntities.Count == 0)
            {
                ShowNotification("Warning: No entities found in DXF file to generate code for", true);
            }
        }
        catch (Exception ex)
        {
            ShowError("Error: " + ex.Message + "\n" + ex.StackTrace);
            SetRightText(""); // Clear generated code editor
            ShowNotification("Failed to load DXF file: " + ex.Message, true);
            _loadedDocument = null;
            _loadedFilePath = null;
        }
    }

    private async void OnRunClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            ClearErrors();
            var code = _rightTextBox?.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(code))
            {
                ShowError("No generated code to compile.");
                return;
            }
            var scripting = new DxfToCSharp.Services.CSharpScriptingService();
            var comp = scripting.Compile(code);
            if (!comp.Success || string.IsNullOrEmpty(comp.AssemblyPath))
            {
                ShowError("Compilation failed:\n" + comp.Output);
                ShowNotification("Compilation failed. Check errors tab for details.", isError: true);
                return;
            }
            var output = scripting.RunCreateMethod(comp.AssemblyPath);
            if (output.StartsWith("Runtime error:") || output.StartsWith("Type '") || output.StartsWith("Static method"))
            {
                ShowError(output);
                ShowNotification("Execution failed. Check errors tab for details.", isError: true);
                return;
            }
            // Show success notification instead of putting result in code control
            if (output.StartsWith("Executed successfully"))
            {
                ShowNotification("DXF file generated successfully!", isError: false);
            }
            else
            {
                ShowNotification(output, isError: false);
            }
        }
        catch (Exception ex)
        {
            ShowError("Execution error:\n" + ex);
            ShowNotification("Execution error occurred. Check errors tab for details.", isError: true);
        }
    }

    private async void SetRightText(string text)
    {
        if (_rightTextBox != null)
        {
            try
            {
                // Use async/await for better thread safety with TextMate
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        // Ensure document exists before any operations
                        if (_rightTextBox.Document == null)
                            return;
                            
                        // Safely clear and set text to prevent TextMate threading issues
                        var textToSet = text ?? "";
                            
                        // If text is empty, ensure we have at least a space to prevent TMModel issues
                        if (string.IsNullOrEmpty(textToSet))
                        {
                            textToSet = " ";
                        }
                            
                        _rightTextBox.Text = textToSet;
                            
                        // Use async delay for better performance
                        await System.Threading.Tasks.Task.Delay(10);
                            
                        // Update folding after text is set
                        if (_rightTextBox.Document != null)
                        {
                            UpdateRightFolding();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fallback: set text without TextMate features if there's an error
                        System.Diagnostics.Debug.WriteLine($"Text update error: {ex.Message}");
                        try
                        {
                            if (_rightTextBox?.Document != null)
                            {
                                var fallbackText = text ?? " "; // Ensure non-empty text
                                _rightTextBox.Text = fallbackText;
                            }
                        }
                        catch (Exception fallbackEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Fallback text setting error: {fallbackEx.Message}");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetRightText error: {ex.Message}");
            }
        }
        if (_rightTabControl != null)
            _rightTabControl.SelectedIndex = 0; // Code tab
    }

    private void ShowError(string message)
    {
        if (_errorsTextBox != null)
            _errorsTextBox.Text = message;
        if (_rightTabControl != null)
            _rightTabControl.SelectedIndex = 1; // Errors tab
    }

    private void ClearErrors()
    {
        if (_errorsTextBox != null)
            _errorsTextBox.Text = string.Empty;
    }

    private void ShowNotification(string message, bool isError = false)
    {
        if (_notificationManager != null)
        {
            var notification = new Notification(
                title: isError ? "Error" : "Success",
                message: message,
                type: isError ? NotificationType.Error : NotificationType.Success,
                expiration: TimeSpan.FromSeconds(5)
            );
                
            _notificationManager.Show(notification);
        }
    }
   
    private void RegenerateCodeIfLoaded()
    {
        if (_loadedDocument != null && !string.IsNullOrEmpty(_loadedFilePath))
        {
            try
            {
                var generator = new DxfCodeGenerator();
                var options = GetOptionsFromUI();
                var generatedCode = generator.Generate(_loadedDocument, _loadedFilePath, null, options);
                SetRightText(generatedCode);
                ClearErrors();
            }
            catch (Exception ex)
            {
                ShowError("Error regenerating code: " + ex.Message);
            }
        }
    }

    private DxfCodeGenerationOptions GetOptionsFromUI()
    {
        return _optionsControl?.GetOptionsFromUI() ?? new DxfCodeGenerationOptions();
    }

    private void OnDxfContentChanged(object? sender, EventArgs e)
    {
        // Parse DXF content from the text editor and regenerate code
        if (_leftTextBox != null && !string.IsNullOrWhiteSpace(_leftTextBox.Text))
        {
            try
            {
                // Parse DXF content from text editor
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_leftTextBox.Text));
                var doc = DxfDocument.Load(stream);
                    
                if (doc != null)
                {
                    // Update stored document for regeneration
                    _loadedDocument = doc;
                    _loadedFilePath = "<from_editor>"; // Indicate content is from editor
                        
                    // Generate code with current options
                    var generator = new DxfCodeGenerator();
                    var options = GetOptionsFromUI();
                    var generatedCode = generator.Generate(doc, _loadedFilePath, null, options);
                    SetRightText(generatedCode);
                    ClearErrors();
                        
                    // Check if there are no entities to warn the user
                    var allEntities = doc.Entities.All?.ToList() ?? new List<EntityObject>();
                    if (allEntities.Count == 0)
                    {
                        ShowNotification("Warning: No entities found in DXF content to generate code for", true);
                    }
                }
            }
            catch (Exception)
            {
                // Don't show errors for partial/invalid DXF content while typing
                // Only clear the generated code if parsing fails
                _loadedDocument = null;
                _loadedFilePath = null;
            }
        }
        else
        {
            // Clear generated code if text editor is empty
            _loadedDocument = null;
            _loadedFilePath = null;
            SetRightText("");
            ClearErrors();
        }
    }

    private void UpdateLeftFolding()
    {
        if (_leftFoldingManager != null && _leftTextBox != null)
        {
            try
            {
                // Ensure document is ready before creating foldings
                if (_leftTextBox.Document != null && _leftTextBox.Document.TextLength > 0)
                {
                    var foldingStrategy = new DxfFoldingStrategy();
                    var newFoldings = foldingStrategy.CreateNewFoldings(_leftTextBox.Document, out int firstErrorOffset);
                    _leftFoldingManager.UpdateFoldings(newFoldings, firstErrorOffset);
                }
            }
            catch (Exception ex)
            {
                // Log folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Folding update error: {ex.Message}");
            }
        }
    }
        
    private void UpdateRightFolding()
    {
        if (_rightFoldingManager != null && _rightTextBox != null)
        {
            try
            {
                // Ensure document is ready before creating foldings
                if (_rightTextBox.Document != null && _rightTextBox.Document.TextLength > 0)
                {
                    var foldingStrategy = new CSharpFoldingStrategy();
                    var newFoldings = foldingStrategy.CreateNewFoldings(_rightTextBox.Document, out int firstErrorOffset);
                    _rightFoldingManager.UpdateFoldings(newFoldings, firstErrorOffset);
                }
            }
            catch (Exception ex)
            {
                // Log folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Folding update error: {ex.Message}");
            }
        }
    }
}
