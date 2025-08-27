using System.CommandLine;
using DxfToCSharp.Core;
using netDxf;

namespace DxfToCSharp.Tool;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("DXF to C# code generator tool")
        {
            Name = "dxf2cs"
        };

        // Input options
        var inputOption = new Option<string[]>(
            aliases: new[] { "--input", "-i" },
            description: "Input DXF file(s) or directory(ies) to process")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };

        var outputOption = new Option<string?>(
            aliases: new[] { "--output", "-o" },
            description: "Output directory (default: same as input file directory)");

        var classNameOption = new Option<string?>(
            aliases: new[] { "--class-name", "-c" },
            description: "Custom class name for generated code (default: based on input filename)");

        var namespaceOption = new Option<string?>(
            aliases: new[] { "--namespace", "-n" },
            description: "Namespace for generated code");

        var recursiveOption = new Option<bool>(
            aliases: new[] { "--recursive", "-r" },
            description: "Process directories recursively");

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Enable verbose output");

        // Generation options
        var noHeaderOption = new Option<bool>(
            "--no-header",
            description: "Skip generating header comments");

        var noUsingOption = new Option<bool>(
            "--no-using",
            description: "Skip generating using statements");

        var noClassOption = new Option<bool>(
            "--no-class",
            description: "Generate only the method body without class wrapper");

        var noTablesOption = new Option<bool>(
            "--no-tables",
            description: "Skip generating table definitions (layers, linetypes, etc.)");

        var noEntitiesOption = new Option<bool>(
            "--no-entities",
            description: "Skip generating entities");

        var noObjectsOption = new Option<bool>(
            "--no-objects",
            description: "Skip generating objects");

        // Add options to command
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(classNameOption);
        rootCommand.AddOption(namespaceOption);
        rootCommand.AddOption(recursiveOption);
        rootCommand.AddOption(verboseOption);
        rootCommand.AddOption(noHeaderOption);
        rootCommand.AddOption(noUsingOption);
        rootCommand.AddOption(noClassOption);
        rootCommand.AddOption(noTablesOption);
        rootCommand.AddOption(noEntitiesOption);
        rootCommand.AddOption(noObjectsOption);

        rootCommand.SetHandler(async (context) =>
        {
            var inputs = context.ParseResult.GetValueForOption(inputOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption);
            var className = context.ParseResult.GetValueForOption(classNameOption);
            var namespaceName = context.ParseResult.GetValueForOption(namespaceOption);
            var recursive = context.ParseResult.GetValueForOption(recursiveOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var noHeader = context.ParseResult.GetValueForOption(noHeaderOption);
            var noUsing = context.ParseResult.GetValueForOption(noUsingOption);
            var noClass = context.ParseResult.GetValueForOption(noClassOption);
            var noTables = context.ParseResult.GetValueForOption(noTablesOption);
            var noEntities = context.ParseResult.GetValueForOption(noEntitiesOption);
            var noObjects = context.ParseResult.GetValueForOption(noObjectsOption);

            try
            {
                await ProcessInputsAsync(inputs, output, className, namespaceName, recursive, verbose,
                    noHeader, noUsing, noClass, noTables, noEntities, noObjects);
                context.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                if (verbose)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                }
                context.ExitCode = 1;
            }
        });

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ProcessInputsAsync(string[] inputs, string? outputDir, string? className,
        string? namespaceName, bool recursive, bool verbose, bool noHeader, bool noUsing, bool noClass,
        bool noTables, bool noEntities, bool noObjects)
    {
        var dxfFiles = new List<string>();

        // Discover DXF files from inputs
        foreach (var input in inputs)
        {
            if (File.Exists(input))
            {
                if (Path.GetExtension(input).Equals(".dxf", StringComparison.OrdinalIgnoreCase))
                {
                    dxfFiles.Add(input);
                }
                else
                {
                    Console.WriteLine($"Warning: Skipping non-DXF file: {input}");
                }
            }
            else if (Directory.Exists(input))
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var foundFiles = Directory.GetFiles(input, "*.dxf", searchOption);
                dxfFiles.AddRange(foundFiles);

                if (verbose)
                {
                    Console.WriteLine($"Found {foundFiles.Length} DXF files in directory: {input}");
                }
            }
            else
            {
                throw new FileNotFoundException($"Input path not found: {input}");
            }
        }

        if (dxfFiles.Count == 0)
        {
            throw new InvalidOperationException("No DXF files found to process.");
        }

        Console.WriteLine($"Processing {dxfFiles.Count} DXF file(s)...");

        var generator = new DxfCodeGenerator();
        var processedCount = 0;
        var errorCount = 0;

        foreach (var dxfFile in dxfFiles)
        {
            try
            {
                if (verbose)
                {
                    Console.WriteLine($"Processing: {dxfFile}");
                }

                // Load DXF document
                var doc = DxfDocument.Load(dxfFile);
                if (doc == null)
                {
                    Console.Error.WriteLine($"Failed to load DXF file: {dxfFile}");
                    errorCount++;
                    continue;
                }

                // Generate options
                var options = new DxfCodeGenerationOptions
                {
                    GenerateHeader = !noHeader,
                    GenerateUsingStatements = !noUsing,
                    GenerateClass = !noClass,
                    GenerateLayers = !noTables,
                    GenerateLinetypes = !noTables,
                    GenerateTextStyles = !noTables,
                    GenerateBlocks = !noTables,
                    GenerateDimensionStyles = !noTables,
                    GenerateMLineStyles = !noTables,
                    GenerateUCS = !noTables,
                    GenerateVPorts = !noTables,
                    GenerateLineEntities = !noEntities,
                    GenerateArcEntities = !noEntities,
                    GenerateCircleEntities = !noEntities,
                    GeneratePolylineEntities = !noEntities,
                    GeneratePolyline2DEntities = !noEntities,
                    GeneratePolyline3DEntities = !noEntities,
                    GenerateTextEntities = !noEntities,
                    GenerateMTextEntities = !noEntities,
                    GeneratePointEntities = !noEntities,
                    GenerateInsertEntities = !noEntities,
                    GenerateHatchEntities = !noEntities,
                    GenerateGroupObjects = !noObjects,
                    GenerateLayoutObjects = !noObjects,
                    GenerateImageDefinitionObjects = !noObjects,
                    GenerateUnderlayDefinitionObjects = !noObjects,
                    GenerateXRecordObjects = !noObjects,
                    GenerateDictionaryObjects = !noObjects,
                    GenerateRasterVariablesObjects = !noObjects,
                    GenerateLayerStateObjects = !noObjects,
                    GeneratePlotSettingsObjects = !noObjects,
                    GenerateMLineStyleObjects = !noObjects,
                    GenerateApplicationRegistryObjects = !noObjects,
                    GenerateShapeStyleObjects = !noObjects
                };

                // Determine class name
                var finalClassName = className ??
                    Path.GetFileNameWithoutExtension(dxfFile).Replace(" ", "").Replace("-", "_") + "Generator";

                // Generate C# code
                var generatedCode = generator.Generate(doc, dxfFile, finalClassName, options);

                // Add namespace if specified
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    generatedCode = $"namespace {namespaceName};\n\n{generatedCode}";
                }

                // Determine output file path
                var outputFileName = Path.GetFileNameWithoutExtension(dxfFile) + ".cs";
                var finalOutputDir = outputDir ?? Path.GetDirectoryName(dxfFile) ?? Environment.CurrentDirectory;
                var outputFilePath = Path.Join(finalOutputDir, outputFileName);

                // Ensure output directory exists
                Directory.CreateDirectory(finalOutputDir);

                // Write generated code to file
                await File.WriteAllTextAsync(outputFilePath, generatedCode);

                Console.WriteLine($"Generated: {outputFilePath}");
                processedCount++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing {dxfFile}: {ex.Message}");
                if (verbose)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                }
                errorCount++;
            }
        }

        Console.WriteLine($"\nCompleted: {processedCount} files processed successfully, {errorCount} errors.");

        if (errorCount > 0)
        {
            throw new InvalidOperationException($"Processing completed with {errorCount} error(s).");
        }
    }
}
