using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using netDxf;
using netDxf.Entities;
using DxfToCSharp.Core;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace DxfToCSharp
{
    public partial class MainWindow : Window
    {
        private TextEditor? _leftTextBox;
        private TextEditor? _rightTextBox;
        private FoldingManager? _leftFoldingManager;
        private FoldingManager? _rightFoldingManager;
        private TextBox? _errorsTextBox;
        private TabControl? _leftTabControl;
        private TabControl? _rightTabControl;
        private Border? _notificationBorder;
        private TextBlock? _notificationText;
        
        // Options UI controls
        private CheckBox? _generateHeaderCheckBox;
        private CheckBox? _generateUsingStatementsCheckBox;
        private CheckBox? _generateDetailedCommentsCheckBox;
        private CheckBox? _groupEntitiesByTypeCheckBox;
        private CheckBox? _generateLayersCheckBox;
        private CheckBox? _generateLinetypesCheckBox;
        private CheckBox? _generateTextStylesCheckBox;
        private CheckBox? _generateBlocksCheckBox;
        private CheckBox? _generateDimensionStylesCheckBox;
        private CheckBox? _generateMLineStylesCheckBox;
        private CheckBox? _generateEntitiesCheckBox;
        private CheckBox? _generateLinesCheckBox;
        private CheckBox? _generateArcsCheckBox;
        private CheckBox? _generateCirclesCheckBox;
        private CheckBox? _generateEllipsesCheckBox;
        private CheckBox? _generatePolylines2DCheckBox;
        private CheckBox? _generatePolylines3DCheckBox;
        private CheckBox? _generateLwPolylinesCheckBox;
        private CheckBox? _generateSplinesCheckBox;
        private CheckBox? _generateTextsCheckBox;
        private CheckBox? _generateMTextsCheckBox;
        private CheckBox? _generatePointsCheckBox;
        private CheckBox? _generateInsertCheckBox;
        private CheckBox? _generateHatchesCheckBox;
        private CheckBox? _generateSolidsCheckBox;
        private CheckBox? _generateFacesCheckBox;
    private CheckBox? _generateWipeoutsCheckBox;
    private CheckBox? _generateDimensionsCheckBox;
    private CheckBox? _generateLeadersCheckBox;
    private CheckBox? _generateMlinesCheckBox;
    private CheckBox? _generateRaysCheckBox;
    private CheckBox? _generateXlinesCheckBox;
        private CheckBox? _generateSaveCommentCheckBox;
    private CheckBox? _generateReturnStatementCheckBox;
    
    // New ComboBox for presets
    private ComboBox? _presetComboBox;
    
    // Track loaded DXF document and path for auto-regeneration
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
            _leftTabControl = this.FindControl<TabControl>("LeftTabControl");
            _rightTabControl = this.FindControl<TabControl>("RightTabControl");
            _notificationBorder = this.FindControl<Border>("NotificationBorder");
            _notificationText = this.FindControl<TextBlock>("NotificationText");
            
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
                
                // Enable folding for right text editor (C# code)
                _rightFoldingManager = FoldingManager.Install(_rightTextBox.TextArea);
                UpdateRightFolding();
            }
            
            // Initialize options UI controls
            _generateHeaderCheckBox = this.FindControl<CheckBox>("GenerateHeaderCheckBox");
            _generateUsingStatementsCheckBox = this.FindControl<CheckBox>("GenerateUsingStatementsCheckBox");
            _generateDetailedCommentsCheckBox = this.FindControl<CheckBox>("GenerateDetailedCommentsCheckBox");
            _groupEntitiesByTypeCheckBox = this.FindControl<CheckBox>("GroupEntitiesByTypeCheckBox");
            _generateLayersCheckBox = this.FindControl<CheckBox>("GenerateLayersCheckBox");
            _generateLinetypesCheckBox = this.FindControl<CheckBox>("GenerateLinetypesCheckBox");
            _generateTextStylesCheckBox = this.FindControl<CheckBox>("GenerateTextStylesCheckBox");
            _generateBlocksCheckBox = this.FindControl<CheckBox>("GenerateBlocksCheckBox");
            _generateDimensionStylesCheckBox = this.FindControl<CheckBox>("GenerateDimensionStylesCheckBox");
            _generateMLineStylesCheckBox = this.FindControl<CheckBox>("GenerateMLineStylesCheckBox");
            _generateEntitiesCheckBox = this.FindControl<CheckBox>("GenerateEntitiesCheckBox");
            _generateLinesCheckBox = this.FindControl<CheckBox>("GenerateLineEntitiesCheckBox");
            _generateArcsCheckBox = this.FindControl<CheckBox>("GenerateArcEntitiesCheckBox");
            _generateCirclesCheckBox = this.FindControl<CheckBox>("GenerateCircleEntitiesCheckBox");
            _generateEllipsesCheckBox = this.FindControl<CheckBox>("GenerateEllipseEntitiesCheckBox");
            _generatePolylines2DCheckBox = this.FindControl<CheckBox>("GeneratePolylineEntitiesCheckBox");
            _generatePolylines3DCheckBox = this.FindControl<CheckBox>("GeneratePolylineEntitiesCheckBox");
            _generateLwPolylinesCheckBox = this.FindControl<CheckBox>("GeneratePolylineEntitiesCheckBox");
            _generateSplinesCheckBox = this.FindControl<CheckBox>("GenerateSplineEntitiesCheckBox");
            _generateTextsCheckBox = this.FindControl<CheckBox>("GenerateTextEntitiesCheckBox");
            _generateMTextsCheckBox = this.FindControl<CheckBox>("GenerateMTextEntitiesCheckBox");
            _generatePointsCheckBox = this.FindControl<CheckBox>("GeneratePointEntitiesCheckBox");
            _generateInsertCheckBox = this.FindControl<CheckBox>("GenerateInsertEntitiesCheckBox");
            _generateHatchesCheckBox = null; // No corresponding checkbox in XAML
            _generateSolidsCheckBox = this.FindControl<CheckBox>("GenerateSolidEntitiesCheckBox");
            _generateFacesCheckBox = this.FindControl<CheckBox>("GenerateFace3dEntitiesCheckBox");
            _generateWipeoutsCheckBox = this.FindControl<CheckBox>("GenerateWipeoutEntitiesCheckBox");
            _generateDimensionsCheckBox = this.FindControl<CheckBox>("GenerateDimensionEntitiesCheckBox");
            _generateLeadersCheckBox = this.FindControl<CheckBox>("GenerateLeaderEntitiesCheckBox");
            _generateMlinesCheckBox = this.FindControl<CheckBox>("GenerateMLineEntitiesCheckBox");
            _generateRaysCheckBox = this.FindControl<CheckBox>("GenerateRayEntitiesCheckBox");
            _generateXlinesCheckBox = this.FindControl<CheckBox>("GenerateXLineEntitiesCheckBox");
            _generateSaveCommentCheckBox = this.FindControl<CheckBox>("GenerateSaveCommentCheckBox");
            _generateReturnStatementCheckBox = this.FindControl<CheckBox>("GenerateReturnStatementCheckBox");
            
            // Initialize preset ComboBox
            _presetComboBox = this.FindControl<ComboBox>("PresetComboBox");
            
            // Set up event handler for master entities checkbox
            if (_generateEntitiesCheckBox != null)
            {
                _generateEntitiesCheckBox.Checked += OnEntitiesCheckBoxChanged;
                _generateEntitiesCheckBox.Unchecked += OnEntitiesCheckBoxChanged;
            }
            
            // Set up event handlers for all option checkboxes to trigger auto-regeneration
            SetupOptionEventHandlers();
            
            // Set up text editor event handler for DXF content changes
            if (_leftTextBox != null)
            {
                _leftTextBox.TextChanged += OnDxfContentChanged;
                _leftTextBox.TextChanged += (s, e) => UpdateLeftFolding();
            }
            
            // Set up text editor event handler for C# code changes
            if (_rightTextBox != null)
            {
                _rightTextBox.TextChanged += (s, e) => UpdateRightFolding();
            }
            
            InitializeTextMate();
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
                            if (_rightTextBox?.Document != null)
                            {
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



        private void SetLeftText(string text)
        {
            if (_leftTextBox != null)
                _leftTextBox.Text = text;
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
                            
                            // Clear text first to prevent TextMate threading issues
                            _rightTextBox.Text = "";
                            
                            // Use async delay instead of Thread.Sleep for better performance
                            await System.Threading.Tasks.Task.Delay(20);
                            
                            // Double-check document still exists after delay
                            if (_rightTextBox.Document != null)
                            {
                                _rightTextBox.Text = text ?? "";
                                
                                // Update folding after text is set
                                UpdateRightFolding();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Fallback: set text without TextMate features if there's an error
                            System.Diagnostics.Debug.WriteLine($"Text update error: {ex.Message}");
                            if (_rightTextBox.Document != null)
                            {
                                _rightTextBox.Text = text ?? "";
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
            if (_notificationText != null && _notificationBorder != null)
            {
                _notificationText.Text = message;
                _notificationBorder.Background = isError ? 
                    Avalonia.Media.Brushes.Red : 
                    Avalonia.Media.Brushes.Green;
                _notificationBorder.IsVisible = true;
                
                // Auto-hide after 5 seconds
                Task.Delay(5000).ContinueWith(_ => 
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => 
                    {
                        if (_notificationBorder != null)
                            _notificationBorder.IsVisible = false;
                    });
                });
            }
        }

        private void OnCloseNotificationClicked(object? sender, RoutedEventArgs e)
        {
            if (_notificationBorder != null)
                _notificationBorder.IsVisible = false;
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
            return new DxfCodeGenerationOptions
            {
                GenerateHeader = _generateHeaderCheckBox?.IsChecked ?? true,
                GenerateUsingStatements = _generateUsingStatementsCheckBox?.IsChecked ?? true,
                GenerateDetailedComments = _generateDetailedCommentsCheckBox?.IsChecked ?? false,
                GroupEntitiesByType = _groupEntitiesByTypeCheckBox?.IsChecked ?? false,
                GenerateLayers = _generateLayersCheckBox?.IsChecked ?? true,
                GenerateLinetypes = _generateLinetypesCheckBox?.IsChecked ?? true,
                GenerateTextStyles = _generateTextStylesCheckBox?.IsChecked ?? true,
                GenerateBlocks = _generateBlocksCheckBox?.IsChecked ?? true,
                GenerateDimensionStyles = _generateDimensionStylesCheckBox?.IsChecked ?? true,
                GenerateMLineStyles = _generateMLineStylesCheckBox?.IsChecked ?? true,
                GenerateEntities = _generateEntitiesCheckBox?.IsChecked ?? true,
                GenerateLineEntities = _generateLinesCheckBox?.IsChecked ?? true,
                GenerateArcEntities = _generateArcsCheckBox?.IsChecked ?? true,
                GenerateCircleEntities = _generateCirclesCheckBox?.IsChecked ?? true,
                GenerateEllipseEntities = _generateEllipsesCheckBox?.IsChecked ?? true,
                GeneratePolylineEntities = _generatePolylines2DCheckBox?.IsChecked ?? true,
                GenerateSplineEntities = _generateSplinesCheckBox?.IsChecked ?? true,
                GenerateTextEntities = _generateTextsCheckBox?.IsChecked ?? true,
                GenerateMTextEntities = _generateMTextsCheckBox?.IsChecked ?? true,
                GeneratePointEntities = _generatePointsCheckBox?.IsChecked ?? true,
                GenerateInsertEntities = _generateInsertCheckBox?.IsChecked ?? true,
                GenerateSolidEntities = _generateSolidsCheckBox?.IsChecked ?? true,
                GenerateFace3dEntities = _generateFacesCheckBox?.IsChecked ?? true,
                GenerateWipeoutEntities = _generateWipeoutsCheckBox?.IsChecked ?? true,
                GenerateDimensionEntities = _generateDimensionsCheckBox?.IsChecked ?? true,
                GenerateLeaderEntities = _generateLeadersCheckBox?.IsChecked ?? true,
                GenerateMLineEntities = _generateMlinesCheckBox?.IsChecked ?? true,
                GenerateRayEntities = _generateRaysCheckBox?.IsChecked ?? true,
                GenerateXLineEntities = _generateXlinesCheckBox?.IsChecked ?? true,
                GenerateSaveComment = _generateSaveCommentCheckBox?.IsChecked ?? true,
                GenerateReturnStatement = _generateReturnStatementCheckBox?.IsChecked ?? true
            };
        }

        private void OnPresetAllClicked(object? sender, RoutedEventArgs e)
        {
            SetAllOptions(DxfCodeGenerationOptions.CreateDefault());
        }

        private void OnPresetEntitiesOnlyClicked(object? sender, RoutedEventArgs e)
        {
            SetAllOptions(DxfCodeGenerationOptions.CreateEntitiesOnly());
        }

        private void OnPresetBasicGeometryClicked(object? sender, RoutedEventArgs e)
        {
            SetAllOptions(DxfCodeGenerationOptions.CreateBasicGeometryOnly());
        }

        private void OnPresetResetClicked(object? sender, RoutedEventArgs e)
        {
            SetAllOptions(DxfCodeGenerationOptions.CreateDefault());
            if (_presetComboBox != null)
                _presetComboBox.SelectedIndex = 4; // Custom
        }
        
        private void OnPresetSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_presetComboBox?.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string tag)
            {
                switch (tag)
                {
                    case "all":
                        SetAllOptions(DxfCodeGenerationOptions.CreateDefault());
                        RegenerateCodeIfLoaded();
                        break;
                    case "entities":
                        SetAllOptions(DxfCodeGenerationOptions.CreateEntitiesOnly());
                        RegenerateCodeIfLoaded();
                        break;
                    case "basic":
                        SetAllOptions(DxfCodeGenerationOptions.CreateBasicGeometryOnly());
                        RegenerateCodeIfLoaded();
                        break;
                    case "tables":
                        SetAllOptions(CreateTablesOnlyOptions());
                        RegenerateCodeIfLoaded();
                        break;
                    case "custom":
                        // Don't change anything for custom
                        break;
                }
            }
        }
        
        private DxfCodeGenerationOptions CreateTablesOnlyOptions()
        {
            var options = new DxfCodeGenerationOptions
            {
                GenerateHeader = true,
                GenerateUsingStatements = true,
                GenerateDetailedComments = false,
                GroupEntitiesByType = false,
                GenerateLayers = true,
                GenerateLinetypes = true,
                GenerateTextStyles = true,
                GenerateBlocks = true,
                GenerateDimensionStyles = true,
                GenerateMLineStyles = true,
                GenerateEntities = false,
                GenerateSaveComment = true,
                GenerateReturnStatement = true
            };
            return options;
        }

        private void SetAllOptions(DxfCodeGenerationOptions options)
        {
            if (_generateHeaderCheckBox != null) _generateHeaderCheckBox.IsChecked = options.GenerateHeader;
            if (_generateUsingStatementsCheckBox != null) _generateUsingStatementsCheckBox.IsChecked = options.GenerateUsingStatements;
            if (_generateDetailedCommentsCheckBox != null) _generateDetailedCommentsCheckBox.IsChecked = options.GenerateDetailedComments;
            if (_groupEntitiesByTypeCheckBox != null) _groupEntitiesByTypeCheckBox.IsChecked = options.GroupEntitiesByType;
            if (_generateLayersCheckBox != null) _generateLayersCheckBox.IsChecked = options.GenerateLayers;
            if (_generateLinetypesCheckBox != null) _generateLinetypesCheckBox.IsChecked = options.GenerateLinetypes;
            if (_generateTextStylesCheckBox != null) _generateTextStylesCheckBox.IsChecked = options.GenerateTextStyles;
            if (_generateBlocksCheckBox != null) _generateBlocksCheckBox.IsChecked = options.GenerateBlocks;
            if (_generateDimensionStylesCheckBox != null) _generateDimensionStylesCheckBox.IsChecked = options.GenerateDimensionStyles;
            if (_generateMLineStylesCheckBox != null) _generateMLineStylesCheckBox.IsChecked = options.GenerateMLineStyles;
            if (_generateEntitiesCheckBox != null) _generateEntitiesCheckBox.IsChecked = options.GenerateEntities;
            if (_generateLinesCheckBox != null) _generateLinesCheckBox.IsChecked = options.GenerateLineEntities;
            if (_generateArcsCheckBox != null) _generateArcsCheckBox.IsChecked = options.GenerateArcEntities;
            if (_generateCirclesCheckBox != null) _generateCirclesCheckBox.IsChecked = options.GenerateCircleEntities;
            if (_generateEllipsesCheckBox != null) _generateEllipsesCheckBox.IsChecked = options.GenerateEllipseEntities;
            if (_generatePolylines2DCheckBox != null) _generatePolylines2DCheckBox.IsChecked = options.GeneratePolylineEntities;
            if (_generatePolylines3DCheckBox != null) _generatePolylines3DCheckBox.IsChecked = options.GeneratePolylineEntities;
            if (_generateLwPolylinesCheckBox != null) _generateLwPolylinesCheckBox.IsChecked = options.GeneratePolylineEntities;
            if (_generateSplinesCheckBox != null) _generateSplinesCheckBox.IsChecked = options.GenerateSplineEntities;
            if (_generateTextsCheckBox != null) _generateTextsCheckBox.IsChecked = options.GenerateTextEntities;
            if (_generateMTextsCheckBox != null) _generateMTextsCheckBox.IsChecked = options.GenerateMTextEntities;
            if (_generatePointsCheckBox != null) _generatePointsCheckBox.IsChecked = options.GeneratePointEntities;
            if (_generateInsertCheckBox != null) _generateInsertCheckBox.IsChecked = options.GenerateInsertEntities;
            if (_generateHatchesCheckBox != null) _generateHatchesCheckBox.IsChecked = options.GenerateEntities;
            if (_generateSolidsCheckBox != null) _generateSolidsCheckBox.IsChecked = options.GenerateSolidEntities;
            if (_generateFacesCheckBox != null) _generateFacesCheckBox.IsChecked = options.GenerateFace3dEntities;
            if (_generateWipeoutsCheckBox != null) _generateWipeoutsCheckBox.IsChecked = options.GenerateWipeoutEntities;
            if (_generateDimensionsCheckBox != null) _generateDimensionsCheckBox.IsChecked = options.GenerateDimensionEntities;
            if (_generateLeadersCheckBox != null) _generateLeadersCheckBox.IsChecked = options.GenerateLeaderEntities;
            if (_generateMlinesCheckBox != null) _generateMlinesCheckBox.IsChecked = options.GenerateMLineEntities;
            if (_generateRaysCheckBox != null) _generateRaysCheckBox.IsChecked = options.GenerateRayEntities;
            if (_generateXlinesCheckBox != null) _generateXlinesCheckBox.IsChecked = options.GenerateXLineEntities;
            if (_generateSaveCommentCheckBox != null) _generateSaveCommentCheckBox.IsChecked = options.GenerateSaveComment;
            if (_generateReturnStatementCheckBox != null) _generateReturnStatementCheckBox.IsChecked = options.GenerateReturnStatement;
        }

        private void OnEntitiesCheckBoxChanged(object? sender, RoutedEventArgs e)
        {
            if (_generateEntitiesCheckBox?.IsChecked == true)
            {
                // Enable all entity checkboxes when master is checked
                if (_generateLinesCheckBox != null) _generateLinesCheckBox.IsChecked = true;
                if (_generateArcsCheckBox != null) _generateArcsCheckBox.IsChecked = true;
                if (_generateCirclesCheckBox != null) _generateCirclesCheckBox.IsChecked = true;
                if (_generateEllipsesCheckBox != null) _generateEllipsesCheckBox.IsChecked = true;
                if (_generatePolylines2DCheckBox != null) _generatePolylines2DCheckBox.IsChecked = true;
                if (_generatePolylines3DCheckBox != null) _generatePolylines3DCheckBox.IsChecked = true;
            if (_generateLwPolylinesCheckBox != null) _generateLwPolylinesCheckBox.IsChecked = true;
            if (_generateSplinesCheckBox != null) _generateSplinesCheckBox.IsChecked = true;
            if (_generateTextsCheckBox != null) _generateTextsCheckBox.IsChecked = true;
            if (_generateMTextsCheckBox != null) _generateMTextsCheckBox.IsChecked = true;
            if (_generatePointsCheckBox != null) _generatePointsCheckBox.IsChecked = true;
            if (_generateInsertCheckBox != null) _generateInsertCheckBox.IsChecked = true;
            if (_generateHatchesCheckBox != null) _generateHatchesCheckBox.IsChecked = true;
            if (_generateSolidsCheckBox != null) _generateSolidsCheckBox.IsChecked = true;
            if (_generateFacesCheckBox != null) _generateFacesCheckBox.IsChecked = true;
            if (_generateWipeoutsCheckBox != null) _generateWipeoutsCheckBox.IsChecked = true;
                if (_generateDimensionsCheckBox != null) _generateDimensionsCheckBox.IsChecked = true;
                if (_generateLeadersCheckBox != null) _generateLeadersCheckBox.IsChecked = true;
                if (_generateMlinesCheckBox != null) _generateMlinesCheckBox.IsChecked = true;
                if (_generateRaysCheckBox != null) _generateRaysCheckBox.IsChecked = true;
                if (_generateXlinesCheckBox != null) _generateXlinesCheckBox.IsChecked = true;
            }
            else
            {
                // Disable all entity checkboxes when master is unchecked
                if (_generateLinesCheckBox != null) _generateLinesCheckBox.IsChecked = false;
                if (_generateArcsCheckBox != null) _generateArcsCheckBox.IsChecked = false;
                if (_generateCirclesCheckBox != null) _generateCirclesCheckBox.IsChecked = false;
                if (_generateEllipsesCheckBox != null) _generateEllipsesCheckBox.IsChecked = false;
                if (_generatePolylines2DCheckBox != null) _generatePolylines2DCheckBox.IsChecked = false;
                if (_generatePolylines3DCheckBox != null) _generatePolylines3DCheckBox.IsChecked = false;
            if (_generateLwPolylinesCheckBox != null) _generateLwPolylinesCheckBox.IsChecked = false;
            if (_generateSplinesCheckBox != null) _generateSplinesCheckBox.IsChecked = false;
            if (_generateTextsCheckBox != null) _generateTextsCheckBox.IsChecked = false;
            if (_generateMTextsCheckBox != null) _generateMTextsCheckBox.IsChecked = false;
            if (_generatePointsCheckBox != null) _generatePointsCheckBox.IsChecked = false;
            if (_generateInsertCheckBox != null) _generateInsertCheckBox.IsChecked = false;
            if (_generateHatchesCheckBox != null) _generateHatchesCheckBox.IsChecked = false;
            if (_generateSolidsCheckBox != null) _generateSolidsCheckBox.IsChecked = false;
            if (_generateFacesCheckBox != null) _generateFacesCheckBox.IsChecked = false;
            if (_generateWipeoutsCheckBox != null) _generateWipeoutsCheckBox.IsChecked = false;
                if (_generateDimensionsCheckBox != null) _generateDimensionsCheckBox.IsChecked = false;
                if (_generateLeadersCheckBox != null) _generateLeadersCheckBox.IsChecked = false;
                if (_generateMlinesCheckBox != null) _generateMlinesCheckBox.IsChecked = false;
                if (_generateRaysCheckBox != null) _generateRaysCheckBox.IsChecked = false;
                if (_generateXlinesCheckBox != null) _generateXlinesCheckBox.IsChecked = false;
            }
            
            RegenerateCodeIfLoaded();
        }
        
        private void OnOptionChanged(object? sender, RoutedEventArgs e)
        {
            RegenerateCodeIfLoaded();
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
        
        private void SetupOptionEventHandlers()
        {
            // Add event handlers to all checkboxes for auto-regeneration
            if (_generateHeaderCheckBox != null)
                _generateHeaderCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateUsingStatementsCheckBox != null)
                _generateUsingStatementsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateDetailedCommentsCheckBox != null)
                _generateDetailedCommentsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_groupEntitiesByTypeCheckBox != null)
                _groupEntitiesByTypeCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateLayersCheckBox != null)
                _generateLayersCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateLinetypesCheckBox != null)
                _generateLinetypesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateTextStylesCheckBox != null)
                _generateTextStylesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateBlocksCheckBox != null)
                _generateBlocksCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateDimensionStylesCheckBox != null)
                _generateDimensionStylesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateMLineStylesCheckBox != null)
                _generateMLineStylesCheckBox.IsCheckedChanged += OnOptionChanged;
            
            // Entity-specific checkboxes (excluding master entities checkbox as it has special handling)
            if (_generateLinesCheckBox != null)
                _generateLinesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateArcsCheckBox != null)
                _generateArcsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateCirclesCheckBox != null)
                _generateCirclesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateEllipsesCheckBox != null)
                _generateEllipsesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generatePolylines2DCheckBox != null)
                _generatePolylines2DCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generatePolylines3DCheckBox != null)
                _generatePolylines3DCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateLwPolylinesCheckBox != null)
                _generateLwPolylinesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateSplinesCheckBox != null)
                _generateSplinesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateTextsCheckBox != null)
                _generateTextsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateMTextsCheckBox != null)
                _generateMTextsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generatePointsCheckBox != null)
                _generatePointsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateInsertCheckBox != null)
                _generateInsertCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateHatchesCheckBox != null)
                _generateHatchesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateSolidsCheckBox != null)
                _generateSolidsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateFacesCheckBox != null)
                _generateFacesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateWipeoutsCheckBox != null)
                _generateWipeoutsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateDimensionsCheckBox != null)
                _generateDimensionsCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateLeadersCheckBox != null)
                _generateLeadersCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateMlinesCheckBox != null)
                _generateMlinesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateRaysCheckBox != null)
                _generateRaysCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateXlinesCheckBox != null)
                _generateXlinesCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateSaveCommentCheckBox != null)
                _generateSaveCommentCheckBox.IsCheckedChanged += OnOptionChanged;
            if (_generateReturnStatementCheckBox != null)
                _generateReturnStatementCheckBox.IsCheckedChanged += OnOptionChanged;
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
}