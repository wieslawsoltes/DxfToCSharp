using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using DxfToCSharp.Core;
using DxfToCSharp.Services;
using netDxf;
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
    private readonly CheckBox? _fileWatchingCheckBox;
    private DxfDocument? _loadedDocument;
    private string? _loadedFilePath;
    private FileSystemWatcher? _fileWatcher;
    private DateTime _lastFileChangeTime = DateTime.MinValue;
    private bool _isProcessingFileChange;

    /// <summary>
    /// Gets a value indicating whether file watching is enabled.
    /// </summary>
    public bool IsFileWatchingEnabled => _fileWatchingCheckBox?.IsChecked == true;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools(); // Commented out to avoid compilation issues
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
            if (_rightTextBox.Document is { LineCount: 0 })
            {
                _rightTextBox.Document.Insert(0, " ");
            }

            // Enable folding for right text editor (C# code)
            _rightFoldingManager = FoldingManager.Install(_rightTextBox.TextArea);
            UpdateRightFolding();
        }

        // Initialize options control
        _optionsControl = this.FindControl<DxfGeneratorOptionsControl>("OptionsControl");

        // Initialize file watching checkbox
        _fileWatchingCheckBox = this.FindControl<CheckBox>("FileWatchingCheckBox");
        if (_fileWatchingCheckBox != null)
        {
            _fileWatchingCheckBox.IsChecked = true; // Default to enabled
            _fileWatchingCheckBox.IsCheckedChanged += OnFileWatchingToggled;
        }

        // Setup cleanup on window close
        Closed += (_, _) => CleanupFileWatcher();

        // Set up event handler for options changes
        if (_optionsControl != null)
        {
            _optionsControl.SetAllOptions(new DxfCodeGenerationOptions());
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
                        if (_rightTextBox is { Document: not null, TextArea: not null })
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
                        catch (InvalidOperationException)
                        {
                            // Ignore cleanup errors - UI element may be disposed
                        }
                        catch (NullReferenceException)
                        {
                            // Ignore cleanup errors - UI element may be null
                        }
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                // Log TextMate initialization errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"TextMate initialization error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                // Log TextMate initialization errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"TextMate initialization error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected TextMate initialization errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Unexpected TextMate initialization error: {ex.Message}");
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnOpenClicked(object? sender, RoutedEventArgs e)
    {
        var storageProvider = StorageProvider;
        if (storageProvider == null)
            return;

        var fileTypeChoices = new[]
        {
            new FilePickerFileType("DXF files")
            {
                Patterns = new[] { "*.dxf" }
            },
            FilePickerFileTypes.All
        };

        var pickerOptions = new FilePickerOpenOptions
        {
            Title = "Open DXF",
            AllowMultiple = false,
            FileTypeFilter = fileTypeChoices
        };

        var result = await storageProvider.OpenFilePickerAsync(pickerOptions);
        var file = result?.FirstOrDefault();
        var path = file?.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        // Load text to left panel
        var text = await ReadAllTextWithSharingAsync(path);
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

            // Setup file watching if enabled
            SetupFileWatcher();

        }
        catch (FileNotFoundException ex)
        {
            ShowError("DXF file not found: " + ex.Message + "\n" + ex.StackTrace);
            SetRightText(""); // Clear generated code editor
            ShowNotification("DXF file not found: " + ex.Message, true);
            _loadedDocument = null;
            _loadedFilePath = null;
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowError("Access denied: " + ex.Message + "\n" + ex.StackTrace);
            SetRightText(""); // Clear generated code editor
            ShowNotification("Access denied to DXF file: " + ex.Message, true);
            _loadedDocument = null;
            _loadedFilePath = null;
        }
        catch (IOException ex)
        {
            ShowError("I/O error: " + ex.Message + "\n" + ex.StackTrace);
            SetRightText(""); // Clear generated code editor
            ShowNotification("I/O error reading DXF file: " + ex.Message, true);
            _loadedDocument = null;
            _loadedFilePath = null;
        }
        catch (ArgumentException ex)
        {
            ShowError("Invalid DXF format: " + ex.Message + "\n" + ex.StackTrace);
            SetRightText(""); // Clear generated code editor
            ShowNotification("Invalid DXF format: " + ex.Message, true);
            _loadedDocument = null;
            _loadedFilePath = null;
        }
        catch (Exception ex)
        {
            ShowError("Unexpected error: " + ex.Message + "\n" + ex.StackTrace);
            SetRightText(""); // Clear generated code editor
            ShowNotification("Unexpected error loading DXF file: " + ex.Message, true);
            _loadedDocument = null;
            _loadedFilePath = null;
        }
    }

    private void OnRunClicked(object? sender, RoutedEventArgs e)
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
            var scripting = new CSharpScriptingService();
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
        catch (InvalidOperationException ex)
        {
            ShowError("Execution error:\n" + ex);
            ShowNotification("Execution error occurred. Check errors tab for details.", isError: true);
        }
        catch (ArgumentException ex)
        {
            ShowError("Argument error:\n" + ex);
            ShowNotification("Argument error occurred. Check errors tab for details.", isError: true);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowError("Access denied:\n" + ex);
            ShowNotification("Access denied. Check errors tab for details.", isError: true);
        }
        catch (Exception ex)
        {
            ShowError("Unexpected execution error:\n" + ex);
            ShowNotification("Unexpected execution error occurred. Check errors tab for details.", isError: true);
        }
    }

    private async void SetRightText(string? text)
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
                        await Task.Delay(10);

                        // Update folding after text is set
                        if (_rightTextBox.Document != null)
                        {
                            UpdateRightFolding();
                        }
                    }
                    catch (InvalidOperationException ex)
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
                    catch (ArgumentException ex)
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
                    catch (Exception ex)
                    {
                        // Fallback: set text without TextMate features if there's an unexpected error
                        System.Diagnostics.Debug.WriteLine($"Unexpected text update error: {ex.Message}");
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
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetRightText error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetRightText error: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected SetRightText error: {ex.Message}");
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
            catch (ArgumentException ex)
            {
                ShowError("Invalid argument regenerating code: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ShowError("Invalid operation regenerating code: " + ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error regenerating code: " + ex.Message);
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

                    // Only set _loadedFilePath to "<from_editor>" if we don't have a real file path
                    // This preserves file watching functionality when a file is loaded via dialog
                    if (string.IsNullOrEmpty(_loadedFilePath) || !File.Exists(_loadedFilePath))
                    {
                        _loadedFilePath = "<from_editor>"; // Indicate content is from editor
                    }
                    // If we have a valid file path, keep it for file watching

                    // Generate code with current options
                    var generator = new DxfCodeGenerator();
                    var options = GetOptionsFromUI();
                    var generatedCode = generator.Generate(doc, _loadedFilePath, null, options);
                    SetRightText(generatedCode);
                    ClearErrors();
                }
            }
            catch (ArgumentException)
            {
                // Don't show errors for partial/invalid DXF content while typing
                // Only clear the generated code if parsing fails
                _loadedDocument = null;
                // Don't clear _loadedFilePath here if it's a valid file path
                if (_loadedFilePath == "<from_editor>" || string.IsNullOrEmpty(_loadedFilePath) || !File.Exists(_loadedFilePath))
                {
                    _loadedFilePath = null;
                }
            }
            catch (InvalidOperationException)
            {
                // Don't show errors for partial/invalid DXF content while typing
                // Only clear the generated code if parsing fails
                _loadedDocument = null;
                // Don't clear _loadedFilePath here if it's a valid file path
                if (_loadedFilePath == "<from_editor>" || string.IsNullOrEmpty(_loadedFilePath) || !File.Exists(_loadedFilePath))
                {
                    _loadedFilePath = null;
                }
            }
            catch (Exception)
            {
                // Don't show errors for partial/invalid DXF content while typing
                // Only clear the generated code if parsing fails
                _loadedDocument = null;
                // Don't clear _loadedFilePath here if it's a valid file path
                if (_loadedFilePath == "<from_editor>" || string.IsNullOrEmpty(_loadedFilePath) || !File.Exists(_loadedFilePath))
                {
                    _loadedFilePath = null;
                }
            }
        }
        else
        {
            // Clear generated code if text editor is empty
            _loadedDocument = null;
            // Don't clear _loadedFilePath here if it's a valid file path
            if (_loadedFilePath == "<from_editor>" || string.IsNullOrEmpty(_loadedFilePath) || !File.Exists(_loadedFilePath))
            {
                _loadedFilePath = null;
            }
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
                if (_leftTextBox.Document is { TextLength: > 0 })
                {
                    var foldingStrategy = new DxfFoldingStrategy();
                    var newFoldings = foldingStrategy.CreateNewFoldings(_leftTextBox.Document, out var firstErrorOffset);
                    _leftFoldingManager.UpdateFoldings(newFoldings, firstErrorOffset);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Folding update error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                // Log folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Folding update error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Unexpected folding update error: {ex.Message}");
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
                if (_rightTextBox.Document is { TextLength: > 0 })
                {
                    var foldingStrategy = new CSharpFoldingStrategy();
                    var newFoldings = foldingStrategy.CreateNewFoldings(_rightTextBox.Document, out var firstErrorOffset);
                    _rightFoldingManager.UpdateFoldings(newFoldings, firstErrorOffset);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Folding update error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                // Log folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Folding update error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log unexpected folding errors but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Unexpected folding update error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles file watching checkbox toggle events.
    /// </summary>
    private void OnFileWatchingToggled(object? sender, RoutedEventArgs e)
    {
        // Setup or cleanup file watcher based on checkbox state
        if (IsFileWatchingEnabled)
        {
            SetupFileWatcher();
        }
        else
        {
            CleanupFileWatcher();
        }
    }

    /// <summary>
    /// Sets up file watching for the currently loaded DXF file.
    /// </summary>
    private void SetupFileWatcher()
    {
        // Clean up existing watcher
        CleanupFileWatcher();

        // Only setup if file watching is enabled and we have a loaded file
        if (!IsFileWatchingEnabled || string.IsNullOrEmpty(_loadedFilePath) || !File.Exists(_loadedFilePath))
            return;

        try
        {
            var directory = Path.GetDirectoryName(_loadedFilePath);
            var fileName = Path.GetFileName(_loadedFilePath);

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                return;

            _fileWatcher = new FileSystemWatcher(directory)
            {
                Filter = "*.*",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName,
                EnableRaisingEvents = false,
                IncludeSubdirectories = false,
                InternalBufferSize = 8192 * 4
            };

            // Subscribe to events before enabling
            _fileWatcher.Created += OnFileChanged;
            _fileWatcher.Renamed += OnFileRenamed;
            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Error += OnFileWatcherError;

            // Enable events after all setup is complete
            _fileWatcher.EnableRaisingEvents = true;
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowError($"Failed to setup file watching: {ex.Message}");
            ShowNotification($"File watching setup failed: {ex.Message}", true);
        }
        catch (ArgumentException ex)
        {
            ShowError($"Failed to setup file watching: {ex.Message}");
            ShowNotification($"File watching setup failed: {ex.Message}", true);
        }
        catch (IOException ex)
        {
            ShowError($"Failed to setup file watching: {ex.Message}");
            ShowNotification($"File watching setup failed: {ex.Message}", true);
        }
        catch (Exception ex)
        {
            ShowError($"Unexpected error setting up file watching: {ex.Message}");
            ShowNotification($"File watching setup failed: {ex.Message}", true);
        }
    }

    /// <summary>
    /// Cleans up the file watcher.
    /// </summary>
    private void CleanupFileWatcher()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Changed -= OnFileChanged;
            _fileWatcher.Created -= OnFileChanged;
            _fileWatcher.Renamed -= OnFileRenamed;
            _fileWatcher.Error -= OnFileWatcherError;
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }
    }

    /// <summary>
    /// Handles file rename events (for editors that use atomic saves).
    /// </summary>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // Check if the renamed file matches our target file
        if (string.Equals(e.FullPath, _loadedFilePath, StringComparison.OrdinalIgnoreCase))
        {

            OnFileChanged(sender, e);
        }
    }

    /// <summary>
    /// Handles file change events.
    /// </summary>
    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Check if this event is for our target file (handle both exact path and filename matching)
        var isTargetFile = string.Equals(e.FullPath, _loadedFilePath, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(e.Name, Path.GetFileName(_loadedFilePath), StringComparison.OrdinalIgnoreCase);

        if (!isTargetFile)
        {

            return;
        }

        // Prevent multiple simultaneous file change processing
        if (_isProcessingFileChange)
        {

            return;
        }

        // Debounce file changes (some editors trigger multiple events)
        var now = DateTime.Now;
        if ((now - _lastFileChangeTime).TotalMilliseconds < 300) // Reduced debounce time
        {

            return;
        }

        _lastFileChangeTime = now;
        _isProcessingFileChange = true;

        try
        {
            await Task.Delay(200);

            if (!File.Exists(_loadedFilePath))
            {
                return;
            }

            var text = await ReadAllTextWithSharingAsync(_loadedFilePath);
            if (text == null)
            {
                return;
            }

            // Reload the file on the UI thread
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    // Load updated text to left panel
                    if (_leftTextBox != null)
                    {
                        _leftTextBox.Text = text;
                        UpdateLeftFolding();
                    }

                    // Parse with netDxf and regenerate C# code
                    var doc = DxfDocument.Load(_loadedFilePath);
                    if (doc != null)
                    {
                        _loadedDocument = doc;
                        var generator = new DxfCodeGenerator();
                        var options = GetOptionsFromUI();
                        var generatedCode = generator.Generate(doc, _loadedFilePath, null, options);
                        SetRightText(generatedCode);
                        ClearErrors();
                        ShowNotification("File reloaded and code regenerated");
                    }
                    else
                    {
                        ShowError("Failed to reload DXF document after file change.");
                        ShowNotification("Failed to reload DXF file", true);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ShowError($"Error reloading file: {ex.Message}");
                    ShowNotification($"Error reloading file: {ex.Message}", true);
                }
                catch (ArgumentException ex)
                {
                    ShowError($"Error reloading file: {ex.Message}");
                    ShowNotification($"Error reloading file: {ex.Message}", true);
                }
                catch (FileNotFoundException ex)
                {
                    ShowError($"Error reloading file: {ex.Message}");
                    ShowNotification($"Error reloading file: {ex.Message}", true);
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowError($"Error reloading file: {ex.Message}");
                    ShowNotification($"Error reloading file: {ex.Message}", true);
                }
                catch (IOException ex)
                {
                    ShowError($"Error reloading file: {ex.Message}");
                    ShowNotification($"Error reloading file: {ex.Message}", true);
                }
                catch (Exception ex)
                {
                    ShowError($"Unexpected error reloading file: {ex.Message}");
                    ShowNotification($"Error reloading file: {ex.Message}", true);
                }

                return Task.CompletedTask;
            });
        }
        catch (InvalidOperationException ex)
        {
            ShowNotification($"File watching error: {ex.Message}", true);
        }
        catch (ArgumentException ex)
        {
            ShowNotification($"File watching error: {ex.Message}", true);
        }
        catch (FileNotFoundException ex)
        {
            ShowNotification($"File watching error: {ex.Message}", true);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowNotification($"File watching error: {ex.Message}", true);
        }
        catch (IOException ex)
        {
            ShowNotification($"File watching error: {ex.Message}", true);
        }
        catch (Exception ex)
        {
            ShowNotification($"Unexpected file watching error: {ex.Message}", true);
        }
        finally
        {
            _isProcessingFileChange = false;
        }
    }

    /// <summary>
    /// Handles FileSystemWatcher error events.
    /// </summary>
    private void OnFileWatcherError(object sender, ErrorEventArgs e)
    {
        ShowNotification($"File watching error: {e.GetException().Message}", true);

        // Try to restart the file watcher
        try
        {
            CleanupFileWatcher();
            SetupFileWatcher();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    /// File.ReadAllTextAsync alternative that can read files locked by other applications.
    /// </summary>
    /// <param name="filePath">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
    private static async Task<string?> ReadAllTextWithSharingAsync(string filePath)
    {
        await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
        using var sr = new StreamReader(fs);
        return await sr.ReadToEndAsync();
    }
}
