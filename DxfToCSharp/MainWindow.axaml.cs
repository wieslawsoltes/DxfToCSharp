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
    private TabControl? _leftTabControl;
    private readonly TabControl? _rightTabControl;
    private readonly WindowNotificationManager? _notificationManager;
        
    // Options UI controls
    private readonly CheckBox? _generateHeaderCheckBox;
    private readonly CheckBox? _generateHeaderVariablesCheckBox;
    private readonly CheckBox? _generateUsingStatementsCheckBox;
    private readonly CheckBox? _generateDetailedCommentsCheckBox;
    private readonly CheckBox? _groupEntitiesByTypeCheckBox;
    private readonly CheckBox? _generateLayersCheckBox;
    private readonly CheckBox? _generateLinetypesCheckBox;
    private readonly CheckBox? _generateTextStylesCheckBox;
    private readonly CheckBox? _generateBlocksCheckBox;
    private readonly CheckBox? _generateDimensionStylesCheckBox;
    private readonly CheckBox? _generateMLineStylesCheckBox;
    private readonly CheckBox? _generateEntitiesCheckBox;
    private readonly CheckBox? _generateLinesCheckBox;
    private readonly CheckBox? _generateArcsCheckBox;
    private readonly CheckBox? _generateCirclesCheckBox;
    private readonly CheckBox? _generateEllipsesCheckBox;
    private readonly CheckBox? _generatePolylines2DCheckBox;
    private readonly CheckBox? _generatePolylines3DCheckBox;
    private readonly CheckBox? _generateLwPolylinesCheckBox;
    private readonly CheckBox? _generateSplinesCheckBox;
    private readonly CheckBox? _generateTextsCheckBox;
    private readonly CheckBox? _generateMTextsCheckBox;
    private readonly CheckBox? _generatePointsCheckBox;
    private readonly CheckBox? _generateInsertCheckBox;
    private readonly CheckBox? _generateHatchesCheckBox;
    private readonly CheckBox? _generateSolidsCheckBox;
    private readonly CheckBox? _generateFacesCheckBox;
    private readonly CheckBox? _generateWipeoutsCheckBox;
    private readonly CheckBox? _generateDimensionsCheckBox;
    private readonly CheckBox? _generateLinearDimensionsCheckBox;
    private readonly CheckBox? _generateAlignedDimensionsCheckBox;
    private readonly CheckBox? _generateRadialDimensionsCheckBox;
    private readonly CheckBox? _generateDiametricDimensionsCheckBox;
    private readonly CheckBox? _generateAngular2LineDimensionsCheckBox;
    private readonly CheckBox? _generateAngular3PointDimensionsCheckBox;
    private readonly CheckBox? _generateOrdinateDimensionsCheckBox;
    private readonly CheckBox? _generateLeadersCheckBox;
    private readonly CheckBox? _generateMlinesCheckBox;
    private readonly CheckBox? _generateRaysCheckBox;
    private readonly CheckBox? _generateXlinesCheckBox;
    private readonly CheckBox? _generateImagesCheckBox;
    private readonly CheckBox? _generateMeshesCheckBox;
    private readonly CheckBox? _generatePolyfaceMeshesCheckBox;
    private readonly CheckBox? _generatePolygonMeshesCheckBox;
    private readonly CheckBox? _generateShapesCheckBox;
    private readonly CheckBox? _generateTolerancesCheckBox;
    private readonly CheckBox? _generateTracesCheckBox;
    private readonly CheckBox? _generateUnderlaysCheckBox;
    private readonly CheckBox? _generateViewportsCheckBox;
    private readonly CheckBox? _generateSaveCommentCheckBox;
    private readonly CheckBox? _generateReturnStatementCheckBox;
    
    // New ComboBox for presets
    private readonly ComboBox? _presetComboBox;
    
    // Track loaded DXF document and path for auto-regeneration
    private DxfDocument? _loadedDocument;
    private string? _loadedFilePath;
    
    // Flag to prevent recursive event handling during programmatic checkbox changes
    private bool _isHandlingEvents = false;
    

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
            
        // Initialize options UI controls
        _generateHeaderCheckBox = this.FindControl<CheckBox>("GenerateHeaderCheckBox");
        _generateHeaderVariablesCheckBox = this.FindControl<CheckBox>("GenerateHeaderVariablesCheckBox");
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
        _generateHatchesCheckBox = this.FindControl<CheckBox>("GenerateHatchEntitiesCheckBox");
        _generateSolidsCheckBox = this.FindControl<CheckBox>("GenerateSolidEntitiesCheckBox");
        _generateFacesCheckBox = this.FindControl<CheckBox>("GenerateFace3dEntitiesCheckBox");
        _generateWipeoutsCheckBox = this.FindControl<CheckBox>("GenerateWipeoutEntitiesCheckBox");
        _generateDimensionsCheckBox = this.FindControl<CheckBox>("GenerateDimensionEntitiesCheckBox");
        _generateLinearDimensionsCheckBox = this.FindControl<CheckBox>("GenerateLinearDimensionEntitiesCheckBox");
        _generateAlignedDimensionsCheckBox = this.FindControl<CheckBox>("GenerateAlignedDimensionEntitiesCheckBox");
        _generateRadialDimensionsCheckBox = this.FindControl<CheckBox>("GenerateRadialDimensionEntitiesCheckBox");
        _generateDiametricDimensionsCheckBox = this.FindControl<CheckBox>("GenerateDiametricDimensionEntitiesCheckBox");
        _generateAngular2LineDimensionsCheckBox = this.FindControl<CheckBox>("GenerateAngular2LineDimensionEntitiesCheckBox");
        _generateAngular3PointDimensionsCheckBox = this.FindControl<CheckBox>("GenerateAngular3PointDimensionEntitiesCheckBox");
        _generateOrdinateDimensionsCheckBox = this.FindControl<CheckBox>("GenerateOrdinateDimensionEntitiesCheckBox");
        _generateLeadersCheckBox = this.FindControl<CheckBox>("GenerateLeaderEntitiesCheckBox");
        _generateMlinesCheckBox = this.FindControl<CheckBox>("GenerateMLineEntitiesCheckBox");
        _generateRaysCheckBox = this.FindControl<CheckBox>("GenerateRayEntitiesCheckBox");
        _generateXlinesCheckBox = this.FindControl<CheckBox>("GenerateXLineEntitiesCheckBox");
        _generateImagesCheckBox = this.FindControl<CheckBox>("GenerateImageEntitiesCheckBox");
        _generateMeshesCheckBox = this.FindControl<CheckBox>("GenerateMeshEntitiesCheckBox");
        _generatePolyfaceMeshesCheckBox = this.FindControl<CheckBox>("GeneratePolyfaceMeshEntitiesCheckBox");
        _generatePolygonMeshesCheckBox = this.FindControl<CheckBox>("GeneratePolygonMeshEntitiesCheckBox");
        _generateShapesCheckBox = this.FindControl<CheckBox>("GenerateShapeEntitiesCheckBox");
        _generateTolerancesCheckBox = this.FindControl<CheckBox>("GenerateToleranceEntitiesCheckBox");
        _generateTracesCheckBox = this.FindControl<CheckBox>("GenerateTraceEntitiesCheckBox");
        _generateUnderlaysCheckBox = this.FindControl<CheckBox>("GenerateUnderlayEntitiesCheckBox");
        _generateViewportsCheckBox = this.FindControl<CheckBox>("GenerateViewportEntitiesCheckBox");
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
        return new DxfCodeGenerationOptions
        {
            GenerateHeader = _generateHeaderCheckBox?.IsChecked ?? true,
            GenerateHeaderVariables = _generateHeaderVariablesCheckBox?.IsChecked ?? true,
            GenerateUsingStatements = _generateUsingStatementsCheckBox?.IsChecked ?? true,
            GenerateDetailedComments = _generateDetailedCommentsCheckBox?.IsChecked ?? false,
            GroupEntitiesByType = _groupEntitiesByTypeCheckBox?.IsChecked ?? false,
            GenerateLayers = _generateLayersCheckBox?.IsChecked ?? true,
            GenerateLinetypes = _generateLinetypesCheckBox?.IsChecked ?? true,
            GenerateTextStyles = _generateTextStylesCheckBox?.IsChecked ?? true,
            GenerateBlocks = _generateBlocksCheckBox?.IsChecked ?? true,
            GenerateDimensionStyles = _generateDimensionStylesCheckBox?.IsChecked ?? true,
            GenerateMLineStyles = _generateMLineStylesCheckBox?.IsChecked ?? true,
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
            GenerateHatchEntities = _generateHatchesCheckBox?.IsChecked ?? true,
            GenerateSolidEntities = _generateSolidsCheckBox?.IsChecked ?? true,
            GenerateFace3dEntities = _generateFacesCheckBox?.IsChecked ?? true,
            GenerateWipeoutEntities = _generateWipeoutsCheckBox?.IsChecked ?? true,
            GenerateLinearDimensionEntities = _generateLinearDimensionsCheckBox?.IsChecked ?? true,
            GenerateAlignedDimensionEntities = _generateAlignedDimensionsCheckBox?.IsChecked ?? true,
            GenerateRadialDimensionEntities = _generateRadialDimensionsCheckBox?.IsChecked ?? true,
            GenerateDiametricDimensionEntities = _generateDiametricDimensionsCheckBox?.IsChecked ?? true,
            GenerateAngular2LineDimensionEntities = _generateAngular2LineDimensionsCheckBox?.IsChecked ?? true,
            GenerateAngular3PointDimensionEntities = _generateAngular3PointDimensionsCheckBox?.IsChecked ?? true,
            GenerateOrdinateDimensionEntities = _generateOrdinateDimensionsCheckBox?.IsChecked ?? true,
            GenerateLeaderEntities = _generateLeadersCheckBox?.IsChecked ?? true,
            GenerateMLineEntities = _generateMlinesCheckBox?.IsChecked ?? true,
            GenerateRayEntities = _generateRaysCheckBox?.IsChecked ?? true,
            GenerateXLineEntities = _generateXlinesCheckBox?.IsChecked ?? true,
            GenerateImageEntities = _generateImagesCheckBox?.IsChecked ?? true,
            GenerateMeshEntities = _generateMeshesCheckBox?.IsChecked ?? true,
            GeneratePolyfaceMeshEntities = _generatePolyfaceMeshesCheckBox?.IsChecked ?? true,
            GeneratePolygonMeshEntities = _generatePolygonMeshesCheckBox?.IsChecked ?? true,
            GenerateShapeEntities = _generateShapesCheckBox?.IsChecked ?? true,
            GenerateToleranceEntities = _generateTolerancesCheckBox?.IsChecked ?? true,
            GenerateTraceEntities = _generateTracesCheckBox?.IsChecked ?? true,
            GenerateUnderlayEntities = _generateUnderlaysCheckBox?.IsChecked ?? true,
            GenerateViewportEntities = _generateViewportsCheckBox?.IsChecked ?? true,
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
            GenerateHeaderVariables = true,
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
        if (_generateHeaderVariablesCheckBox != null) _generateHeaderVariablesCheckBox.IsChecked = options.GenerateHeaderVariables;
        if (_generateUsingStatementsCheckBox != null) _generateUsingStatementsCheckBox.IsChecked = options.GenerateUsingStatements;
        if (_generateDetailedCommentsCheckBox != null) _generateDetailedCommentsCheckBox.IsChecked = options.GenerateDetailedComments;
        if (_groupEntitiesByTypeCheckBox != null) _groupEntitiesByTypeCheckBox.IsChecked = options.GroupEntitiesByType;
        if (_generateLayersCheckBox != null) _generateLayersCheckBox.IsChecked = options.GenerateLayers;
        if (_generateLinetypesCheckBox != null) _generateLinetypesCheckBox.IsChecked = options.GenerateLinetypes;
        if (_generateTextStylesCheckBox != null) _generateTextStylesCheckBox.IsChecked = options.GenerateTextStyles;
        if (_generateBlocksCheckBox != null) _generateBlocksCheckBox.IsChecked = options.GenerateBlocks;
        if (_generateDimensionStylesCheckBox != null) _generateDimensionStylesCheckBox.IsChecked = options.GenerateDimensionStyles;
        if (_generateMLineStylesCheckBox != null) _generateMLineStylesCheckBox.IsChecked = options.GenerateMLineStyles;
        if (_generateEntitiesCheckBox != null)
            _generateEntitiesCheckBox.IsChecked = options.GenerateEntities;
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
        if (_generateHatchesCheckBox != null) _generateHatchesCheckBox.IsChecked = options.GenerateHatchEntities;
        if (_generateSolidsCheckBox != null) _generateSolidsCheckBox.IsChecked = options.GenerateSolidEntities;
        if (_generateFacesCheckBox != null) _generateFacesCheckBox.IsChecked = options.GenerateFace3dEntities;
        if (_generateWipeoutsCheckBox != null) _generateWipeoutsCheckBox.IsChecked = options.GenerateWipeoutEntities;
        if (_generateDimensionsCheckBox != null) _generateDimensionsCheckBox.IsChecked = options.GenerateDimensionEntities;
        if (_generateLeadersCheckBox != null) _generateLeadersCheckBox.IsChecked = options.GenerateLeaderEntities;
        if (_generateMlinesCheckBox != null) _generateMlinesCheckBox.IsChecked = options.GenerateMLineEntities;
        if (_generateRaysCheckBox != null) _generateRaysCheckBox.IsChecked = options.GenerateRayEntities;
        if (_generateXlinesCheckBox != null) _generateXlinesCheckBox.IsChecked = options.GenerateXLineEntities;
        if (_generateImagesCheckBox != null) _generateImagesCheckBox.IsChecked = options.GenerateImageEntities;
        if (_generateMeshesCheckBox != null) _generateMeshesCheckBox.IsChecked = options.GenerateMeshEntities;
        if (_generatePolyfaceMeshesCheckBox != null) _generatePolyfaceMeshesCheckBox.IsChecked = options.GeneratePolyfaceMeshEntities;
        if (_generatePolygonMeshesCheckBox != null) _generatePolygonMeshesCheckBox.IsChecked = options.GeneratePolygonMeshEntities;
        if (_generateShapesCheckBox != null) _generateShapesCheckBox.IsChecked = options.GenerateShapeEntities;
        if (_generateTolerancesCheckBox != null) _generateTolerancesCheckBox.IsChecked = options.GenerateToleranceEntities;
        if (_generateTracesCheckBox != null) _generateTracesCheckBox.IsChecked = options.GenerateTraceEntities;
        if (_generateUnderlaysCheckBox != null) _generateUnderlaysCheckBox.IsChecked = options.GenerateUnderlayEntities;
        if (_generateViewportsCheckBox != null) _generateViewportsCheckBox.IsChecked = options.GenerateViewportEntities;
        if (_generateSaveCommentCheckBox != null) _generateSaveCommentCheckBox.IsChecked = options.GenerateSaveComment;
        if (_generateReturnStatementCheckBox != null) _generateReturnStatementCheckBox.IsChecked = options.GenerateReturnStatement;
    }

    private void OnEntitiesCheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        // Temporarily disable event handlers to prevent recursive calls during programmatic changes
        bool wasHandlingEvents = _isHandlingEvents;
        _isHandlingEvents = true;
            
        try
        {
            if (_generateEntitiesCheckBox?.IsChecked == true)
            {
                // Check all entity checkboxes and disable them when master is checked
                if (_generateLinesCheckBox != null) { _generateLinesCheckBox.IsChecked = true; _generateLinesCheckBox.IsEnabled = false; }
                if (_generateArcsCheckBox != null) { _generateArcsCheckBox.IsChecked = true; _generateArcsCheckBox.IsEnabled = false; }
                if (_generateCirclesCheckBox != null) { _generateCirclesCheckBox.IsChecked = true; _generateCirclesCheckBox.IsEnabled = false; }
                if (_generateEllipsesCheckBox != null) { _generateEllipsesCheckBox.IsChecked = true; _generateEllipsesCheckBox.IsEnabled = false; }
                if (_generatePolylines2DCheckBox != null) { _generatePolylines2DCheckBox.IsChecked = true; _generatePolylines2DCheckBox.IsEnabled = false; }
                if (_generatePolylines3DCheckBox != null) { _generatePolylines3DCheckBox.IsChecked = true; _generatePolylines3DCheckBox.IsEnabled = false; }
                if (_generateLwPolylinesCheckBox != null) { _generateLwPolylinesCheckBox.IsChecked = true; _generateLwPolylinesCheckBox.IsEnabled = false; }
                if (_generateSplinesCheckBox != null) { _generateSplinesCheckBox.IsChecked = true; _generateSplinesCheckBox.IsEnabled = false; }
                if (_generateTextsCheckBox != null) { _generateTextsCheckBox.IsChecked = true; _generateTextsCheckBox.IsEnabled = false; }
                if (_generateMTextsCheckBox != null) { _generateMTextsCheckBox.IsChecked = true; _generateMTextsCheckBox.IsEnabled = false; }
                if (_generatePointsCheckBox != null) { _generatePointsCheckBox.IsChecked = true; _generatePointsCheckBox.IsEnabled = false; }
                if (_generateInsertCheckBox != null) { _generateInsertCheckBox.IsChecked = true; _generateInsertCheckBox.IsEnabled = false; }
                if (_generateHatchesCheckBox != null) { _generateHatchesCheckBox.IsChecked = true; _generateHatchesCheckBox.IsEnabled = false; }
                if (_generateSolidsCheckBox != null) { _generateSolidsCheckBox.IsChecked = true; _generateSolidsCheckBox.IsEnabled = false; }
                if (_generateFacesCheckBox != null) { _generateFacesCheckBox.IsChecked = true; _generateFacesCheckBox.IsEnabled = false; }
                if (_generateWipeoutsCheckBox != null) { _generateWipeoutsCheckBox.IsChecked = true; _generateWipeoutsCheckBox.IsEnabled = false; }
                if (_generateDimensionsCheckBox != null) { _generateDimensionsCheckBox.IsChecked = true; _generateDimensionsCheckBox.IsEnabled = false; }
                if (_generateLeadersCheckBox != null) { _generateLeadersCheckBox.IsChecked = true; _generateLeadersCheckBox.IsEnabled = false; }
                if (_generateMlinesCheckBox != null) { _generateMlinesCheckBox.IsChecked = true; _generateMlinesCheckBox.IsEnabled = false; }
                if (_generateRaysCheckBox != null) { _generateRaysCheckBox.IsChecked = true; _generateRaysCheckBox.IsEnabled = false; }
                if (_generateXlinesCheckBox != null) { _generateXlinesCheckBox.IsChecked = true; _generateXlinesCheckBox.IsEnabled = false; }
                if (_generateImagesCheckBox != null) { _generateImagesCheckBox.IsChecked = true; _generateImagesCheckBox.IsEnabled = false; }
                if (_generateMeshesCheckBox != null) { _generateMeshesCheckBox.IsChecked = true; _generateMeshesCheckBox.IsEnabled = false; }
                if (_generatePolyfaceMeshesCheckBox != null) { _generatePolyfaceMeshesCheckBox.IsChecked = true; _generatePolyfaceMeshesCheckBox.IsEnabled = false; }
                if (_generatePolygonMeshesCheckBox != null) { _generatePolygonMeshesCheckBox.IsChecked = true; _generatePolygonMeshesCheckBox.IsEnabled = false; }
                if (_generateShapesCheckBox != null) { _generateShapesCheckBox.IsChecked = true; _generateShapesCheckBox.IsEnabled = false; }
                if (_generateTolerancesCheckBox != null) { _generateTolerancesCheckBox.IsChecked = true; _generateTolerancesCheckBox.IsEnabled = false; }
                if (_generateTracesCheckBox != null) { _generateTracesCheckBox.IsChecked = true; _generateTracesCheckBox.IsEnabled = false; }
                if (_generateUnderlaysCheckBox != null) { _generateUnderlaysCheckBox.IsChecked = true; _generateUnderlaysCheckBox.IsEnabled = false; }
                if (_generateViewportsCheckBox != null) { _generateViewportsCheckBox.IsChecked = true; _generateViewportsCheckBox.IsEnabled = false; }
            }
            else
            {
                // When "All Entities" is unchecked, uncheck all individual entity checkboxes and enable them
                if (_generateLinesCheckBox != null) { _generateLinesCheckBox.IsChecked = false; _generateLinesCheckBox.IsEnabled = true; }
                if (_generateArcsCheckBox != null) { _generateArcsCheckBox.IsChecked = false; _generateArcsCheckBox.IsEnabled = true; }
                if (_generateCirclesCheckBox != null) { _generateCirclesCheckBox.IsChecked = false; _generateCirclesCheckBox.IsEnabled = true; }
                if (_generateEllipsesCheckBox != null) { _generateEllipsesCheckBox.IsChecked = false; _generateEllipsesCheckBox.IsEnabled = true; }
                if (_generatePolylines2DCheckBox != null) { _generatePolylines2DCheckBox.IsChecked = false; _generatePolylines2DCheckBox.IsEnabled = true; }
                if (_generatePolylines3DCheckBox != null) { _generatePolylines3DCheckBox.IsChecked = false; _generatePolylines3DCheckBox.IsEnabled = true; }
                if (_generateLwPolylinesCheckBox != null) { _generateLwPolylinesCheckBox.IsChecked = false; _generateLwPolylinesCheckBox.IsEnabled = true; }
                if (_generateSplinesCheckBox != null) { _generateSplinesCheckBox.IsChecked = false; _generateSplinesCheckBox.IsEnabled = true; }
                if (_generateTextsCheckBox != null) { _generateTextsCheckBox.IsChecked = false; _generateTextsCheckBox.IsEnabled = true; }
                if (_generateMTextsCheckBox != null) { _generateMTextsCheckBox.IsChecked = false; _generateMTextsCheckBox.IsEnabled = true; }
                if (_generatePointsCheckBox != null) { _generatePointsCheckBox.IsChecked = false; _generatePointsCheckBox.IsEnabled = true; }
                if (_generateInsertCheckBox != null) { _generateInsertCheckBox.IsChecked = false; _generateInsertCheckBox.IsEnabled = true; }
                if (_generateHatchesCheckBox != null) { _generateHatchesCheckBox.IsChecked = false; _generateHatchesCheckBox.IsEnabled = true; }
                if (_generateSolidsCheckBox != null) { _generateSolidsCheckBox.IsChecked = false; _generateSolidsCheckBox.IsEnabled = true; }
                if (_generateFacesCheckBox != null) { _generateFacesCheckBox.IsChecked = false; _generateFacesCheckBox.IsEnabled = true; }
                if (_generateWipeoutsCheckBox != null) { _generateWipeoutsCheckBox.IsChecked = false; _generateWipeoutsCheckBox.IsEnabled = true; }
                if (_generateDimensionsCheckBox != null) { _generateDimensionsCheckBox.IsChecked = false; _generateDimensionsCheckBox.IsEnabled = true; }
                if (_generateLeadersCheckBox != null) { _generateLeadersCheckBox.IsChecked = false; _generateLeadersCheckBox.IsEnabled = true; }
                if (_generateMlinesCheckBox != null) { _generateMlinesCheckBox.IsChecked = false; _generateMlinesCheckBox.IsEnabled = true; }
                if (_generateRaysCheckBox != null) { _generateRaysCheckBox.IsChecked = false; _generateRaysCheckBox.IsEnabled = true; }
                if (_generateXlinesCheckBox != null) { _generateXlinesCheckBox.IsChecked = false; _generateXlinesCheckBox.IsEnabled = true; }
                if (_generateImagesCheckBox != null) { _generateImagesCheckBox.IsChecked = false; _generateImagesCheckBox.IsEnabled = true; }
                if (_generateMeshesCheckBox != null) { _generateMeshesCheckBox.IsChecked = false; _generateMeshesCheckBox.IsEnabled = true; }
                if (_generatePolyfaceMeshesCheckBox != null) { _generatePolyfaceMeshesCheckBox.IsChecked = false; _generatePolyfaceMeshesCheckBox.IsEnabled = true; }
                if (_generatePolygonMeshesCheckBox != null) { _generatePolygonMeshesCheckBox.IsChecked = false; _generatePolygonMeshesCheckBox.IsEnabled = true; }
                if (_generateShapesCheckBox != null) { _generateShapesCheckBox.IsChecked = false; _generateShapesCheckBox.IsEnabled = true; }
                if (_generateTolerancesCheckBox != null) { _generateTolerancesCheckBox.IsChecked = false; _generateTolerancesCheckBox.IsEnabled = true; }
                if (_generateTracesCheckBox != null) { _generateTracesCheckBox.IsChecked = false; _generateTracesCheckBox.IsEnabled = true; }
                if (_generateUnderlaysCheckBox != null) { _generateUnderlaysCheckBox.IsChecked = false; _generateUnderlaysCheckBox.IsEnabled = true; }
                if (_generateViewportsCheckBox != null) { _generateViewportsCheckBox.IsChecked = false; _generateViewportsCheckBox.IsEnabled = true; }
            }
        }
        finally
        {
            _isHandlingEvents = wasHandlingEvents;
        }
            
        // Always regenerate code after changing checkbox states
        RegenerateCodeIfLoaded();
    }
        
    private void OnOptionChanged(object? sender, RoutedEventArgs e)
    {
        // Skip regeneration if we're in the middle of programmatic checkbox changes
        if (!_isHandlingEvents)
        {
            RegenerateCodeIfLoaded();
        }
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
        if (_generateHeaderVariablesCheckBox != null)
            _generateHeaderVariablesCheckBox.IsCheckedChanged += OnOptionChanged;
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
        if (_generateImagesCheckBox != null)
            _generateImagesCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generateMeshesCheckBox != null)
            _generateMeshesCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generatePolyfaceMeshesCheckBox != null)
            _generatePolyfaceMeshesCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generatePolygonMeshesCheckBox != null)
            _generatePolygonMeshesCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generateShapesCheckBox != null)
            _generateShapesCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generateTolerancesCheckBox != null)
            _generateTolerancesCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generateTracesCheckBox != null)
            _generateTracesCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generateUnderlaysCheckBox != null)
            _generateUnderlaysCheckBox.IsCheckedChanged += OnOptionChanged;
        if (_generateViewportsCheckBox != null)
            _generateViewportsCheckBox.IsCheckedChanged += OnOptionChanged;
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
