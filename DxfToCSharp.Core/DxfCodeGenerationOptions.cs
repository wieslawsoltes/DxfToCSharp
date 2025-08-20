using System;

namespace DxfToCSharp.Core;

/// <summary>
/// Options for controlling DXF to C# code generation
/// </summary>
public class DxfCodeGenerationOptions
{
    /// <summary>
    /// Whether to generate header comments with source file info and timestamp
    /// </summary>
    public bool GenerateHeader { get; set; } = true;

    /// <summary>
    /// Whether to generate using statements
    /// </summary>
    public bool GenerateUsingStatements { get; set; } = true;

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
    /// Whether to generate entity objects
    /// </summary>
    public bool GenerateEntities { get; set; } = true;

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
    /// Whether to generate dimension entities
    /// </summary>
    public bool GenerateDimensionEntities { get; set; } = true;

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
    /// Whether to generate the document save comment
    /// </summary>
    public bool GenerateSaveComment { get; set; } = true;

    /// <summary>
    /// Whether to generate the return statement
    /// </summary>
    public bool GenerateReturnStatement { get; set; } = true;

    /// <summary>
    /// Custom class name for the generated code (null for default)
    /// </summary>
    public string? CustomClassName { get; set; }

    /// <summary>
    /// Whether to include detailed comments for each entity
    /// </summary>
    public bool GenerateDetailedComments { get; set; } = false;

    /// <summary>
    /// Whether to group entities by type in the generated code
    /// </summary>
    public bool GroupEntitiesByType { get; set; } = false;

    /// <summary>
    /// Whether to generate only used table definitions (layers, styles, etc.)
    /// </summary>
    public bool GenerateOnlyUsedTables { get; set; } = true;

    /// <summary>
    /// Creates a new instance with all options enabled (default behavior)
    /// </summary>
    public static DxfCodeGenerationOptions CreateDefault()
    {
        return new DxfCodeGenerationOptions();
    }

    /// <summary>
    /// Creates a new instance with only entities enabled (no table definitions)
    /// </summary>
    public static DxfCodeGenerationOptions CreateEntitiesOnly()
    {
        return new DxfCodeGenerationOptions
        {
            GenerateLayers = false,
            GenerateLinetypes = false,
            GenerateTextStyles = false,
            GenerateBlocks = false,
            GenerateDimensionStyles = false,
            GenerateMLineStyles = false
        };
    }

    /// <summary>
    /// Creates a new instance with only basic geometric entities (lines, arcs, circles)
    /// </summary>
    public static DxfCodeGenerationOptions CreateBasicGeometryOnly()
    {
        var options = CreateEntitiesOnly();
        options.GenerateTextEntities = false;
        options.GenerateMTextEntities = false;
        options.GenerateInsertEntities = false;
        options.GenerateDimensionEntities = false;
        options.GenerateLeaderEntities = false;
        options.GenerateMLineEntities = false;
        options.GenerateWipeoutEntities = false;
        return options;
    }
}