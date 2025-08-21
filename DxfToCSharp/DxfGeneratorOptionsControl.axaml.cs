using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DxfToCSharp.Core;

namespace DxfToCSharp;

public partial class DxfGeneratorOptionsControl : UserControl
{
    private CheckBox? _generateHeaderCheckBox;
    private CheckBox? _generateHeaderVariablesCheckBox;
    private CheckBox? _generateUsingStatementsCheckBox;
    private CheckBox? _generateDetailedCommentsCheckBox;
    private CheckBox? _generateTablesCheckBox;
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
    private CheckBox? _generateLinearDimensionsCheckBox;
    private CheckBox? _generateAlignedDimensionsCheckBox;
    private CheckBox? _generateRadialDimensionsCheckBox;
    private CheckBox? _generateDiametricDimensionsCheckBox;
    private CheckBox? _generateAngular2LineDimensionsCheckBox;
    private CheckBox? _generateAngular3PointDimensionsCheckBox;
    private CheckBox? _generateOrdinateDimensionsCheckBox;
    private CheckBox? _generateLeadersCheckBox;
    private CheckBox? _generateMlinesCheckBox;
    private CheckBox? _generateRaysCheckBox;
    private CheckBox? _generateXlinesCheckBox;
    private CheckBox? _generateImagesCheckBox;
    private CheckBox? _generateMeshesCheckBox;
    private CheckBox? _generatePolyfaceMeshesCheckBox;
    private CheckBox? _generatePolygonMeshesCheckBox;
    private CheckBox? _generateShapesCheckBox;
    private CheckBox? _generateTolerancesCheckBox;
    private CheckBox? _generateTracesCheckBox;
    private CheckBox? _generateUnderlaysCheckBox;
    private CheckBox? _generateViewportsCheckBox;
    private CheckBox? _generateSaveCommentCheckBox;
    private CheckBox? _generateReturnStatementCheckBox;
    private CheckBox? _generateObjectsCheckBox;
    private CheckBox? _generateGroupsCheckBox;
    private CheckBox? _generateLayoutsCheckBox;
    private CheckBox? _generateImageDefinitionsCheckBox;
    private CheckBox? _generateUnderlayDefinitionsCheckBox;
    private CheckBox? _generateMLineEntitiesCheckBox;
    private CheckBox? _generateXRecordObjectsCheckBox;
    private CheckBox? _generateDictionaryObjectsCheckBox;
    private CheckBox? _generateRasterVariablesObjectsCheckBox;
    private bool _isHandlingEvents = true;

    public event EventHandler<OptionsChangedEventArgs>? OptionsChanged;
        
    public DxfGeneratorOptionsControl()
    {
        InitializeComponent();
        InitializeControls();
        SetupEventHandlers();
    }
        
    private void InitializeControls()
    {
        // Initialize all checkbox controls
        _generateHeaderCheckBox = this.FindControl<CheckBox>("GenerateHeaderCheckBox");
        _generateHeaderVariablesCheckBox = this.FindControl<CheckBox>("GenerateHeaderVariablesCheckBox");
        _generateUsingStatementsCheckBox = this.FindControl<CheckBox>("GenerateUsingStatementsCheckBox");
        _generateDetailedCommentsCheckBox = this.FindControl<CheckBox>("GenerateDetailedCommentsCheckBox");
        _generateTablesCheckBox = this.FindControl<CheckBox>("GenerateTablesCheckBox");
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
            
        // Initialize Objects section checkboxes
        _generateObjectsCheckBox = this.FindControl<CheckBox>("GenerateObjectsCheckBox");
        _generateGroupsCheckBox = this.FindControl<CheckBox>("GenerateGroupsCheckBox");
        _generateLayoutsCheckBox = this.FindControl<CheckBox>("GenerateLayoutsCheckBox");
        _generateImageDefinitionsCheckBox = this.FindControl<CheckBox>("GenerateImageDefinitionsCheckBox");
        _generateUnderlayDefinitionsCheckBox = this.FindControl<CheckBox>("GenerateUnderlayDefinitionsCheckBox");
        _generateMLineEntitiesCheckBox = this.FindControl<CheckBox>("GenerateMLineEntitiesCheckBox");
        _generateXRecordObjectsCheckBox = this.FindControl<CheckBox>("GenerateXRecordObjectsCheckBox");
        _generateDictionaryObjectsCheckBox = this.FindControl<CheckBox>("GenerateDictionaryObjectsCheckBox");
        _generateRasterVariablesObjectsCheckBox = this.FindControl<CheckBox>("GenerateRasterVariablesObjectsCheckBox");
    }
        
    private void SetupEventHandlers()
    {
        // Set up event handler for master entities checkbox
        if (_generateEntitiesCheckBox != null)
        {
            _generateEntitiesCheckBox.Checked += OnEntitiesCheckBoxChanged;
            _generateEntitiesCheckBox.Unchecked += OnEntitiesCheckBoxChanged;
        }
            
        // Set up event handler for master objects checkbox
        if (_generateObjectsCheckBox != null)
        {
            _generateObjectsCheckBox.Checked += OnObjectsCheckBoxChanged;
            _generateObjectsCheckBox.Unchecked += OnObjectsCheckBoxChanged;
        }
            
        // Set up event handler for master tables checkbox
        if (_generateTablesCheckBox != null)
        {
            _generateTablesCheckBox.Checked += OnTablesCheckBoxChanged;
            _generateTablesCheckBox.Unchecked += OnTablesCheckBoxChanged;
        }
                
        // Set up event handlers for all option checkboxes to trigger options changed event
        SetupOptionEventHandlers();
    }
        
    private void SetupOptionEventHandlers()
    {
        var checkboxes = new[]
        {
            _generateHeaderCheckBox, _generateHeaderVariablesCheckBox, _generateUsingStatementsCheckBox,
            _generateTablesCheckBox, _generateLinetypesCheckBox, _generateTextStylesCheckBox, _generateBlocksCheckBox,
            _generateDimensionStylesCheckBox, _generateMLineStylesCheckBox, _generateEntitiesCheckBox,
            _generateLinesCheckBox, _generateArcsCheckBox, _generateCirclesCheckBox, _generateEllipsesCheckBox,
            _generatePolylines2DCheckBox, _generatePolylines3DCheckBox, _generateLwPolylinesCheckBox,
            _generateSplinesCheckBox, _generateTextsCheckBox, _generateMTextsCheckBox, _generatePointsCheckBox,
            _generateInsertCheckBox, _generateHatchesCheckBox, _generateSolidsCheckBox, _generateFacesCheckBox,
            _generateWipeoutsCheckBox, _generateLinearDimensionsCheckBox,
            _generateAlignedDimensionsCheckBox, _generateRadialDimensionsCheckBox, _generateDiametricDimensionsCheckBox,
            _generateAngular2LineDimensionsCheckBox, _generateAngular3PointDimensionsCheckBox, _generateOrdinateDimensionsCheckBox,
            _generateLeadersCheckBox, _generateMlinesCheckBox, _generateRaysCheckBox, _generateXlinesCheckBox,
            _generateImagesCheckBox, _generateMeshesCheckBox, _generatePolyfaceMeshesCheckBox, _generatePolygonMeshesCheckBox,
            _generateShapesCheckBox, _generateTolerancesCheckBox, _generateTracesCheckBox, _generateUnderlaysCheckBox,
            _generateViewportsCheckBox, _generateSaveCommentCheckBox, _generateReturnStatementCheckBox,
            _generateObjectsCheckBox, _generateGroupsCheckBox, _generateLayoutsCheckBox, _generateImageDefinitionsCheckBox,
            _generateUnderlayDefinitionsCheckBox, _generateMLineEntitiesCheckBox, _generateXRecordObjectsCheckBox,
            _generateDictionaryObjectsCheckBox, _generateRasterVariablesObjectsCheckBox
        };
            
        foreach (var checkbox in checkboxes)
        {
            if (checkbox != null)
            {
                checkbox.Checked += OnOptionChanged;
                checkbox.Unchecked += OnOptionChanged;
            }
        }
    }
        
    private void OnOptionChanged(object? sender, RoutedEventArgs e)
    {
        if (_isHandlingEvents)
        {
            RaiseOptionsChanged();
        }
    }
        
    private void RaiseOptionsChanged()
    {
        var options = GetOptionsFromUI();
        OptionsChanged?.Invoke(this, new OptionsChangedEventArgs(options));
    }

    public DxfCodeGenerationOptions GetOptionsFromUI()
    {
        return new DxfCodeGenerationOptions
        {
            GenerateHeader = _generateHeaderCheckBox?.IsChecked ?? true,
            GenerateHeaderVariables = _generateHeaderVariablesCheckBox?.IsChecked ?? true,
            GenerateUsingStatements = _generateUsingStatementsCheckBox?.IsChecked ?? true,
            GenerateDetailedComments = _generateDetailedCommentsCheckBox?.IsChecked ?? false,
            GenerateLayers = _generateLayersCheckBox?.IsChecked ?? true,
            GenerateLinetypes = _generateLinetypesCheckBox?.IsChecked ?? true,
            GenerateTextStyles = _generateTextStylesCheckBox?.IsChecked ?? true,
            GenerateBlocks = _generateBlocksCheckBox?.IsChecked ?? true,
            GenerateDimensionStyles = _generateDimensionStylesCheckBox?.IsChecked ?? true,
            GenerateMLineStyles = _generateMLineStylesCheckBox?.IsChecked ?? true,
            GenerateGroupObjects = _generateGroupsCheckBox?.IsChecked ?? true,
            GenerateLayoutObjects = _generateLayoutsCheckBox?.IsChecked ?? true,
            GenerateImageDefinitionObjects = _generateImageDefinitionsCheckBox?.IsChecked ?? true,
            GenerateUnderlayDefinitionObjects = _generateUnderlayDefinitionsCheckBox?.IsChecked ?? true,
            GenerateXRecordObjects = _generateXRecordObjectsCheckBox?.IsChecked ?? true,
            GenerateDictionaryObjects = _generateDictionaryObjectsCheckBox?.IsChecked ?? true,
            GenerateRasterVariablesObjects = _generateRasterVariablesObjectsCheckBox?.IsChecked ?? true,
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

    public void SetAllOptions(DxfCodeGenerationOptions options)
    {
        var wasHandlingEvents = _isHandlingEvents;
        _isHandlingEvents = false;
            
        try
        {
            if (_generateHeaderCheckBox != null) _generateHeaderCheckBox.IsChecked = options.GenerateHeader;
            if (_generateHeaderVariablesCheckBox != null) _generateHeaderVariablesCheckBox.IsChecked = options.GenerateHeaderVariables;
            if (_generateUsingStatementsCheckBox != null) _generateUsingStatementsCheckBox.IsChecked = options.GenerateUsingStatements;
            if (_generateDetailedCommentsCheckBox != null) _generateDetailedCommentsCheckBox.IsChecked = options.GenerateDetailedComments;
            if (_generateLayersCheckBox != null) _generateLayersCheckBox.IsChecked = options.GenerateLayers;
            if (_generateLinetypesCheckBox != null) _generateLinetypesCheckBox.IsChecked = options.GenerateLinetypes;
            if (_generateTextStylesCheckBox != null) _generateTextStylesCheckBox.IsChecked = options.GenerateTextStyles;
            if (_generateBlocksCheckBox != null) _generateBlocksCheckBox.IsChecked = options.GenerateBlocks;
            if (_generateDimensionStylesCheckBox != null) _generateDimensionStylesCheckBox.IsChecked = options.GenerateDimensionStyles;
            if (_generateMLineStylesCheckBox != null) _generateMLineStylesCheckBox.IsChecked = options.GenerateMLineStyles;

            var allTablesEnabled = options.GenerateLayers && options.GenerateLinetypes && options.GenerateTextStyles && 
                                   options.GenerateBlocks && options.GenerateDimensionStyles && options.GenerateMLineStyles;
            if (_generateTablesCheckBox != null) _generateTablesCheckBox.IsChecked = allTablesEnabled;
            
            var allObjectsEnabled = options.GenerateGroupObjects && options.GenerateLayoutObjects &&
                                   options.GenerateImageDefinitionObjects && options.GenerateUnderlayDefinitionObjects &&
                                   options.GenerateXRecordObjects && options.GenerateDictionaryObjects && options.GenerateRasterVariablesObjects;
            if (_generateObjectsCheckBox != null) _generateObjectsCheckBox.IsChecked = allObjectsEnabled;
            if (_generateGroupsCheckBox != null) _generateGroupsCheckBox.IsChecked = options.GenerateGroupObjects;
            if (_generateLayoutsCheckBox != null) _generateLayoutsCheckBox.IsChecked = options.GenerateLayoutObjects;
            if (_generateImageDefinitionsCheckBox != null) _generateImageDefinitionsCheckBox.IsChecked = options.GenerateImageDefinitionObjects;
            if (_generateUnderlayDefinitionsCheckBox != null) _generateUnderlayDefinitionsCheckBox.IsChecked = options.GenerateUnderlayDefinitionObjects;
            if (_generateXRecordObjectsCheckBox != null) _generateXRecordObjectsCheckBox.IsChecked = options.GenerateXRecordObjects;
            if (_generateDictionaryObjectsCheckBox != null) _generateDictionaryObjectsCheckBox.IsChecked = options.GenerateDictionaryObjects;
            if (_generateRasterVariablesObjectsCheckBox != null) _generateRasterVariablesObjectsCheckBox.IsChecked = options.GenerateRasterVariablesObjects;
            if (_generateLinesCheckBox != null) _generateLinesCheckBox.IsChecked = options.GenerateLineEntities;
            if (_generateArcsCheckBox != null) _generateArcsCheckBox.IsChecked = options.GenerateArcEntities;
            if (_generateCirclesCheckBox != null) _generateCirclesCheckBox.IsChecked = options.GenerateCircleEntities;
            if (_generateEllipsesCheckBox != null) _generateEllipsesCheckBox.IsChecked = options.GenerateEllipseEntities;
            if (_generatePolylines2DCheckBox != null) _generatePolylines2DCheckBox.IsChecked = options.GeneratePolylineEntities;
            if (_generateSplinesCheckBox != null) _generateSplinesCheckBox.IsChecked = options.GenerateSplineEntities;
            if (_generateTextsCheckBox != null) _generateTextsCheckBox.IsChecked = options.GenerateTextEntities;
            if (_generateMTextsCheckBox != null) _generateMTextsCheckBox.IsChecked = options.GenerateMTextEntities;
            if (_generatePointsCheckBox != null) _generatePointsCheckBox.IsChecked = options.GeneratePointEntities;
            if (_generateInsertCheckBox != null) _generateInsertCheckBox.IsChecked = options.GenerateInsertEntities;
            if (_generateHatchesCheckBox != null) _generateHatchesCheckBox.IsChecked = options.GenerateHatchEntities;
            if (_generateSolidsCheckBox != null) _generateSolidsCheckBox.IsChecked = options.GenerateSolidEntities;
            if (_generateFacesCheckBox != null) _generateFacesCheckBox.IsChecked = options.GenerateFace3dEntities;
            if (_generateWipeoutsCheckBox != null) _generateWipeoutsCheckBox.IsChecked = options.GenerateWipeoutEntities;
            if (_generateLinearDimensionsCheckBox != null) _generateLinearDimensionsCheckBox.IsChecked = options.GenerateLinearDimensionEntities;
            if (_generateAlignedDimensionsCheckBox != null) _generateAlignedDimensionsCheckBox.IsChecked = options.GenerateAlignedDimensionEntities;
            if (_generateRadialDimensionsCheckBox != null) _generateRadialDimensionsCheckBox.IsChecked = options.GenerateRadialDimensionEntities;
            if (_generateDiametricDimensionsCheckBox != null) _generateDiametricDimensionsCheckBox.IsChecked = options.GenerateDiametricDimensionEntities;
            if (_generateAngular2LineDimensionsCheckBox != null) _generateAngular2LineDimensionsCheckBox.IsChecked = options.GenerateAngular2LineDimensionEntities;
            if (_generateAngular3PointDimensionsCheckBox != null) _generateAngular3PointDimensionsCheckBox.IsChecked = options.GenerateAngular3PointDimensionEntities;
            if (_generateOrdinateDimensionsCheckBox != null) _generateOrdinateDimensionsCheckBox.IsChecked = options.GenerateOrdinateDimensionEntities;
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
            
            var allEntitiesEnabled = options.GenerateLineEntities && options.GenerateArcEntities && options.GenerateCircleEntities &&
                                    options.GenerateEllipseEntities && options.GeneratePolylineEntities && options.GenerateSplineEntities &&
                                    options.GenerateTextEntities && options.GenerateMTextEntities && options.GeneratePointEntities &&
                                    options.GenerateInsertEntities && options.GenerateHatchEntities && options.GenerateSolidEntities &&
                                    options.GenerateFace3dEntities && options.GenerateWipeoutEntities && options.GenerateLinearDimensionEntities &&
                                    options.GenerateAlignedDimensionEntities && options.GenerateRadialDimensionEntities && options.GenerateDiametricDimensionEntities &&
                                    options.GenerateAngular2LineDimensionEntities && options.GenerateAngular3PointDimensionEntities && options.GenerateOrdinateDimensionEntities &&
                                    options.GenerateLeaderEntities && options.GenerateMLineEntities && options.GenerateRayEntities &&
                                    options.GenerateXLineEntities && options.GenerateImageEntities && options.GenerateMeshEntities &&
                                    options.GeneratePolyfaceMeshEntities && options.GeneratePolygonMeshEntities && options.GenerateShapeEntities &&
                                    options.GenerateToleranceEntities && options.GenerateTraceEntities && options.GenerateUnderlayEntities &&
                                    options.GenerateViewportEntities;
            if (_generateEntitiesCheckBox != null) _generateEntitiesCheckBox.IsChecked = allEntitiesEnabled;
            
            if (_generateSaveCommentCheckBox != null) _generateSaveCommentCheckBox.IsChecked = options.GenerateSaveComment;
            if (_generateReturnStatementCheckBox != null) _generateReturnStatementCheckBox.IsChecked = options.GenerateReturnStatement;
        }
        finally
        {
            _isHandlingEvents = wasHandlingEvents;
        }
            
        // Raise options changed event after setting all options
        if (_isHandlingEvents)
        {
            RaiseOptionsChanged();
        }
    }
        
    private void OnEntitiesCheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        var wasHandlingEvents = _isHandlingEvents;
        _isHandlingEvents = false;
            
        try
        {
            var isChecked = _generateEntitiesCheckBox?.IsChecked ?? false;
                
            // Update all entity checkboxes based on master checkbox state
            var entityCheckboxes = new[]
            {
                _generateLinesCheckBox, _generateArcsCheckBox, _generateCirclesCheckBox, _generateEllipsesCheckBox,
                _generatePolylines2DCheckBox, _generatePolylines3DCheckBox, _generateLwPolylinesCheckBox,
                _generateSplinesCheckBox, _generateTextsCheckBox, _generateMTextsCheckBox, _generatePointsCheckBox,
                _generateInsertCheckBox, _generateHatchesCheckBox, _generateSolidsCheckBox, _generateFacesCheckBox,
                _generateWipeoutsCheckBox, _generateLinearDimensionsCheckBox,
                _generateAlignedDimensionsCheckBox, _generateRadialDimensionsCheckBox, _generateDiametricDimensionsCheckBox,
                _generateAngular2LineDimensionsCheckBox, _generateAngular3PointDimensionsCheckBox, _generateOrdinateDimensionsCheckBox,
                _generateLeadersCheckBox, _generateMlinesCheckBox, _generateRaysCheckBox, _generateXlinesCheckBox,
                _generateImagesCheckBox, _generateMeshesCheckBox, _generatePolyfaceMeshesCheckBox, _generatePolygonMeshesCheckBox,
                _generateShapesCheckBox, _generateTolerancesCheckBox, _generateTracesCheckBox, _generateUnderlaysCheckBox,
                _generateViewportsCheckBox
            };
                
            foreach (var checkbox in entityCheckboxes)
            {
                if (checkbox != null)
                {
                    checkbox.IsChecked = isChecked;
                    checkbox.IsEnabled = isChecked;
                }
            }
        }
        finally
        {
            _isHandlingEvents = wasHandlingEvents;
        }
            
        // Always raise options changed event after changing checkbox states
        RaiseOptionsChanged();
    }
        
    private void OnObjectsCheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        var wasHandlingEvents = _isHandlingEvents;
        _isHandlingEvents = false;
            
        try
        {
            var isChecked = _generateObjectsCheckBox?.IsChecked ?? false;
                
            // Update all object checkboxes based on master checkbox state
            if (_generateGroupsCheckBox != null) { _generateGroupsCheckBox.IsChecked = isChecked; _generateGroupsCheckBox.IsEnabled = isChecked; }
            if (_generateLayoutsCheckBox != null) { _generateLayoutsCheckBox.IsChecked = isChecked; _generateLayoutsCheckBox.IsEnabled = isChecked; }
            if (_generateImageDefinitionsCheckBox != null) { _generateImageDefinitionsCheckBox.IsChecked = isChecked; _generateImageDefinitionsCheckBox.IsEnabled = isChecked; }
            if (_generateUnderlayDefinitionsCheckBox != null) { _generateUnderlayDefinitionsCheckBox.IsChecked = isChecked; _generateUnderlayDefinitionsCheckBox.IsEnabled = isChecked; }
            if (_generateMLineEntitiesCheckBox != null) { _generateMLineEntitiesCheckBox.IsChecked = isChecked; _generateMLineEntitiesCheckBox.IsEnabled = isChecked; }
            if (_generateXRecordObjectsCheckBox != null) { _generateXRecordObjectsCheckBox.IsChecked = isChecked; _generateXRecordObjectsCheckBox.IsEnabled = isChecked; }
            if (_generateDictionaryObjectsCheckBox != null) { _generateDictionaryObjectsCheckBox.IsChecked = isChecked; _generateDictionaryObjectsCheckBox.IsEnabled = isChecked; }
            if (_generateRasterVariablesObjectsCheckBox != null) { _generateRasterVariablesObjectsCheckBox.IsChecked = isChecked; _generateRasterVariablesObjectsCheckBox.IsEnabled = isChecked; }
        }
        finally
        {
            _isHandlingEvents = wasHandlingEvents;
        }
            
        // Always raise options changed event after changing checkbox states
        RaiseOptionsChanged();
    }
        
    private void OnTablesCheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        var wasHandlingEvents = _isHandlingEvents;
        _isHandlingEvents = false;
            
        try
        {
            var isChecked = _generateTablesCheckBox?.IsChecked ?? false;
                
            // Update all table checkboxes based on master checkbox state
            if (_generateLayersCheckBox != null) { _generateLayersCheckBox.IsChecked = isChecked; _generateLayersCheckBox.IsEnabled = isChecked; }
            if (_generateLinetypesCheckBox != null) { _generateLinetypesCheckBox.IsChecked = isChecked; _generateLinetypesCheckBox.IsEnabled = isChecked; }
            if (_generateTextStylesCheckBox != null) { _generateTextStylesCheckBox.IsChecked = isChecked; _generateTextStylesCheckBox.IsEnabled = isChecked; }
            if (_generateBlocksCheckBox != null) { _generateBlocksCheckBox.IsChecked = isChecked; _generateBlocksCheckBox.IsEnabled = isChecked; }
            if (_generateDimensionStylesCheckBox != null) { _generateDimensionStylesCheckBox.IsChecked = isChecked; _generateDimensionStylesCheckBox.IsEnabled = isChecked; }
            if (_generateMLineStylesCheckBox != null) { _generateMLineStylesCheckBox.IsChecked = isChecked; _generateMLineStylesCheckBox.IsEnabled = isChecked; }
        }
        finally
        {
            _isHandlingEvents = wasHandlingEvents;
        }
            
        // Always raise options changed event after changing checkbox states
        RaiseOptionsChanged();
    }
}
