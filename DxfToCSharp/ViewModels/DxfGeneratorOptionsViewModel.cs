using System;
using System.Reactive.Linq;
using System.Windows.Input;
using DxfToCSharp.Core;
using ReactiveUI;

namespace DxfToCSharp.ViewModels;

public class DxfGeneratorOptionsViewModel : ReactiveObject
{
    // General options
    private bool _generateHeader = true;
    public bool GenerateHeader { get => _generateHeader; set => this.RaiseAndSetIfChanged(ref _generateHeader, value); }

    private bool _generateHeaderVariables = true;

    public bool GenerateHeaderVariables
    {
        get => _generateHeaderVariables;
        set => this.RaiseAndSetIfChanged(ref _generateHeaderVariables, value);
    }

    private bool _generateUsingStatements = true;

    public bool GenerateUsingStatements
    {
        get => _generateUsingStatements;
        set => this.RaiseAndSetIfChanged(ref _generateUsingStatements, value);
    }

    private bool _generateDetailedComments = false;

    public bool GenerateDetailedComments
    {
        get => _generateDetailedComments;
        set => this.RaiseAndSetIfChanged(ref _generateDetailedComments, value);
    }

    private bool _generateClass = true;
    public bool GenerateClass { get => _generateClass; set => this.RaiseAndSetIfChanged(ref _generateClass, value); }

    // Tables options
    private bool _generateLayers = true;
    public bool GenerateLayers { get => _generateLayers; set => this.RaiseAndSetIfChanged(ref _generateLayers, value); }

    private bool _generateLinetypes = true;

    public bool GenerateLinetypes
    {
        get => _generateLinetypes;
        set => this.RaiseAndSetIfChanged(ref _generateLinetypes, value);
    }

    private bool _generateTextStyles = true;

    public bool GenerateTextStyles
    {
        get => _generateTextStyles;
        set => this.RaiseAndSetIfChanged(ref _generateTextStyles, value);
    }

    private bool _generateBlocks = true;
    public bool GenerateBlocks { get => _generateBlocks; set => this.RaiseAndSetIfChanged(ref _generateBlocks, value); }

    private bool _generateDimensionStyles = true;

    public bool GenerateDimensionStyles
    {
        get => _generateDimensionStyles;
        set => this.RaiseAndSetIfChanged(ref _generateDimensionStyles, value);
    }

    private bool _generateMLineStyles = true;

    public bool GenerateMLineStyles
    {
        get => _generateMLineStyles;
        set => this.RaiseAndSetIfChanged(ref _generateMLineStyles, value);
    }

    // Objects options
    private bool _generateGroupObjects = true;

    public bool GenerateGroupObjects
    {
        get => _generateGroupObjects;
        set => this.RaiseAndSetIfChanged(ref _generateGroupObjects, value);
    }

    private bool _generateLayoutObjects = true;

    public bool GenerateLayoutObjects
    {
        get => _generateLayoutObjects;
        set => this.RaiseAndSetIfChanged(ref _generateLayoutObjects, value);
    }

    private bool _generateImageDefinitionObjects = true;

    public bool GenerateImageDefinitionObjects
    {
        get => _generateImageDefinitionObjects;
        set => this.RaiseAndSetIfChanged(ref _generateImageDefinitionObjects, value);
    }

    private bool _generateUnderlayDefinitionObjects = true;

    public bool GenerateUnderlayDefinitionObjects
    {
        get => _generateUnderlayDefinitionObjects;
        set => this.RaiseAndSetIfChanged(ref _generateUnderlayDefinitionObjects, value);
    }

    private bool _generateXRecordObjects = true;

    public bool GenerateXRecordObjects
    {
        get => _generateXRecordObjects;
        set => this.RaiseAndSetIfChanged(ref _generateXRecordObjects, value);
    }

    private bool _generateDictionaryObjects = true;

    public bool GenerateDictionaryObjects
    {
        get => _generateDictionaryObjects;
        set => this.RaiseAndSetIfChanged(ref _generateDictionaryObjects, value);
    }

    private bool _generateRasterVariablesObjects = true;

    public bool GenerateRasterVariablesObjects
    {
        get => _generateRasterVariablesObjects;
        set => this.RaiseAndSetIfChanged(ref _generateRasterVariablesObjects, value);
    }

    private bool _generateLayerStateObjects = true;

    public bool GenerateLayerStateObjects
    {
        get => _generateLayerStateObjects;
        set => this.RaiseAndSetIfChanged(ref _generateLayerStateObjects, value);
    }

    private bool _generatePlotSettingsObjects = true;

    public bool GeneratePlotSettingsObjects
    {
        get => _generatePlotSettingsObjects;
        set => this.RaiseAndSetIfChanged(ref _generatePlotSettingsObjects, value);
    }

    private bool _generateMLineStyleObjects = true;

    public bool GenerateMLineStyleObjects
    {
        get => _generateMLineStyleObjects;
        set => this.RaiseAndSetIfChanged(ref _generateMLineStyleObjects, value);
    }

    private bool _generateApplicationRegistryObjects = true;

    public bool GenerateApplicationRegistryObjects
    {
        get => _generateApplicationRegistryObjects;
        set => this.RaiseAndSetIfChanged(ref _generateApplicationRegistryObjects, value);
    }

    // Note: View objects are not supported as the Views collection is internal in netDxf

    private bool _generateShapeStyleObjects = true;

    public bool GenerateShapeStyleObjects
    {
        get => _generateShapeStyleObjects;
        set => this.RaiseAndSetIfChanged(ref _generateShapeStyleObjects, value);
    }

    // Entity options
    private bool _generateLineEntities = true;

    public bool GenerateLineEntities
    {
        get => _generateLineEntities;
        set => this.RaiseAndSetIfChanged(ref _generateLineEntities, value);
    }

    private bool _generateArcEntities = true;

    public bool GenerateArcEntities
    {
        get => _generateArcEntities;
        set => this.RaiseAndSetIfChanged(ref _generateArcEntities, value);
    }

    private bool _generateAttributeDefinitionEntities = true;

    public bool GenerateAttributeDefinitionEntities
    {
        get => _generateAttributeDefinitionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateAttributeDefinitionEntities, value);
    }

    private bool _generateCircleEntities = true;

    public bool GenerateCircleEntities
    {
        get => _generateCircleEntities;
        set => this.RaiseAndSetIfChanged(ref _generateCircleEntities, value);
    }

    private bool _generateEllipseEntities = true;

    public bool GenerateEllipseEntities
    {
        get => _generateEllipseEntities;
        set => this.RaiseAndSetIfChanged(ref _generateEllipseEntities, value);
    }

    private bool _generatePolylineEntities = true;

    public bool GeneratePolylineEntities
    {
        get => _generatePolylineEntities;
        set => this.RaiseAndSetIfChanged(ref _generatePolylineEntities, value);
    }

    private bool _generatePolyline2DEntities = true;

    public bool GeneratePolyline2DEntities
    {
        get => _generatePolyline2DEntities;
        set => this.RaiseAndSetIfChanged(ref _generatePolyline2DEntities, value);
    }

    private bool _generatePolyline3DEntities = true;

    public bool GeneratePolyline3DEntities
    {
        get => _generatePolyline3DEntities;
        set => this.RaiseAndSetIfChanged(ref _generatePolyline3DEntities, value);
    }

    private bool _generateSplineEntities = true;

    public bool GenerateSplineEntities
    {
        get => _generateSplineEntities;
        set => this.RaiseAndSetIfChanged(ref _generateSplineEntities, value);
    }

    private bool _generateTextEntities = true;

    public bool GenerateTextEntities
    {
        get => _generateTextEntities;
        set => this.RaiseAndSetIfChanged(ref _generateTextEntities, value);
    }

    private bool _generateMTextEntities = true;

    public bool GenerateMTextEntities
    {
        get => _generateMTextEntities;
        set => this.RaiseAndSetIfChanged(ref _generateMTextEntities, value);
    }

    private bool _generatePointEntities = true;

    public bool GeneratePointEntities
    {
        get => _generatePointEntities;
        set => this.RaiseAndSetIfChanged(ref _generatePointEntities, value);
    }

    private bool _generateAttributeEntities = true;

    public bool GenerateAttributeEntities
    {
        get => _generateAttributeEntities;
        set => this.RaiseAndSetIfChanged(ref _generateAttributeEntities, value);
    }

    private bool _generateInsertEntities = true;

    public bool GenerateInsertEntities
    {
        get => _generateInsertEntities;
        set => this.RaiseAndSetIfChanged(ref _generateInsertEntities, value);
    }

    private bool _generateHatchEntities = true;

    public bool GenerateHatchEntities
    {
        get => _generateHatchEntities;
        set => this.RaiseAndSetIfChanged(ref _generateHatchEntities, value);
    }

    private bool _generateSolidEntities = true;

    public bool GenerateSolidEntities
    {
        get => _generateSolidEntities;
        set => this.RaiseAndSetIfChanged(ref _generateSolidEntities, value);
    }

    private bool _generateFace3dEntities = true;

    public bool GenerateFace3dEntities
    {
        get => _generateFace3dEntities;
        set => this.RaiseAndSetIfChanged(ref _generateFace3dEntities, value);
    }

    private bool _generateWipeoutEntities = true;

    public bool GenerateWipeoutEntities
    {
        get => _generateWipeoutEntities;
        set => this.RaiseAndSetIfChanged(ref _generateWipeoutEntities, value);
    }

    private bool _generateLinearDimensionEntities = true;

    public bool GenerateLinearDimensionEntities
    {
        get => _generateLinearDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateLinearDimensionEntities, value);
    }

    private bool _generateAlignedDimensionEntities = true;

    public bool GenerateAlignedDimensionEntities
    {
        get => _generateAlignedDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateAlignedDimensionEntities, value);
    }

    private bool _generateRadialDimensionEntities = true;

    public bool GenerateRadialDimensionEntities
    {
        get => _generateRadialDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateRadialDimensionEntities, value);
    }

    private bool _generateDiametricDimensionEntities = true;

    public bool GenerateDiametricDimensionEntities
    {
        get => _generateDiametricDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateDiametricDimensionEntities, value);
    }

    private bool _generateAngular2LineDimensionEntities = true;

    public bool GenerateAngular2LineDimensionEntities
    {
        get => _generateAngular2LineDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateAngular2LineDimensionEntities, value);
    }

    private bool _generateAngular3PointDimensionEntities = true;

    public bool GenerateAngular3PointDimensionEntities
    {
        get => _generateAngular3PointDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateAngular3PointDimensionEntities, value);
    }

    private bool _generateOrdinateDimensionEntities = true;

    public bool GenerateOrdinateDimensionEntities
    {
        get => _generateOrdinateDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateOrdinateDimensionEntities, value);
    }

    private bool _generateArcLengthDimensionEntities = true;

    public bool GenerateArcLengthDimensionEntities
    {
        get => _generateArcLengthDimensionEntities;
        set => this.RaiseAndSetIfChanged(ref _generateArcLengthDimensionEntities, value);
    }

    private bool _generateLeaderEntities = true;

    public bool GenerateLeaderEntities
    {
        get => _generateLeaderEntities;
        set => this.RaiseAndSetIfChanged(ref _generateLeaderEntities, value);
    }

    private bool _generateMLineEntities = true;

    public bool GenerateMLineEntities
    {
        get => _generateMLineEntities;
        set => this.RaiseAndSetIfChanged(ref _generateMLineEntities, value);
    }

    private bool _generateRayEntities = true;

    public bool GenerateRayEntities
    {
        get => _generateRayEntities;
        set => this.RaiseAndSetIfChanged(ref _generateRayEntities, value);
    }

    private bool _generateXLineEntities = true;

    public bool GenerateXLineEntities
    {
        get => _generateXLineEntities;
        set => this.RaiseAndSetIfChanged(ref _generateXLineEntities, value);
    }

    private bool _generateImageEntities = true;

    public bool GenerateImageEntities
    {
        get => _generateImageEntities;
        set => this.RaiseAndSetIfChanged(ref _generateImageEntities, value);
    }

    private bool _generateMeshEntities = true;

    public bool GenerateMeshEntities
    {
        get => _generateMeshEntities;
        set => this.RaiseAndSetIfChanged(ref _generateMeshEntities, value);
    }

    private bool _generatePolyfaceMeshEntities = true;

    public bool GeneratePolyfaceMeshEntities
    {
        get => _generatePolyfaceMeshEntities;
        set => this.RaiseAndSetIfChanged(ref _generatePolyfaceMeshEntities, value);
    }

    private bool _generatePolygonMeshEntities = true;

    public bool GeneratePolygonMeshEntities
    {
        get => _generatePolygonMeshEntities;
        set => this.RaiseAndSetIfChanged(ref _generatePolygonMeshEntities, value);
    }

    private bool _generateShapeEntities = true;

    public bool GenerateShapeEntities
    {
        get => _generateShapeEntities;
        set => this.RaiseAndSetIfChanged(ref _generateShapeEntities, value);
    }

    private bool _generateToleranceEntities = true;

    public bool GenerateToleranceEntities
    {
        get => _generateToleranceEntities;
        set => this.RaiseAndSetIfChanged(ref _generateToleranceEntities, value);
    }

    private bool _generateTraceEntities = true;

    public bool GenerateTraceEntities
    {
        get => _generateTraceEntities;
        set => this.RaiseAndSetIfChanged(ref _generateTraceEntities, value);
    }

    private bool _generateUnderlayEntities = true;

    public bool GenerateUnderlayEntities
    {
        get => _generateUnderlayEntities;
        set => this.RaiseAndSetIfChanged(ref _generateUnderlayEntities, value);
    }

    private bool _generateViewportEntities = true;

    public bool GenerateViewportEntities
    {
        get => _generateViewportEntities;
        set => this.RaiseAndSetIfChanged(ref _generateViewportEntities, value);
    }

    // Computed observables for master checkboxes
    private readonly ObservableAsPropertyHelper<bool> _allGeneralSelected;
    public bool AllGeneralSelected => _allGeneralSelected.Value;

    private readonly ObservableAsPropertyHelper<bool> _allTablesSelected;
    public bool AllTablesSelected => _allTablesSelected.Value;

    private readonly ObservableAsPropertyHelper<bool> _allObjectsSelected;
    public bool AllObjectsSelected => _allObjectsSelected.Value;

    private readonly ObservableAsPropertyHelper<bool> _allEntitiesSelected;
    public bool AllEntitiesSelected => _allEntitiesSelected.Value;

    // Commands
    public ICommand ToggleAllGeneralCommand { get; }
    public ICommand ToggleAllTablesCommand { get; }
    public ICommand ToggleAllObjectsCommand { get; }
    public ICommand ToggleAllEntitiesCommand { get; }

    // Observable for options changed
    public IObservable<DxfCodeGenerationOptions> OptionsChanged { get; }

    public DxfGeneratorOptionsViewModel()
    {
        // Create computed observables for master checkboxes
        _allGeneralSelected = this.WhenAnyValue(
                x => x.GenerateHeader,
                x => x.GenerateUsingStatements,
                x => x.GenerateHeaderVariables,
                x => x.GenerateDetailedComments,
                x => x.GenerateClass,
                (header, usingStmts, headerVars, detailedComments, generateClass) =>
                    header && usingStmts && headerVars && detailedComments && generateClass)
            .ToProperty(this, x => x.AllGeneralSelected);

        _allTablesSelected = this.WhenAnyValue(
                x => x.GenerateLayers,
                x => x.GenerateLinetypes,
                x => x.GenerateTextStyles,
                x => x.GenerateBlocks,
                x => x.GenerateDimensionStyles,
                x => x.GenerateMLineStyles,
                (layers, linetypes, textStyles, blocks, dimStyles, mlineStyles) =>
                    layers && linetypes && textStyles && blocks && dimStyles && mlineStyles)
            .ToProperty(this, x => x.AllTablesSelected);

        _allObjectsSelected = this.WhenAnyValue(
                x => x.GenerateGroupObjects,
                x => x.GenerateLayoutObjects,
                x => x.GenerateImageDefinitionObjects,
                x => x.GenerateUnderlayDefinitionObjects,
                x => x.GenerateXRecordObjects,
                x => x.GenerateDictionaryObjects,
                x => x.GenerateRasterVariablesObjects,
                x => x.GenerateLayerStateObjects,
                x => x.GeneratePlotSettingsObjects,
                x => x.GenerateMLineStyleObjects,
                x => x.GenerateApplicationRegistryObjects,
                // View objects not supported (internal API)
                x => x.GenerateShapeStyleObjects,
                (groups, layouts, imageDefs, underlayDefs, xrecords, dictionaries, rasterVars, layerStates, plotSettings, mlineStyles, appRegs, shapeStyles) =>
                    groups && layouts && imageDefs && underlayDefs && xrecords && dictionaries && rasterVars && layerStates && plotSettings && mlineStyles && appRegs && shapeStyles)
            .ToProperty(this, x => x.AllObjectsSelected);

        var entitiesGroup1a = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateLineEntities),
            this.WhenAnyValue(x => x.GenerateArcEntities),
            this.WhenAnyValue(x => x.GenerateAttributeDefinitionEntities),
            this.WhenAnyValue(x => x.GenerateCircleEntities),
            this.WhenAnyValue(x => x.GenerateEllipseEntities),
            this.WhenAnyValue(x => x.GeneratePolylineEntities),
            this.WhenAnyValue(x => x.GeneratePolyline2DEntities),
            this.WhenAnyValue(x => x.GeneratePolyline3DEntities),
            this.WhenAnyValue(x => x.GenerateSplineEntities),
            this.WhenAnyValue(x => x.GenerateTextEntities),
            this.WhenAnyValue(x => x.GenerateMTextEntities),
            this.WhenAnyValue(x => x.GeneratePointEntities),
            (line, arc, attributeDefinition, circle, ellipse, polyline, polyline2D, polyline3D, spline, text, mtext, point) =>
                line && arc && attributeDefinition && circle && ellipse && polyline && polyline2D && polyline3D && spline && text && mtext && point);

        var entitiesGroup1b = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateAttributeEntities),
            this.WhenAnyValue(x => x.GenerateInsertEntities),
            this.WhenAnyValue(x => x.GenerateHatchEntities),
            this.WhenAnyValue(x => x.GenerateSolidEntities),
            this.WhenAnyValue(x => x.GenerateFace3dEntities),
            this.WhenAnyValue(x => x.GenerateWipeoutEntities),
            this.WhenAnyValue(x => x.GenerateLinearDimensionEntities),
            this.WhenAnyValue(x => x.GenerateAlignedDimensionEntities),
            (attribute, insert, hatch, solid, face3d, wipeout, linearDim, alignedDim) =>
                attribute && insert && hatch && solid && face3d && wipeout && linearDim && alignedDim);

        var entitiesGroup1 = Observable.CombineLatest(
            entitiesGroup1a,
            entitiesGroup1b,
            (group1a, group1b) => group1a && group1b);

        var entitiesGroup2a = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateRadialDimensionEntities),
            this.WhenAnyValue(x => x.GenerateDiametricDimensionEntities),
            this.WhenAnyValue(x => x.GenerateAngular2LineDimensionEntities),
            this.WhenAnyValue(x => x.GenerateAngular3PointDimensionEntities),
            this.WhenAnyValue(x => x.GenerateOrdinateDimensionEntities),
            this.WhenAnyValue(x => x.GenerateArcLengthDimensionEntities),
            this.WhenAnyValue(x => x.GenerateLeaderEntities),
            this.WhenAnyValue(x => x.GenerateMLineEntities),
            (radialDim, diametricDim, angular2LineDim, angular3PointDim, ordinateDim, arcLengthDim, leader, mline) =>
                radialDim && diametricDim && angular2LineDim && angular3PointDim && ordinateDim && arcLengthDim && leader && mline);

        var entitiesGroup2b = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateRayEntities),
            this.WhenAnyValue(x => x.GenerateXLineEntities),
            this.WhenAnyValue(x => x.GenerateImageEntities),
            this.WhenAnyValue(x => x.GenerateMeshEntities),
            this.WhenAnyValue(x => x.GeneratePolyfaceMeshEntities),
            this.WhenAnyValue(x => x.GeneratePolygonMeshEntities),
            this.WhenAnyValue(x => x.GenerateShapeEntities),
            this.WhenAnyValue(x => x.GenerateToleranceEntities),
            this.WhenAnyValue(x => x.GenerateTraceEntities),
            (ray, xline, image, mesh, polyfaceMesh, polygonMesh, shape, tolerance, trace) =>
                ray && xline && image && mesh && polyfaceMesh && polygonMesh && shape && tolerance && trace);

        var entitiesGroup2 = Observable.CombineLatest(
            entitiesGroup2a,
            entitiesGroup2b,
            (group2a, group2b) => group2a && group2b);

        var entitiesGroup3 = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateUnderlayEntities),
            this.WhenAnyValue(x => x.GenerateViewportEntities),
            (underlay, viewport) => underlay && viewport);

        _allEntitiesSelected = Observable.CombineLatest(
                entitiesGroup1,
                entitiesGroup2,
                entitiesGroup3,
                (group1, group2, group3) => group1 && group2 && group3)
            .ToProperty(this, x => x.AllEntitiesSelected);

        // Create commands for master checkboxes
        ToggleAllGeneralCommand = ReactiveCommand.Create<bool>(ToggleAllGeneral);
        ToggleAllTablesCommand = ReactiveCommand.Create<bool>(ToggleAllTables);
        ToggleAllObjectsCommand = ReactiveCommand.Create<bool>(ToggleAllObjects);
        ToggleAllEntitiesCommand = ReactiveCommand.Create<bool>(ToggleAllEntities);

        // Create observable for options changed
        var generalOptions = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateHeader),
            this.WhenAnyValue(x => x.GenerateHeaderVariables),
            this.WhenAnyValue(x => x.GenerateUsingStatements),
            this.WhenAnyValue(x => x.GenerateDetailedComments),
            this.WhenAnyValue(x => x.GenerateClass),
            (header, headerVars, usingStmts, detailedComments, generateClass) => true);

        var tableOptions = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateLayers),
            this.WhenAnyValue(x => x.GenerateLinetypes),
            this.WhenAnyValue(x => x.GenerateTextStyles),
            this.WhenAnyValue(x => x.GenerateBlocks),
            this.WhenAnyValue(x => x.GenerateDimensionStyles),
            this.WhenAnyValue(x => x.GenerateMLineStyles),
            (layers, linetypes, textStyles, blocks, dimStyles, mlineStyles) => true);

        var objectOptions = Observable.CombineLatest(
            this.WhenAnyValue(x => x.GenerateGroupObjects),
            this.WhenAnyValue(x => x.GenerateLayoutObjects),
            this.WhenAnyValue(x => x.GenerateImageDefinitionObjects),
            this.WhenAnyValue(x => x.GenerateUnderlayDefinitionObjects),
            this.WhenAnyValue(x => x.GenerateXRecordObjects),
            this.WhenAnyValue(x => x.GenerateDictionaryObjects),
            this.WhenAnyValue(x => x.GenerateRasterVariablesObjects),
            this.WhenAnyValue(x => x.GenerateLayerStateObjects),
            this.WhenAnyValue(x => x.GeneratePlotSettingsObjects),
            this.WhenAnyValue(x => x.GenerateMLineStyleObjects),
            this.WhenAnyValue(x => x.GenerateApplicationRegistryObjects),
            // View objects not supported (internal API)
            this.WhenAnyValue(x => x.GenerateShapeStyleObjects),
            (groups, layouts, imageDefs, underlayDefs, xrecords, dictionaries, rasterVars, layerStates, plotSettings, mlineStyles, appRegs, shapeStyles) => true);

        OptionsChanged = Observable.CombineLatest(
                generalOptions,
                tableOptions,
                objectOptions,
                entitiesGroup1,
                entitiesGroup2,
                entitiesGroup3,
                (general, tables, objects, entities1, entities2, entities3) => true)
            .Select(_ => ToOptions());
    }

    private void ToggleAllGeneral(bool value)
    {
        GenerateHeader = value;
        GenerateUsingStatements = value;
        GenerateHeaderVariables = value;
        GenerateDetailedComments = value;
        GenerateClass = value;
    }

    private void ToggleAllTables(bool value)
    {
        GenerateLayers = value;
        GenerateLinetypes = value;
        GenerateTextStyles = value;
        GenerateBlocks = value;
        GenerateDimensionStyles = value;
        GenerateMLineStyles = value;
    }

    private void ToggleAllObjects(bool value)
    {
        GenerateGroupObjects = value;
        GenerateLayoutObjects = value;
        GenerateImageDefinitionObjects = value;
        GenerateUnderlayDefinitionObjects = value;
        GenerateXRecordObjects = value;
        GenerateDictionaryObjects = value;
        GenerateRasterVariablesObjects = value;
        GenerateLayerStateObjects = value;
        GeneratePlotSettingsObjects = value;
        GenerateMLineStyleObjects = value;
        GenerateApplicationRegistryObjects = value;
        // View objects not supported (internal API)
        GenerateShapeStyleObjects = value;
    }

    private void ToggleAllEntities(bool value)
    {
        GenerateLineEntities = value;
        GenerateArcEntities = value;
        GenerateAttributeDefinitionEntities = value;
        GenerateCircleEntities = value;
        GenerateEllipseEntities = value;
        GeneratePolylineEntities = value;
        GeneratePolyline2DEntities = value;
        GeneratePolyline3DEntities = value;
        GenerateSplineEntities = value;
        GenerateTextEntities = value;
        GenerateMTextEntities = value;
        GeneratePointEntities = value;
        GenerateAttributeEntities = value;
        GenerateInsertEntities = value;
        GenerateHatchEntities = value;
        GenerateSolidEntities = value;
        GenerateFace3dEntities = value;
        GenerateWipeoutEntities = value;
        GenerateLinearDimensionEntities = value;
        GenerateAlignedDimensionEntities = value;
        GenerateRadialDimensionEntities = value;
        GenerateDiametricDimensionEntities = value;
        GenerateAngular2LineDimensionEntities = value;
        GenerateAngular3PointDimensionEntities = value;
        GenerateOrdinateDimensionEntities = value;
        GenerateArcLengthDimensionEntities = value;
        GenerateLeaderEntities = value;
        GenerateMLineEntities = value;
        GenerateRayEntities = value;
        GenerateXLineEntities = value;
        GenerateImageEntities = value;
        GenerateMeshEntities = value;
        GeneratePolyfaceMeshEntities = value;
        GeneratePolygonMeshEntities = value;
        GenerateShapeEntities = value;
        GenerateToleranceEntities = value;
        GenerateTraceEntities = value;
        GenerateUnderlayEntities = value;
        GenerateViewportEntities = value;
    }

    public DxfCodeGenerationOptions ToOptions()
    {
        return new DxfCodeGenerationOptions
        {
            GenerateHeader = GenerateHeader,
            GenerateHeaderVariables = GenerateHeaderVariables,
            GenerateUsingStatements = GenerateUsingStatements,
            GenerateDetailedComments = GenerateDetailedComments,
            GenerateClass = GenerateClass,
            GenerateLayers = GenerateLayers,
            GenerateLinetypes = GenerateLinetypes,
            GenerateTextStyles = GenerateTextStyles,
            GenerateBlocks = GenerateBlocks,
            GenerateDimensionStyles = GenerateDimensionStyles,
            GenerateMLineStyles = GenerateMLineStyles,
            GenerateGroupObjects = GenerateGroupObjects,
            GenerateLayoutObjects = GenerateLayoutObjects,
            GenerateImageDefinitionObjects = GenerateImageDefinitionObjects,
            GenerateUnderlayDefinitionObjects = GenerateUnderlayDefinitionObjects,
            GenerateXRecordObjects = GenerateXRecordObjects,
            GenerateDictionaryObjects = GenerateDictionaryObjects,
            GenerateRasterVariablesObjects = GenerateRasterVariablesObjects,
            GenerateLayerStateObjects = GenerateLayerStateObjects,
            GeneratePlotSettingsObjects = GeneratePlotSettingsObjects,
            GenerateMLineStyleObjects = GenerateMLineStyleObjects,
            GenerateApplicationRegistryObjects = GenerateApplicationRegistryObjects,
            // View objects not supported (internal API)
            GenerateShapeStyleObjects = GenerateShapeStyleObjects,
            GenerateLineEntities = GenerateLineEntities,
            GenerateArcEntities = GenerateArcEntities,
            GenerateAttributeDefinitionEntities = GenerateAttributeDefinitionEntities,
            GenerateCircleEntities = GenerateCircleEntities,
            GenerateEllipseEntities = GenerateEllipseEntities,
            GeneratePolylineEntities = GeneratePolylineEntities,
            GeneratePolyline2DEntities = GeneratePolyline2DEntities,
            GeneratePolyline3DEntities = GeneratePolyline3DEntities,
            GenerateSplineEntities = GenerateSplineEntities,
            GenerateTextEntities = GenerateTextEntities,
            GenerateMTextEntities = GenerateMTextEntities,
            GeneratePointEntities = GeneratePointEntities,
            GenerateAttributeEntities = GenerateAttributeEntities,
            GenerateInsertEntities = GenerateInsertEntities,
            GenerateHatchEntities = GenerateHatchEntities,
            GenerateSolidEntities = GenerateSolidEntities,
            GenerateFace3dEntities = GenerateFace3dEntities,
            GenerateWipeoutEntities = GenerateWipeoutEntities,
            GenerateLinearDimensionEntities = GenerateLinearDimensionEntities,
            GenerateAlignedDimensionEntities = GenerateAlignedDimensionEntities,
            GenerateRadialDimensionEntities = GenerateRadialDimensionEntities,
            GenerateDiametricDimensionEntities = GenerateDiametricDimensionEntities,
            GenerateAngular2LineDimensionEntities = GenerateAngular2LineDimensionEntities,
            GenerateAngular3PointDimensionEntities = GenerateAngular3PointDimensionEntities,
            GenerateOrdinateDimensionEntities = GenerateOrdinateDimensionEntities,
            GenerateArcLengthDimensionEntities = GenerateArcLengthDimensionEntities,
            GenerateLeaderEntities = GenerateLeaderEntities,
            GenerateMLineEntities = GenerateMLineEntities,
            GenerateRayEntities = GenerateRayEntities,
            GenerateXLineEntities = GenerateXLineEntities,
            GenerateImageEntities = GenerateImageEntities,
            GenerateMeshEntities = GenerateMeshEntities,
            GeneratePolyfaceMeshEntities = GeneratePolyfaceMeshEntities,
            GeneratePolygonMeshEntities = GeneratePolygonMeshEntities,
            GenerateShapeEntities = GenerateShapeEntities,
            GenerateToleranceEntities = GenerateToleranceEntities,
            GenerateTraceEntities = GenerateTraceEntities,
            GenerateUnderlayEntities = GenerateUnderlayEntities,
            GenerateViewportEntities = GenerateViewportEntities
        };
    }

    public void FromOptions(DxfCodeGenerationOptions options)
    {
        GenerateHeader = options.GenerateHeader;
        GenerateHeaderVariables = options.GenerateHeaderVariables;
        GenerateUsingStatements = options.GenerateUsingStatements;
        GenerateDetailedComments = options.GenerateDetailedComments;
        GenerateClass = options.GenerateClass;
        GenerateLayers = options.GenerateLayers;
        GenerateLinetypes = options.GenerateLinetypes;
        GenerateTextStyles = options.GenerateTextStyles;
        GenerateBlocks = options.GenerateBlocks;
        GenerateDimensionStyles = options.GenerateDimensionStyles;
        GenerateMLineStyles = options.GenerateMLineStyles;
        GenerateGroupObjects = options.GenerateGroupObjects;
        GenerateLayoutObjects = options.GenerateLayoutObjects;
        GenerateImageDefinitionObjects = options.GenerateImageDefinitionObjects;
        GenerateUnderlayDefinitionObjects = options.GenerateUnderlayDefinitionObjects;
        GenerateXRecordObjects = options.GenerateXRecordObjects;
        GenerateDictionaryObjects = options.GenerateDictionaryObjects;
        GenerateRasterVariablesObjects = options.GenerateRasterVariablesObjects;
        GenerateLayerStateObjects = options.GenerateLayerStateObjects;
        GeneratePlotSettingsObjects = options.GeneratePlotSettingsObjects;
        GenerateMLineStyleObjects = options.GenerateMLineStyleObjects;
        GenerateApplicationRegistryObjects = options.GenerateApplicationRegistryObjects;
        // View objects not supported (internal API)
        GenerateShapeStyleObjects = options.GenerateShapeStyleObjects;
        GenerateLineEntities = options.GenerateLineEntities;
        GenerateArcEntities = options.GenerateArcEntities;
        GenerateAttributeDefinitionEntities = options.GenerateAttributeDefinitionEntities;
        GenerateCircleEntities = options.GenerateCircleEntities;
        GenerateEllipseEntities = options.GenerateEllipseEntities;
        GeneratePolylineEntities = options.GeneratePolylineEntities;
        GeneratePolyline2DEntities = options.GeneratePolyline2DEntities;
        GeneratePolyline3DEntities = options.GeneratePolyline3DEntities;
        GenerateSplineEntities = options.GenerateSplineEntities;
        GenerateTextEntities = options.GenerateTextEntities;
        GenerateMTextEntities = options.GenerateMTextEntities;
        GeneratePointEntities = options.GeneratePointEntities;
        GenerateAttributeEntities = options.GenerateAttributeEntities;
        GenerateInsertEntities = options.GenerateInsertEntities;
        GenerateHatchEntities = options.GenerateHatchEntities;
        GenerateSolidEntities = options.GenerateSolidEntities;
        GenerateFace3dEntities = options.GenerateFace3dEntities;
        GenerateWipeoutEntities = options.GenerateWipeoutEntities;
        GenerateLinearDimensionEntities = options.GenerateLinearDimensionEntities;
        GenerateAlignedDimensionEntities = options.GenerateAlignedDimensionEntities;
        GenerateRadialDimensionEntities = options.GenerateRadialDimensionEntities;
        GenerateDiametricDimensionEntities = options.GenerateDiametricDimensionEntities;
        GenerateAngular2LineDimensionEntities = options.GenerateAngular2LineDimensionEntities;
        GenerateAngular3PointDimensionEntities = options.GenerateAngular3PointDimensionEntities;
        GenerateOrdinateDimensionEntities = options.GenerateOrdinateDimensionEntities;
        GenerateArcLengthDimensionEntities = options.GenerateArcLengthDimensionEntities;
        GenerateLeaderEntities = options.GenerateLeaderEntities;
        GenerateMLineEntities = options.GenerateMLineEntities;
        GenerateRayEntities = options.GenerateRayEntities;
        GenerateXLineEntities = options.GenerateXLineEntities;
        GenerateImageEntities = options.GenerateImageEntities;
        GenerateMeshEntities = options.GenerateMeshEntities;
        GeneratePolyfaceMeshEntities = options.GeneratePolyfaceMeshEntities;
        GeneratePolygonMeshEntities = options.GeneratePolygonMeshEntities;
        GenerateShapeEntities = options.GenerateShapeEntities;
        GenerateToleranceEntities = options.GenerateToleranceEntities;
        GenerateTraceEntities = options.GenerateTraceEntities;
        GenerateUnderlayEntities = options.GenerateUnderlayEntities;
        GenerateViewportEntities = options.GenerateViewportEntities;
    }
}
