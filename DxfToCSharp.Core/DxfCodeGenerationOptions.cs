namespace DxfToCSharp.Core;

/// <summary>
/// Options for controlling DXF to C# code generation
/// </summary>
public record DxfCodeGenerationOptions
{
    /// <summary>
    /// Custom class name for the generated code (null for default)
    /// </summary>
    public string? CustomClassName { get; set; }

    /// <summary>
    /// Whether to generate the class, create method and return statement
    /// </summary>
    public bool GenerateClass { get; set; } = true;

    /// <summary>
    /// Whether to include detailed comments for each entity
    /// </summary>
    public bool GenerateDetailedComments { get; set; }

    /// <summary>
    /// Whether to generate header comments with source file info and timestamp
    /// </summary>
    public bool GenerateHeader { get; set; } = true;

    /// <summary>
    /// Whether to generate using statements
    /// </summary>
    public bool GenerateUsingStatements { get; set; } = true;

    /// <summary>
    /// Whether to generate header variables (drawing variables)
    /// </summary>
    public bool GenerateHeaderVariables { get; set; } = true;

    /// <summary>
    /// Whether to generate layer definitions
    /// </summary>
    public bool GenerateLayers { get; set; } = true;

    /// <summary>
    /// Whether to generate linetype definitions
    /// </summary>
    public bool GenerateLinetypes { get; set; } = true;

    /// <summary>
    /// Whether to generate text style definitions
    /// </summary>
    public bool GenerateTextStyles { get; set; } = true;

    /// <summary>
    /// Whether to generate block definitions
    /// </summary>
    public bool GenerateBlocks { get; set; } = true;

    /// <summary>
    /// Whether to generate dimension style definitions
    /// </summary>
    public bool GenerateDimensionStyles { get; set; } = true;

    /// <summary>
    /// Whether to generate multiline style definitions
    /// </summary>
    public bool GenerateMLineStyles { get; set; } = true;

    /// <summary>
    /// Whether to generate UCS (User Coordinate System) definitions
    /// </summary>
    public bool GenerateUCS { get; set; } = true;

    /// <summary>
    /// Whether to generate VPort (Viewport) definitions
    /// </summary>
    public bool GenerateVPorts { get; set; } = true;

    /// <summary>
    /// Whether to generate Group objects
    /// </summary>
    public bool GenerateGroupObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate Layout objects
    /// </summary>
    public bool GenerateLayoutObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate ImageDefinition objects
    /// </summary>
    public bool GenerateImageDefinitionObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate UnderlayDefinition objects (PDF, DWF, DGN)
    /// </summary>
    public bool GenerateUnderlayDefinitionObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate XRecord objects
    /// </summary>
    public bool GenerateXRecordObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate DictionaryObject objects
    /// </summary>
    public bool GenerateDictionaryObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate RasterVariables objects
    /// </summary>
    public bool GenerateRasterVariablesObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate LayerState objects
    /// </summary>
    public bool GenerateLayerStateObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate PlotSettings objects
    /// </summary>
    public bool GeneratePlotSettingsObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate MLineStyle objects
    /// </summary>
    public bool GenerateMLineStyleObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate ApplicationRegistry table objects
    /// </summary>
    public bool GenerateApplicationRegistryObjects { get; set; } = true;

    // Note: View objects are not supported as the Views collection is internal in netDxf

    /// <summary>
    /// Whether to generate ShapeStyle table objects
    /// </summary>
    public bool GenerateShapeStyleObjects { get; set; } = true;

    /// <summary>
    /// Whether to generate line entities
    /// </summary>
    public bool GenerateLineEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate arc entities
    /// </summary>
    public bool GenerateArcEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate circle entities
    /// </summary>
    public bool GenerateCircleEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate polyline entities
    /// </summary>
    public bool GeneratePolylineEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate text entities
    /// </summary>
    public bool GenerateTextEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate multitext entities
    /// </summary>
    public bool GenerateMTextEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate point entities
    /// </summary>
    public bool GeneratePointEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate insert entities (block references)
    /// </summary>
    public bool GenerateInsertEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate hatch entities
    /// </summary>
    public bool GenerateHatchEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate dimension entities
    /// </summary>
    public bool GenerateDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate linear dimension entities
    /// </summary>
    public bool GenerateLinearDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate aligned dimension entities
    /// </summary>
    public bool GenerateAlignedDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate radial dimension entities
    /// </summary>
    public bool GenerateRadialDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate diametric dimension entities
    /// </summary>
    public bool GenerateDiametricDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate angular 2-line dimension entities
    /// </summary>
    public bool GenerateAngular2LineDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate angular 3-point dimension entities
    /// </summary>
    public bool GenerateAngular3PointDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate ordinate dimension entities
    /// </summary>
    public bool GenerateOrdinateDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate arc length dimension entities
    /// </summary>
    public bool GenerateArcLengthDimensionEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate leader entities
    /// </summary>
    public bool GenerateLeaderEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate spline entities
    /// </summary>
    public bool GenerateSplineEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate ellipse entities
    /// </summary>
    public bool GenerateEllipseEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate solid entities
    /// </summary>
    public bool GenerateSolidEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate face3d entities
    /// </summary>
    public bool GenerateFace3dEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate multiline entities
    /// </summary>
    public bool GenerateMLineEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate ray entities
    /// </summary>
    public bool GenerateRayEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate xline entities
    /// </summary>
    public bool GenerateXLineEntities { get; set; } = true;

    /// <summary>
    /// Whether to generate wipeout entities
    /// </summary>
    public bool GenerateWipeoutEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate image entities.
    /// </summary>
    public bool GenerateImageEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate mesh entities.
    /// </summary>
    public bool GenerateMeshEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate polyface mesh entities.
    /// </summary>
    public bool GeneratePolyfaceMeshEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate polygon mesh entities.
    /// </summary>
    public bool GeneratePolygonMeshEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate shape entities.
    /// </summary>
    public bool GenerateShapeEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate tolerance entities.
    /// </summary>
    public bool GenerateToleranceEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate trace entities.
    /// </summary>
    public bool GenerateTraceEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate underlay entities.
    /// </summary>
    public bool GenerateUnderlayEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate viewport entities.
    /// </summary>
    public bool GenerateViewportEntities { get; set; } = true;
}
