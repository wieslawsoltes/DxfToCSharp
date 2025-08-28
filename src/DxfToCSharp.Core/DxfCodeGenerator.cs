using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;
using Attribute = netDxf.Entities.Attribute;
using PointEntity = netDxf.Entities.Point;

namespace DxfToCSharp.Core;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public class DxfCodeGenerator
{
    private readonly HashSet<string> _usedLayers = new();
    private readonly HashSet<string> _usedLinetypes = new();
    private readonly HashSet<string> _usedTextStyles = new();
    private readonly HashSet<string> _usedBlocks = new();
    private readonly HashSet<string> _usedDimensionStyles = new();
    private readonly HashSet<string> _usedMLineStyles = new();
    private readonly HashSet<string> _usedUCS = new();
    private readonly HashSet<string> _usedVPorts = new();
    private readonly HashSet<string> _usedViews = new();
    private readonly HashSet<string> _entitiesNeedingVariables = new();
    private int _insertCounter;
    private int _entityCounter;

    public string Generate(DxfDocument doc, string? sourcePath, string? className = null, DxfCodeGenerationOptions? options = null)
    {
        // Use provided options or create default
        options ??= new DxfCodeGenerationOptions();

        var allEntities = doc.Entities.All?.ToList() ?? new List<EntityObject>();

        // Clear collections for fresh generation
        _usedLayers.Clear();
        _usedLinetypes.Clear();
        _usedTextStyles.Clear();
        _usedBlocks.Clear();
        _usedDimensionStyles.Clear();
        _usedMLineStyles.Clear();
        _usedUCS.Clear();
        _usedVPorts.Clear();
        _insertCounter = 0;

        var sb = new StringBuilder();

        // Use provided class name, options class name, or default
        var finalClassName = className ?? options.CustomClassName ?? "DxfDocumentGenerator";

        // Header and using statements
        GenerateHeader(sb, sourcePath, options);

        // Analyze what tables we need (only if generating any tables)
        if (options.GenerateLayers || options.GenerateLinetypes || options.GenerateTextStyles ||
            options.GenerateBlocks || options.GenerateDimensionStyles || options.GenerateMLineStyles || options.GenerateUCS || options.GenerateVPorts)
        {
            AnalyzeUsedTables(allEntities, options);
        }

        // Analyze which entities need to be generated as variables (for group references)
        AnalyzeEntitiesReferencedByGroups(doc, options);

        // Class definition start (if enabled)
        if (options.GenerateClass)
        {
            sb.AppendLine($"public static class {finalClassName}");
            sb.AppendLine("{");
            sb.AppendLine("public static DxfDocument Create()");
            sb.AppendLine("{");
        }
        sb.AppendLine("var doc = new DxfDocument();");
        sb.AppendLine();

        // Generate header variables
        if (options.GenerateHeaderVariables)
        {
            GenerateHeaderVariables(sb, doc, options);
        }

        // Generate table definitions
        GenerateTableDefinitions(sb, doc, options);

        // Generate entities
        GenerateEntities(sb, allEntities, options);

        // Generate objects
        GenerateObjects(sb, doc, options);

        // Footer
        sb.AppendLine();

        if (options.GenerateClass)
        {
            sb.AppendLine("return doc;");
            sb.AppendLine("}");
            sb.AppendLine("}");
        }
        else
        {
            sb.AppendLine("return doc;");
        }

        var code = sb.ToString();

        if (!options.FormatWithRoslyn)
        {
            return code;
        }

        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            root = root.NormalizeWhitespace();

            root = new EmptyInitializerRemover().Visit(root);
            root = new SemicolonJoiner().Visit(root);

            code = Format(root).ToFullString();
        }
        catch
        {
            // If Roslyn formatting fails for any reason, fall back to unformatted code
        }

        return code;
    }

    public static SyntaxNode Format(SyntaxNode root)
    {
        using var workspace = new AdhocWorkspace();
        var opt = workspace.Options
            .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, false)
            .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, 4)
            .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 4)
            .WithChangedOption(CSharpFormattingOptions.IndentBlock, true)
            .WithChangedOption(CSharpFormattingOptions.WrappingPreserveSingleLine, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, true)
            .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, true)
            .WithChangedOption(CSharpFormattingOptions.NewLineForMembersInObjectInit, true)
            .WithChangedOption(CSharpFormattingOptions.NewLineForMembersInAnonymousTypes, true);

        var formatted = Formatter.Format(root, workspace, opt);
        return formatted;
    }

    private sealed class EmptyInitializerRemover : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var visited = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node)!;
            var init = visited.Initializer;
            if (init != null && init.Expressions.Count == 0)
            {
                // Ensure argument list exists if initializer was the only thing after type (e.g., `new Foo {}`)
                if (visited.ArgumentList == null)
                {
                    visited = visited.WithArgumentList(SyntaxFactory.ArgumentList());
                }
                visited = visited.WithInitializer(null);
            }
            return visited;
        }
    }

    private sealed class SemicolonJoiner : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var visited = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(node)!;
            var semi = visited.SemicolonToken;
            var prev = semi.GetPreviousToken();

            // Remove any leading EOL before semicolon
            var newSemi = semi.WithLeadingTrivia();

            // Remove trailing EOL from the token before semicolon
            var trailing = prev.TrailingTrivia;
            if (trailing.Count > 0)
            {
                var filtered = new SyntaxTriviaList();
                foreach (var t in trailing)
                {
                    if (!t.IsKind(SyntaxKind.EndOfLineTrivia))
                        filtered = filtered.Add(t);
                }
                var newPrev = prev.WithTrailingTrivia(filtered);
                visited = visited.ReplaceToken(prev, newPrev);
            }

            visited = visited.WithSemicolonToken(newSemi);
            return visited;
        }
    }

    private static string F(double v)
    {
        // Always emit a representation that is unequivocally a double literal.
        // This avoids cases where values like 0 are rendered as "0" (an int literal),
        // which can cause runtime type mismatches when boxed into object (e.g., XData double requirements).
        var s = v.ToString("G17", CultureInfo.InvariantCulture);
        if (!s.Contains(".") && !s.Contains("e") && !s.Contains("E"))
        {
            s += ".0";
        }
        return s;
    }

    private string GenerateEnumFlags<T>(T enumValue) where T : Enum
    {
        var enumType = typeof(T);
        var enumName = enumType.Name;
        var value = Convert.ToInt32(enumValue);

        if (value == 0)
        {
            return $"{enumName}.None";
        }

        var flags = new List<string>();
        foreach (var enumVal in Enum.GetValues(enumType))
        {
            var intVal = Convert.ToInt32(enumVal);
            if (intVal != 0 && (value & intVal) == intVal)
            {
                flags.Add($"{enumName}.{enumVal}");
            }
        }

        return flags.Count > 0 ? string.Join(" | ", flags) : $"{enumName}.None";
    }

    private void GenerateHeader(StringBuilder sb, string? sourcePath, DxfCodeGenerationOptions options)
    {
        if (options.GenerateHeader)
        {
            sb.AppendLine("/// <auto-generated>");
            sb.AppendLine("/// This code was generated by DxfToCSharp.");
            if (!string.IsNullOrEmpty(sourcePath))
            {
                sb.AppendLine($"/// Source: {Path.GetFileName(sourcePath)}");
            }
            sb.AppendLine($"/// Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("/// </auto-generated>");
        }

        if (options.GenerateUsingStatements)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using netDxf;");
            sb.AppendLine("using netDxf.Entities;");
            sb.AppendLine("using netDxf.Header;");
            sb.AppendLine("using netDxf.Objects;");
            sb.AppendLine("using netDxf.Tables;");
            sb.AppendLine("using netDxf.Blocks;");
            sb.AppendLine("using netDxf.Units;");
            sb.AppendLine("using Attribute = netDxf.Entities.Attribute;");
            sb.AppendLine();
        }
    }

    private void GenerateHeaderVariables(StringBuilder sb, DxfDocument doc, DxfCodeGenerationOptions options)
    {
        var headerVars = doc.DrawingVariables;
        var hasNonDefaultValues = false;

        // Check if any header variables have non-default values
        var tempSb = new StringBuilder();

        // Generate Vector3 properties
        if (headerVars.InsBase != Vector3.Zero)
        {
            tempSb.AppendLine($"doc.DrawingVariables.InsBase = new Vector3({F(headerVars.InsBase.X)}, {F(headerVars.InsBase.Y)}, {F(headerVars.InsBase.Z)});");
            hasNonDefaultValues = true;
        }

        // Generate enum properties with public setters
        if (headerVars.AcadVer != DxfVersion.AutoCad2000)
        {
            tempSb.AppendLine($"doc.DrawingVariables.AcadVer = DxfVersion.{headerVars.AcadVer};");
            hasNonDefaultValues = true;
        }

        // Generate double properties
        if (Math.Abs(headerVars.Angbase - 0.0) > 1e-9)
        {
            tempSb.AppendLine($"doc.DrawingVariables.Angbase = {F(headerVars.Angbase)};");
            hasNonDefaultValues = true;
        }

        if (Math.Abs(headerVars.TextSize - 2.5) > 1e-9)
        {
            tempSb.AppendLine($"doc.DrawingVariables.TextSize = {F(headerVars.TextSize)};");
            hasNonDefaultValues = true;
        }

        if (Math.Abs(headerVars.LtScale - 1.0) > 1e-9)
        {
            tempSb.AppendLine($"doc.DrawingVariables.LtScale = {F(headerVars.LtScale)};");
            hasNonDefaultValues = true;
        }

        if (Math.Abs(headerVars.CeLtScale - 1.0) > 1e-9)
        {
            tempSb.AppendLine($"doc.DrawingVariables.CeLtScale = {F(headerVars.CeLtScale)};");
            hasNonDefaultValues = true;
        }

        if (Math.Abs(headerVars.PdSize - 0.0) > 1e-9)
        {
            tempSb.AppendLine($"doc.DrawingVariables.PdSize = {F(headerVars.PdSize)};");
            hasNonDefaultValues = true;
        }

        if (Math.Abs(headerVars.CMLScale - 20.0) > 1e-9)
        {
            tempSb.AppendLine($"doc.DrawingVariables.CMLScale = {F(headerVars.CMLScale)};");
            hasNonDefaultValues = true;
        }

        // Generate enum properties
        if (headerVars.Angdir != AngleDirection.CCW)
        {
            tempSb.AppendLine($"doc.DrawingVariables.Angdir = AngleDirection.{headerVars.Angdir};");
            hasNonDefaultValues = true;
        }

        if (headerVars.InsUnits != DrawingUnits.Unitless)
        {
            tempSb.AppendLine($"doc.DrawingVariables.InsUnits = DrawingUnits.{headerVars.InsUnits};");
            hasNonDefaultValues = true;
        }

        if (headerVars.AttMode != AttMode.Normal)
        {
            tempSb.AppendLine($"doc.DrawingVariables.AttMode = AttMode.{headerVars.AttMode};");
            hasNonDefaultValues = true;
        }

        if (headerVars.PdMode != PointShape.Dot)
        {
            tempSb.AppendLine($"doc.DrawingVariables.PdMode = PointShape.{headerVars.PdMode};");
            hasNonDefaultValues = true;
        }

        if (headerVars.CeLweight != Lineweight.ByLayer)
        {
            tempSb.AppendLine($"doc.DrawingVariables.CeLweight = Lineweight.{headerVars.CeLweight};");
            hasNonDefaultValues = true;
        }

        if (headerVars.AUnits != AngleUnitType.DecimalDegrees)
        {
            tempSb.AppendLine($"doc.DrawingVariables.AUnits = AngleUnitType.{headerVars.AUnits};");
            hasNonDefaultValues = true;
        }

        if (headerVars.LUnits != LinearUnitType.Decimal)
        {
            tempSb.AppendLine($"doc.DrawingVariables.LUnits = LinearUnitType.{headerVars.LUnits};");
            hasNonDefaultValues = true;
        }

        if (headerVars.CMLJust != MLineJustification.Top)
        {
            tempSb.AppendLine($"doc.DrawingVariables.CMLJust = MLineJustification.{headerVars.CMLJust};");
            hasNonDefaultValues = true;
        }

        // Generate AciColor properties
        if (headerVars.CeColor.Index != AciColor.ByLayer.Index)
        {
            if (headerVars.CeColor.Index is >= 0 and <= 255)
            {
                tempSb.AppendLine($"doc.DrawingVariables.CeColor = new AciColor({headerVars.CeColor.Index});");
            }
            else
            {
                tempSb.AppendLine($"doc.DrawingVariables.CeColor = AciColor.ByLayer;");
            }
            hasNonDefaultValues = true;
        }

        // Generate string properties
        if (!string.IsNullOrEmpty(headerVars.LastSavedBy) && headerVars.LastSavedBy != Environment.UserName)
        {
            tempSb.AppendLine($"doc.DrawingVariables.LastSavedBy = \"{Escape(headerVars.LastSavedBy)}\";");
            hasNonDefaultValues = true;
        }

        if (headerVars.CeLtype != "ByLayer")
        {
            tempSb.AppendLine($"doc.DrawingVariables.CeLtype = \"{Escape(headerVars.CeLtype)}\";");
            hasNonDefaultValues = true;
        }

        if (headerVars.CLayer != "0")
        {
            tempSb.AppendLine($"doc.DrawingVariables.CLayer = \"{Escape(headerVars.CLayer)}\";");
            hasNonDefaultValues = true;
        }

        if (headerVars.CMLStyle != "Standard")
        {
            tempSb.AppendLine($"doc.DrawingVariables.CMLStyle = \"{Escape(headerVars.CMLStyle)}\";");
            hasNonDefaultValues = true;
        }

        if (headerVars.DimStyle != "Standard")
        {
            tempSb.AppendLine($"doc.DrawingVariables.DimStyle = \"{Escape(headerVars.DimStyle)}\";");
            hasNonDefaultValues = true;
        }

        if (headerVars.TextStyle != "Standard")
        {
            tempSb.AppendLine($"doc.DrawingVariables.TextStyle = \"{Escape(headerVars.TextStyle)}\";");
            hasNonDefaultValues = true;
        }

        // Generate integer properties
        if (headerVars.AUprec != 0)
        {
            tempSb.AppendLine($"doc.DrawingVariables.AUprec = {headerVars.AUprec};");
            hasNonDefaultValues = true;
        }

        if (headerVars.LUprec != 4)
        {
            tempSb.AppendLine($"doc.DrawingVariables.LUprec = {headerVars.LUprec};");
            hasNonDefaultValues = true;
        }

        if (headerVars.PLineGen != 0)
        {
            tempSb.AppendLine($"doc.DrawingVariables.PLineGen = {headerVars.PLineGen};");
            hasNonDefaultValues = true;
        }

        if (headerVars.PsLtScale != 1)
        {
            tempSb.AppendLine($"doc.DrawingVariables.PsLtScale = {headerVars.PsLtScale};");
            hasNonDefaultValues = true;
        }

        if (headerVars.SplineSegs != 8)
        {
            tempSb.AppendLine($"doc.DrawingVariables.SplineSegs = {headerVars.SplineSegs};");
            hasNonDefaultValues = true;
        }

        if (headerVars.SurfU != 6)
        {
            tempSb.AppendLine($"doc.DrawingVariables.SurfU = {headerVars.SurfU};");
            hasNonDefaultValues = true;
        }

        if (headerVars.SurfV != 6)
        {
            tempSb.AppendLine($"doc.DrawingVariables.SurfV = {headerVars.SurfV};");
            hasNonDefaultValues = true;
        }

        // Generate boolean properties
        if (headerVars.MirrText)
        {
            tempSb.AppendLine($"doc.DrawingVariables.MirrText = {headerVars.MirrText.ToString().ToLower()};");
            hasNonDefaultValues = true;
        }

        if (headerVars.LwDisplay)
        {
            tempSb.AppendLine($"doc.DrawingVariables.LwDisplay = {headerVars.LwDisplay.ToString().ToLower()};");
            hasNonDefaultValues = true;
        }

        // Generate DateTime properties
        // Note: DateTime properties are initialized with DateTime.Now/UtcNow in HeaderVariables constructor,
        // so we only generate code if they have been explicitly set to specific values
        // We'll generate code for any DateTime that's not the current date/time (allowing some tolerance)
        var now = DateTime.Now;
        var utcNow = DateTime.UtcNow;

        // Only generate DateTime code if the values are significantly different from current time
        // or if they represent specific dates that should be preserved
        if (Math.Abs((headerVars.TdCreate - now).TotalMinutes) > 1)
        {
            tempSb.AppendLine($"doc.DrawingVariables.TdCreate = new DateTime({headerVars.TdCreate.Year}, {headerVars.TdCreate.Month}, {headerVars.TdCreate.Day}, {headerVars.TdCreate.Hour}, {headerVars.TdCreate.Minute}, {headerVars.TdCreate.Second});");
            hasNonDefaultValues = true;
        }

        if (Math.Abs((headerVars.TduCreate - utcNow).TotalMinutes) > 1)
        {
            tempSb.AppendLine($"doc.DrawingVariables.TduCreate = new DateTime({headerVars.TduCreate.Year}, {headerVars.TduCreate.Month}, {headerVars.TduCreate.Day}, {headerVars.TduCreate.Hour}, {headerVars.TduCreate.Minute}, {headerVars.TduCreate.Second}, DateTimeKind.Utc);");
            hasNonDefaultValues = true;
        }

        if (Math.Abs((headerVars.TdUpdate - now).TotalMinutes) > 1)
        {
            tempSb.AppendLine($"doc.DrawingVariables.TdUpdate = new DateTime({headerVars.TdUpdate.Year}, {headerVars.TdUpdate.Month}, {headerVars.TdUpdate.Day}, {headerVars.TdUpdate.Hour}, {headerVars.TdUpdate.Minute}, {headerVars.TdUpdate.Second});");
            hasNonDefaultValues = true;
        }

        if (Math.Abs((headerVars.TduUpdate - utcNow).TotalMinutes) > 1)
        {
            tempSb.AppendLine($"doc.DrawingVariables.TduUpdate = new DateTime({headerVars.TduUpdate.Year}, {headerVars.TduUpdate.Month}, {headerVars.TduUpdate.Day}, {headerVars.TduUpdate.Hour}, {headerVars.TduUpdate.Minute}, {headerVars.TduUpdate.Second}, DateTimeKind.Utc);");
            hasNonDefaultValues = true;
        }

        // Generate TimeSpan properties
        if (headerVars.TdinDwg != TimeSpan.Zero)
        {
            tempSb.AppendLine($"doc.DrawingVariables.TdinDwg = new TimeSpan({headerVars.TdinDwg.Days}, {headerVars.TdinDwg.Hours}, {headerVars.TdinDwg.Minutes}, {headerVars.TdinDwg.Seconds});");
            hasNonDefaultValues = true;
        }

        // Generate UCS properties
        if (headerVars.CurrentUCS != null && headerVars.CurrentUCS.Name != "Unnamed")
        {
            var ucs = headerVars.CurrentUCS;
            tempSb.AppendLine($"doc.DrawingVariables.CurrentUCS = new UCS(\"{Escape(ucs.Name)}\", new Vector3({F(ucs.Origin.X)}, {F(ucs.Origin.Y)}, {F(ucs.Origin.Z)}), new Vector3({F(ucs.XAxis.X)}, {F(ucs.XAxis.Y)}, {F(ucs.XAxis.Z)}), new Vector3({F(ucs.YAxis.X)}, {F(ucs.YAxis.Y)}, {F(ucs.YAxis.Z)}));");
            hasNonDefaultValues = true;
        }

        // Only add the header variables section if there are non-default values
        if (hasNonDefaultValues)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Header variables (drawing variables)");
            }
            sb.Append(tempSb);
            sb.AppendLine();
        }
    }

    private void AnalyzeUsedTables(List<EntityObject> entities, DxfCodeGenerationOptions options)
    {
        foreach (var entity in entities)
        {
            if (options.GenerateLayers && entity.Layer != null)
                _usedLayers.Add(entity.Layer.Name);
            // Layers referenced by viewport FrozenLayers must be generated too
            if (options.GenerateLayers && entity is Viewport vp && vp.FrozenLayers != null && vp.FrozenLayers.Count > 0)
            {
                foreach (var l in vp.FrozenLayers)
                {
                    if (l != null) _usedLayers.Add(l.Name);
                }
            }
            if (options.GenerateLinetypes && entity.Linetype != null)
                _usedLinetypes.Add(entity.Linetype.Name);

            // Check for text entities that use text styles
            if (options.GenerateTextStyles)
            {
                if (entity is Text { Style: not null } text)
                    _usedTextStyles.Add(text.Style.Name);
                else if (entity is MText { Style: not null } mtext)
                    _usedTextStyles.Add(mtext.Style.Name);
            }

            // Check for Insert entities that use blocks
            if (options.GenerateBlocks && entity is Insert { Block: not null } insert)
                _usedBlocks.Add(insert.Block.Name);

            // Check for Leader entities that use dimension styles
            if (options.GenerateDimensionStyles)
            {
                if (entity is Leader { Style: not null } leader)
                    _usedDimensionStyles.Add(leader.Style.Name);

                // Check for Dimension entities that use dimension styles
                if (entity is Dimension { Style: not null } dimension)
                    _usedDimensionStyles.Add(dimension.Style.Name);
            }

            // Check for MLine entities that use multiline styles
            if (options.GenerateMLineStyles && entity is MLine { Style: not null } mline)
                _usedMLineStyles.Add(mline.Style.Name);
        }

        // Analyze UCS objects (they are not directly referenced by entities but are part of document structure)
        if (options.GenerateUCS)
        {
            AnalyzeUsedUCS(entities, options);
        }

        // Analyze VPort objects (they are not directly referenced by entities but are part of document structure)
        if (options.GenerateVPorts)
        {
            AnalyzeUsedVPorts(entities, options);
        }

        // Analyze View objects (they are not directly referenced by entities but are part of document structure)
        if (options.GenerateViews)
        {
            AnalyzeUsedViews(entities, options);
        }
    }

    private void AnalyzeEntitiesReferencedByGroups(DxfDocument doc, DxfCodeGenerationOptions options)
    {
        if (!options.GenerateGroupObjects)
            return;

        foreach (var group in doc.Groups)
        {
            foreach (var entity in group.Entities)
            {
                _entitiesNeedingVariables.Add(entity.Handle);
            }
        }
    }

    private static bool HasXData(EntityObject entity)
    {
        try
        {
            return entity.XData != null && entity.XData.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private void AnalyzeUsedUCS(List<EntityObject> entities, DxfCodeGenerationOptions options)
    {
        // For now, we'll include all UCS objects in the document since they are typically
        // standalone coordinate system definitions that may be referenced by name
        // In a more sophisticated implementation, we could track which UCS objects are actually used
    }

    private void AnalyzeUsedVPorts(List<EntityObject> entities, DxfCodeGenerationOptions options)
    {
        // For now, we'll include all VPort objects in the document since they are typically
        // viewport definitions that may be referenced by name
        // In a more sophisticated implementation, we could track which VPort objects are actually used

        // Only include the active viewport (*Active) if it has been modified from defaults
        // This will be checked later in the generation phase
    }

    private void AnalyzeUsedViews(List<EntityObject> entities, DxfCodeGenerationOptions options)
    {
        // For now, we'll include all View objects in the document since they are typically
        // view definitions that may be referenced by name
        // In a more sophisticated implementation, we could track which View objects are actually used
        // Note: Views collection is internal in netDxf, so we generate placeholder objects
    }

    private void GenerateTableDefinitions(StringBuilder sb, DxfDocument doc, DxfCodeGenerationOptions options)
    {
        // Generate layer definitions
        if (options.GenerateLayers && _usedLayers.Count > 0)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Layer definitions");
            }
            foreach (var layerName in _usedLayers.OrderBy(x => x))
            {
                var layer = doc.Layers.FirstOrDefault(l => l.Name == layerName);
                if (layer != null)
                {
                    sb.AppendLine($"var layer{SafeName(layerName)} = new Layer(\"{Escape(layerName)}\")");
                    sb.AppendLine("{");
                    if (layer.Color.Index != 7) // Default color is 7 (white)
                        sb.AppendLine($"Color = new AciColor({layer.Color.Index}),");
                    if (layer.Lineweight != Lineweight.Default)
                        sb.AppendLine($"Lineweight = Lineweight.{layer.Lineweight},");
                    if (!layer.IsVisible)
                        sb.AppendLine($"IsVisible = false,");
                    if (layer.IsFrozen)
                        sb.AppendLine($"IsFrozen = true,");
                    if (layer.IsLocked)
                        sb.AppendLine($"IsLocked = true,");
                    sb.AppendLine("};");
                    sb.AppendLine($"doc.Layers.Add(layer{SafeName(layerName)});");
                    sb.AppendLine();
                }
                else if (layerName == "0")
                {
                    // Handle default layer "0" which might not be in the layers collection
                    sb.AppendLine($"var layer{SafeName(layerName)} = doc.Layers[\"{layerName}\"];");
                    sb.AppendLine();
                }
            }
        }

        // Generate linetype definitions (if any custom ones)
        if (options.GenerateLinetypes && _usedLinetypes.Any(lt => lt != "Continuous" && lt != "ByLayer" && lt != "ByBlock"))
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Linetype definitions");
            }
            foreach (var linetypeName in _usedLinetypes.Where(lt => lt != "Continuous" && lt != "ByLayer" && lt != "ByBlock"))
            {
                var linetype = doc.Linetypes.FirstOrDefault(lt => lt.Name == linetypeName);
                if (linetype != null)
                {
                    sb.AppendLine($"var linetype{SafeName(linetypeName)} = new Linetype(\"{Escape(linetypeName)}\");");
                    sb.AppendLine($"doc.Linetypes.Add(linetype{SafeName(linetypeName)});");
                }
            }
            sb.AppendLine();
        }

        // Generate text style definitions (if any custom ones)
        if (options.GenerateTextStyles && _usedTextStyles.Any(ts => ts != "Standard"))
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Text style definitions");
            }
            foreach (var styleName in _usedTextStyles.Where(ts => ts != "Standard"))
            {
                var style = doc.TextStyles.FirstOrDefault(ts => ts.Name == styleName);
                if (style != null)
                {
                    // Use the required constructor parameters for TextStyle
                    var fontFile = !string.IsNullOrEmpty(style.FontFile) ? style.FontFile : TextStyle.DefaultFont;
                    sb.AppendLine($"var textStyle{SafeName(styleName)} = new TextStyle(\"{Escape(styleName)}\", \"{Escape(fontFile)}\");");
                    sb.AppendLine($"doc.TextStyles.Add(textStyle{SafeName(styleName)});");
                }
            }
            sb.AppendLine();
        }

        // Generate block definitions (if any used)
        if (options.GenerateBlocks && _usedBlocks.Count > 0)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Block definitions");
            }

            // For block entity generation, ignore per-entity enable/disable flags by overriding them to true
            var blockEntityOptions = options with
            {
                GenerateLineEntities = true,
                GenerateArcEntities = true,
                GenerateAttributeDefinitionEntities = true,
                GenerateCircleEntities = true,
                GeneratePolylineEntities = true,
                GeneratePolyline2DEntities = true,
                GeneratePolyline3DEntities = true,
                GenerateTextEntities = true,
                GenerateMTextEntities = true,
                GeneratePointEntities = true,
                GenerateInsertEntities = true,
                GenerateHatchEntities = true,
                GenerateDimensionEntities = true,
                GenerateLinearDimensionEntities = true,
                GenerateAlignedDimensionEntities = true,
                GenerateRadialDimensionEntities = true,
                GenerateDiametricDimensionEntities = true,
                GenerateAngular2LineDimensionEntities = true,
                GenerateAngular3PointDimensionEntities = true,
                GenerateOrdinateDimensionEntities = true,
                GenerateArcLengthDimensionEntities = true,
                GenerateLeaderEntities = true,
                GenerateSplineEntities = true,
                GenerateEllipseEntities = true,
                GenerateSolidEntities = true,
                GenerateFace3dEntities = true,
                GenerateMLineEntities = true,
                GenerateRayEntities = true,
                GenerateXLineEntities = true,
                GenerateWipeoutEntities = true,
                GenerateImageEntities = true,
                GenerateMeshEntities = true,
                GeneratePolyfaceMeshEntities = true,
                GeneratePolygonMeshEntities = true,
                GenerateShapeEntities = true,
                GenerateToleranceEntities = true,
                GenerateTraceEntities = true,
                GenerateUnderlayEntities = true,
                GenerateViewportEntities = true
            };

            foreach (var blockName in _usedBlocks.OrderBy(x => x))
            {
                var block = doc.Blocks.FirstOrDefault(b => b.Name == blockName);
                if (block != null)
                {
                    sb.AppendLine($"var block{SafeName(blockName)} = new Block(\"{Escape(blockName)}\")");
                    sb.AppendLine("{");
                    sb.AppendLine($"Origin = new Vector3({F(block.Origin.X)}, {F(block.Origin.Y)}, {F(block.Origin.Z)}),");
                    if (!string.IsNullOrEmpty(block.Description))
                        sb.AppendLine($"Description = \"{Escape(block.Description)}\",");
                    if (block.Layer?.Name != "0" && block.Layer != null)
                        sb.AppendLine($"Layer = new Layer(\"{Escape(block.Layer.Name)}\"),");
                    sb.AppendLine("};");

                    // Add attribute definitions if any
                    if (options.GenerateAttributeDefinitionEntities && block.AttributeDefinitions.Count > 0)
                    {
                        foreach (var attDef in block.AttributeDefinitions.Values)
                        {
                            var textStyleName = attDef.Style?.Name ?? "Standard";
                            if (textStyleName == "Standard")
                            {
                                sb.AppendLine($"var attDef{SafeName(blockName)}{SafeName(attDef.Tag)} = new AttributeDefinition(\"{Escape(attDef.Tag)}\", {F(attDef.Height)}, TextStyle.Default)");
                            }
                            else
                            {
                                sb.AppendLine($"var attDef{SafeName(blockName)}{SafeName(attDef.Tag)} = new AttributeDefinition(\"{Escape(attDef.Tag)}\", {F(attDef.Height)}, textStyle{SafeName(textStyleName)})");
                            }
                            sb.AppendLine("{");

                            // Basic properties
                            if (!string.IsNullOrEmpty(attDef.Prompt))
                                sb.AppendLine($"Prompt = \"{Escape(attDef.Prompt)}\",");
                            if (!string.IsNullOrEmpty(attDef.Value))
                                sb.AppendLine($"Value = \"{Escape(attDef.Value)}\",");

                            // Position and dimensions
                            if (attDef.Position != Vector3.Zero)
                                sb.AppendLine($"Position = new Vector3({F(attDef.Position.X)}, {F(attDef.Position.Y)}, {F(attDef.Position.Z)}),");
                            if (Math.Abs(attDef.Height - 1.0) > 1e-10)
                                sb.AppendLine($"Height = {F(attDef.Height)},");
                            if (Math.Abs(attDef.Width - 1.0) > 1e-10)
                                sb.AppendLine($"Width = {F(attDef.Width)},");
                            if (Math.Abs(attDef.WidthFactor - 1.0) > 1e-10)
                                sb.AppendLine($"WidthFactor = {F(attDef.WidthFactor)},");

                            // Text formatting
                            if (Math.Abs(attDef.Rotation) > 1e-12)
                                sb.AppendLine($"Rotation = {F(attDef.Rotation)},");
                            if (Math.Abs(attDef.ObliqueAngle) > 1e-10)
                                sb.AppendLine($"ObliqueAngle = {F(attDef.ObliqueAngle)},");
                            if (attDef.Alignment != TextAlignment.BaselineLeft)
                                sb.AppendLine($"Alignment = TextAlignment.{attDef.Alignment},");
                            if (attDef.IsBackward)
                                sb.AppendLine($"IsBackward = true,");
                            if (attDef.IsUpsideDown)
                                sb.AppendLine($"IsUpsideDown = true,");

                            // Style and flags
                            if (attDef.Style != null && attDef.Style.Name != "Standard" && _usedTextStyles.Contains(attDef.Style.Name))
                                sb.AppendLine($"Style = textStyle{SafeName(attDef.Style.Name)},");
                            if (attDef.Flags != AttributeFlags.None)
                                sb.AppendLine($"Flags = AttributeFlags.{attDef.Flags},");

                            // Entity properties (Layer, Color, etc.)
                            if (attDef.Layer != null && _usedLayers.Contains(attDef.Layer.Name))
                                sb.AppendLine($"Layer = layer{SafeName(attDef.Layer.Name)},");
                            if (attDef.Color.Index != 256) // Not ByLayer
                            {
                                if (attDef.Color.Index == 0)
                                    sb.AppendLine($"Color = AciColor.ByBlock,");
                                else if (attDef.Color.Index is >= 1 and <= 255)
                                    sb.AppendLine($"Color = new AciColor({attDef.Color.Index}),");
                            }
                            if (attDef.Linetype != null && attDef.Linetype.Name != "ByLayer" && attDef.Linetype.Name != "Continuous")
                            {
                                if (attDef.Linetype.Name == "ByBlock")
                                    sb.AppendLine($"Linetype = Linetype.ByBlock,");
                                else
                                    sb.AppendLine($"Linetype = linetype{SafeName(attDef.Linetype.Name)},");
                            }
                            if (attDef.Lineweight != Lineweight.ByLayer)
                                sb.AppendLine($"Lineweight = Lineweight.{attDef.Lineweight},");
                            if (Math.Abs(attDef.LinetypeScale - 1.0) > 1e-10)
                                sb.AppendLine($"LinetypeScale = {F(attDef.LinetypeScale)},");
                            if (!attDef.IsVisible)
                                sb.AppendLine($"IsVisible = false,");
                            if (attDef.Normal != Vector3.UnitZ)
                                sb.AppendLine($"Normal = new Vector3({F(attDef.Normal.X)}, {F(attDef.Normal.Y)}, {F(attDef.Normal.Z)}),");

                            sb.AppendLine("};");
                            sb.AppendLine($"block{SafeName(blockName)}.AttributeDefinitions.Add(attDef{SafeName(blockName)}{SafeName(attDef.Tag)});");
                        }
                        sb.AppendLine();
                    }

                    // Add block entities
                    if (block.Entities.Count > 0)
                    {
                        foreach (var entity in block.Entities)
                        {
                            // Generate simplified entity code for block entities with all entity types enabled
                            GenerateBlockEntity(sb, entity, blockName, blockEntityOptions);
                        }
                        sb.AppendLine();
                    }

                    sb.AppendLine($"doc.Blocks.Add(block{SafeName(blockName)});");
                    sb.AppendLine();
                }
            }
        }

        // Generate dimension style definitions (if any custom ones)
        if (options.GenerateDimensionStyles && _usedDimensionStyles.Any(ds => ds != "Standard"))
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Dimension style definitions");
            }
            foreach (var styleName in _usedDimensionStyles.Where(ds => ds != "Standard"))
            {
                var style = doc.DimensionStyles.FirstOrDefault(ds => ds.Name == styleName);
                if (style != null)
                {
                    sb.AppendLine($"var dimStyle{SafeName(styleName)} = new DimensionStyle(\"{Escape(styleName)}\");");
                    sb.AppendLine($"doc.DimensionStyles.Add(dimStyle{SafeName(styleName)});");
                }
            }
            sb.AppendLine();
        }

        // Generate multiline style definitions (if any custom ones)
        if (options.GenerateMLineStyles && _usedMLineStyles.Any(ms => ms != "Standard"))
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Multiline style definitions");
            }
            foreach (var styleName in _usedMLineStyles.Where(ms => ms != "Standard"))
            {
                var style = doc.MlineStyles.FirstOrDefault(ms => ms.Name == styleName);
                if (style != null)
                {
                    GenerateMLineStyle(sb, style);
                }
            }
            sb.AppendLine();
        }

        // Generate UCS definitions (if any custom ones)
        if (options.GenerateUCS && doc.UCSs.Count > 0)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// UCS definitions");
            }
            foreach (var ucs in doc.UCSs.Where(u => u.Name != "*ACTIVE"))
            {
                sb.AppendLine($"var ucs{SafeName(ucs.Name)} = new UCS(");
                sb.AppendLine($"\"{Escape(ucs.Name)}\",");
                sb.AppendLine($"new Vector3({F(ucs.Origin.X)}, {F(ucs.Origin.Y)}, {F(ucs.Origin.Z)}),");
                sb.AppendLine($"new Vector3({F(ucs.XAxis.X)}, {F(ucs.XAxis.Y)}, {F(ucs.XAxis.Z)}),");
                sb.AppendLine($"new Vector3({F(ucs.YAxis.X)}, {F(ucs.YAxis.Y)}, {F(ucs.YAxis.Z)}));");
                sb.AppendLine($"doc.UCSs.Add(ucs{SafeName(ucs.Name)});");
                sb.AppendLine();
            }
        }

        // Generate VPort definitions (modify the active viewport)
        if (options.GenerateVPorts && doc.Viewport != null)
        {
            var vport = doc.Viewport;
            var hasNonDefaultValues = false;
            var viewportCode = new StringBuilder();

            // Check if any properties differ from defaults and build the code
            if (vport.ViewCenter.X != 0 || vport.ViewCenter.Y != 0)
            {
                viewportCode.AppendLine($"activeViewport.ViewCenter = new Vector2({F(vport.ViewCenter.X)}, {F(vport.ViewCenter.Y)});");
                hasNonDefaultValues = true;
            }

            if (Math.Abs(vport.ViewHeight - 10) > 1e-6) // Default height is 10
            {
                viewportCode.AppendLine($"activeViewport.ViewHeight = {F(vport.ViewHeight)};");
                hasNonDefaultValues = true;
            }

            if (Math.Abs(vport.ViewAspectRatio - 1.0) > 1e-6) // Default aspect ratio is 1.0
            {
                viewportCode.AppendLine($"activeViewport.ViewAspectRatio = {F(vport.ViewAspectRatio)};");
                hasNonDefaultValues = true;
            }

            if (vport.ViewTarget.X != 0 || vport.ViewTarget.Y != 0 || vport.ViewTarget.Z != 0)
            {
                viewportCode.AppendLine($"activeViewport.ViewTarget = new Vector3({F(vport.ViewTarget.X)}, {F(vport.ViewTarget.Y)}, {F(vport.ViewTarget.Z)});");
                hasNonDefaultValues = true;
            }

            if (vport.ViewDirection.X != 0 || vport.ViewDirection.Y != 0 || Math.Abs(vport.ViewDirection.Z - 1) > 1e-6) // Default direction is UnitZ
            {
                viewportCode.AppendLine($"activeViewport.ViewDirection = new Vector3({F(vport.ViewDirection.X)}, {F(vport.ViewDirection.Y)}, {F(vport.ViewDirection.Z)});");
                hasNonDefaultValues = true;
            }

            if (!vport.ShowGrid) // Default is true
            {
                viewportCode.AppendLine($"activeViewport.ShowGrid = false;");
                hasNonDefaultValues = true;
            }

            if (vport.SnapMode) // Default is false
            {
                viewportCode.AppendLine($"activeViewport.SnapMode = true;");
                hasNonDefaultValues = true;
            }

            if (Math.Abs(vport.SnapSpacing.X - 0.5) > 1e-6 || Math.Abs(vport.SnapSpacing.Y - 0.5) > 1e-6) // Default is 0.5
            {
                viewportCode.AppendLine($"activeViewport.SnapSpacing = new Vector2({F(vport.SnapSpacing.X)}, {F(vport.SnapSpacing.Y)});");
                hasNonDefaultValues = true;
            }

            if (Math.Abs(vport.GridSpacing.X - 10.0) > 1e-6 || Math.Abs(vport.GridSpacing.Y - 10.0) > 1e-6) // Default is 10.0
            {
                viewportCode.AppendLine($"activeViewport.GridSpacing = new Vector2({F(vport.GridSpacing.X)}, {F(vport.GridSpacing.Y)});");
                hasNonDefaultValues = true;
            }

            if (vport.SnapBasePoint.X != 0 || vport.SnapBasePoint.Y != 0)
            {
                viewportCode.AppendLine($"activeViewport.SnapBasePoint = new Vector2({F(vport.SnapBasePoint.X)}, {F(vport.SnapBasePoint.Y)});");
                hasNonDefaultValues = true;
            }

            // Only generate the viewport variable and code if there are non-default values
            if (hasNonDefaultValues)
            {
                if (options.GenerateDetailedComments)
                {
                    sb.AppendLine($"// VPort (Viewport) configuration");
                }
                sb.AppendLine($"var activeViewport = doc.Viewport;");
                sb.Append(viewportCode);
                sb.AppendLine();
            }
        }

        // Generate ApplicationRegistry definitions (if any custom ones)
        if (options.GenerateApplicationRegistryObjects && doc.ApplicationRegistries.Count > 0)
        {
            var customAppRegs = doc.ApplicationRegistries.Where(ar => ar.Name != "ACAD").ToList();
            if (customAppRegs.Any())
            {
                if (options.GenerateDetailedComments)
                {
                    sb.AppendLine($"// ApplicationRegistry definitions");
                }
                foreach (var appReg in customAppRegs)
                {
                    sb.AppendLine($"var appReg{SafeName(appReg.Name)} = new ApplicationRegistry(\"{Escape(appReg.Name)}\");");
                    sb.AppendLine($"doc.ApplicationRegistries.Add(appReg{SafeName(appReg.Name)});");
                }
                sb.AppendLine();
            }
        }

        // Generate View definitions (if any custom ones)
        // Note: Views collection is internal in netDxf, but we can generate View objects
        // that could be used if the API becomes public in the future
        if (options.GenerateViews && _usedViews.Any())
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// View definitions (Views collection is internal in netDxf)");
                sb.AppendLine($"// These View objects are generated for reference but cannot be added to the document");
            }
            foreach (var viewName in _usedViews)
            {
                // Since we cannot access the actual View objects from the internal collection,
                // we generate placeholder View creation code with common properties
                GenerateViewPlaceholder(sb, viewName);
            }
            sb.AppendLine();
        }

        // Generate ShapeStyle definitions (if any custom ones)
        if (options.GenerateShapeStyleObjects && doc.ShapeStyles.Count > 0)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// ShapeStyle definitions");
            }
            foreach (var shapeStyle in doc.ShapeStyles)
            {
                sb.AppendLine($"var shapeStyle{SafeName(shapeStyle.Name)} = new ShapeStyle(\"{Escape(shapeStyle.Name)}\", \"{Escape(shapeStyle.File)}\");");
                sb.AppendLine($"doc.ShapeStyles.Add(shapeStyle{SafeName(shapeStyle.Name)});");
            }
            sb.AppendLine();
        }
    }

private void GenerateEntities(StringBuilder sb, List<EntityObject> entities, DxfCodeGenerationOptions options)
{
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Entities");
        }

        foreach (var entity in entities)
        {
            GenerateEntity(sb, entity, options);
        }
    }

    private void GenerateEntity(StringBuilder sb, EntityObject entity, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// {entity.GetType().Name}: {entity.Handle}");
        }

        // Check if this entity needs to be generated as a variable
        var needsVariable = _entitiesNeedingVariables.Contains(entity.Handle) || HasXData(entity);

        switch (entity)
        {
            case Line line when options.GenerateLineEntities:
                GenerateLine(sb, line, needsVariable);
                break;
            case Arc arc when options.GenerateArcEntities:
                GenerateArc(sb, arc, needsVariable);
                break;
            case Circle circle when options.GenerateCircleEntities:
                GenerateCircle(sb, circle, needsVariable);
                break;
            case PointEntity point when options.GeneratePointEntities:
                GeneratePoint(sb, point, needsVariable);
                break;
            case Polyline2D poly2d when options.GeneratePolyline2DEntities:
                GeneratePolyline2D(sb, poly2d);
                break;
            case Polyline3D poly3d when options.GeneratePolyline3DEntities:
                GeneratePolyline3D(sb, poly3d, needsVariable);
                break;
            case Spline spline when options.GenerateSplineEntities:
                GenerateSpline(sb, spline);
                break;
            case Text text when options.GenerateTextEntities:
                GenerateText(sb, text, needsVariable);
                break;
            case MText mtext when options.GenerateMTextEntities:
                GenerateMText(sb, mtext, needsVariable);
                break;
            case Ellipse ellipse when options.GenerateEllipseEntities:
                GenerateEllipse(sb, ellipse, needsVariable);
                break;
            case Insert insert when options.GenerateInsertEntities:
                GenerateInsert(sb, insert, needsVariable);
                break;
            case Hatch hatch when options.GenerateHatchEntities:
                GenerateHatch(sb, hatch, needsVariable);
                break;
            case Wipeout wipeout when options.GenerateWipeoutEntities:
                GenerateWipeout(sb, wipeout, needsVariable);
                break;
            case Leader leader when options.GenerateLeaderEntities:
                GenerateLeader(sb, leader, needsVariable);
                break;
            case Face3D face3d when options.GenerateFace3dEntities:
                GenerateFace3D(sb, face3d);
                break;
            case LinearDimension linearDim when options is { GenerateDimensionEntities: true, GenerateLinearDimensionEntities: true }:
                GenerateLinearDimension(sb, linearDim);
                break;
            case AlignedDimension alignedDim when options is { GenerateDimensionEntities: true, GenerateAlignedDimensionEntities: true }:
                GenerateAlignedDimension(sb, alignedDim);
                break;
            case RadialDimension radialDim when options is { GenerateDimensionEntities: true, GenerateRadialDimensionEntities: true }:
                GenerateRadialDimension(sb, radialDim);
                break;
            case DiametricDimension diametricDim when options is { GenerateDimensionEntities: true, GenerateDiametricDimensionEntities: true }:
                GenerateDiametricDimension(sb, diametricDim);
                break;
            case Angular2LineDimension angular2LineDim when options is { GenerateDimensionEntities: true, GenerateAngular2LineDimensionEntities: true }:
                GenerateAngular2LineDimension(sb, angular2LineDim);
                break;
            case Angular3PointDimension angular3PointDim when options is { GenerateDimensionEntities: true, GenerateAngular3PointDimensionEntities: true }:
                GenerateAngular3PointDimension(sb, angular3PointDim);
                break;
            case OrdinateDimension ordinateDim when options is { GenerateDimensionEntities: true, GenerateOrdinateDimensionEntities: true }:
                GenerateOrdinateDimension(sb, ordinateDim);
                break;
            case ArcLengthDimension arcLengthDim when options is { GenerateDimensionEntities: true, GenerateArcLengthDimensionEntities: true }:
                GenerateArcLengthDimension(sb, arcLengthDim);
                break;
            case Ray ray when options.GenerateRayEntities:
                GenerateRay(sb, ray);
                break;
            case XLine xline when options.GenerateXLineEntities:
                GenerateXLine(sb, xline);
                break;
            case Solid solid when options.GenerateSolidEntities:
                GenerateSolid(sb, solid);
                break;
            case MLine mline when options.GenerateMLineEntities:
                GenerateMLine(sb, mline, needsVariable);
                break;
            case Image image when options.GenerateImageEntities:
                GenerateImage(sb, image, needsVariable);
                break;
            case Mesh mesh when options.GenerateMeshEntities:
                GenerateMesh(sb, mesh, needsVariable);
                break;
            case PolyfaceMesh polyfaceMesh when options.GeneratePolyfaceMeshEntities:
                GeneratePolyfaceMesh(sb, polyfaceMesh, needsVariable);
                break;
            case PolygonMesh polygonMesh when options.GeneratePolygonMeshEntities:
                GeneratePolygonMesh(sb, polygonMesh);
                break;
            case Shape shape when options.GenerateShapeEntities:
                GenerateShape(sb, shape);
                break;

            case Tolerance tolerance when options.GenerateToleranceEntities:
                GenerateTolerance(sb, tolerance);
                break;
            case Trace trace when options.GenerateTraceEntities:
                GenerateTrace(sb, trace);
                break;
            case Underlay underlay when options.GenerateUnderlayEntities:
                GenerateUnderlay(sb, underlay);
                break;
            case Viewport viewport when options.GenerateViewportEntities:
                GenerateViewport(sb, viewport);
                break;
            default:
                if (options.GenerateDetailedComments)
                {
                    sb.AppendLine($"// Skipped entity type: {entity.GetType().Name}");
                }
                break;
        }
    }

private void GenerateObjects(StringBuilder sb, DxfDocument doc, DxfCodeGenerationOptions options)
{
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Objects");
        }

        // Generate Groups
        if (options.GenerateGroupObjects && doc.Groups.Count > 0)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Groups");
            }
            foreach (var group in doc.Groups)
            {
                GenerateGroup(sb, group, options);
            }
            sb.AppendLine();
        }

        // Generate Layouts
        if (options.GenerateLayoutObjects && doc.Layouts.Count > 0)
        {
            // Skip the default "Model" layout as it's automatically created
            var customLayouts = doc.Layouts.Where(layout => !string.Equals(layout.Name, "Model", StringComparison.OrdinalIgnoreCase)).ToList();
            if (customLayouts.Any())
            {
                if (options.GenerateDetailedComments)
                {
                    sb.AppendLine($"// Layouts");
                }
                foreach (var layout in customLayouts)
                {
                    GenerateLayout(sb, layout, options);
                }
                sb.AppendLine();
            }
        }

        // Generate Image Definitions
        if (options.GenerateImageDefinitionObjects)
        {
            var imageDefinitions = doc.ImageDefinitions.Items.Where(item => item != null).ToList();
            if (imageDefinitions.Any())
            {
                if (options.GenerateDetailedComments)
                {
                    sb.AppendLine($"// Image Definitions");
                }
                foreach (var imageDef in imageDefinitions)
                {
                    GenerateImageDefinition(sb, imageDef, options);
                }
                sb.AppendLine();
            }
        }

        // Generate Underlay Definitions
        if (options.GenerateUnderlayDefinitionObjects)
        {
            var underlayDefinitions = doc.UnderlayDgnDefinitions.Items
                .Concat(doc.UnderlayDwfDefinitions.Items.Cast<UnderlayDefinition>())
                .Concat(doc.UnderlayPdfDefinitions.Items)
                .Where(item => item != null)
                .ToList();
            if (underlayDefinitions.Any())
            {
                if (options.GenerateDetailedComments)
                {
                    sb.AppendLine($"// Underlay Definitions");
                }
                foreach (var underlayDef in underlayDefinitions)
                {
                    GenerateUnderlayDefinition(sb, underlayDef, options);
                }
                sb.AppendLine();
            }
        }

        // Generate RasterVariables
        if (options.GenerateRasterVariablesObjects && doc.RasterVariables != null)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Raster Variables");
            }
            GenerateRasterVariables(sb, doc.RasterVariables, options);
            sb.AppendLine();
        }

        // Generate LayerState objects
        if (options.GenerateLayerStateObjects)
        {
            GenerateLayerStatePlaceholder(sb, options);
        }

        // Generate PlotSettings objects
        if (options.GeneratePlotSettingsObjects)
        {
            GeneratePlotSettingsPlaceholder(sb, options);
        }

        // Generate XRecord objects
        if (options.GenerateXRecordObjects)
        {
            GenerateXRecordPlaceholder(sb, options);
        }

        // Generate Dictionary objects
        if (options.GenerateDictionaryObjects)
        {
            GenerateDictionaryObjectPlaceholder(sb, options);
        }

        // Generate MLineStyle objects
        if (options is { GenerateMLineStyleObjects: true, GenerateDetailedComments: true })
        // Note: MLineStyle objects are typically stored in dictionaries
        // This is a placeholder for when MLineStyle access is available
        {
            sb.AppendLine($"// MLineStyle objects (stored in dictionaries - not directly accessible)");
        }
    }

    private void GenerateLine(StringBuilder sb, Line line, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{line.Handle} = ");
            GenerateLineConstructor(sb, line);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, line);
            sb.AppendLine($"}};");
            GenerateXData(sb, line, $"entity{line.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{line.Handle});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add(");
            GenerateLineConstructor(sb, line);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, line);
            sb.AppendLine($"}}");
            sb.AppendLine($");");
        }
    }

    private void GenerateLineConstructor(StringBuilder sb, Line line)
    {
        sb.AppendLine($"new Line(");
        sb.AppendLine($"new Vector3({F(line.StartPoint.X)}, {F(line.StartPoint.Y)}, {F(line.StartPoint.Z)}),");
        sb.AppendLine($"new Vector3({F(line.EndPoint.X)}, {F(line.EndPoint.Y)}, {F(line.EndPoint.Z)}))");
    }

    private void GenerateArc(StringBuilder sb, Arc arc, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{arc.Handle} = ");
            GenerateArcConstructor(sb, arc);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, arc);
            sb.AppendLine($"}};");
            GenerateXData(sb, arc, $"entity{arc.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{arc.Handle});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add(");
            GenerateArcConstructor(sb, arc);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, arc);
            sb.AppendLine($"}}");
            sb.AppendLine($");");
        }
    }

    private void GenerateArcConstructor(StringBuilder sb, Arc arc)
    {
        sb.AppendLine($"new Arc(");
        sb.AppendLine($"new Vector3({F(arc.Center.X)}, {F(arc.Center.Y)}, {F(arc.Center.Z)}),");
        sb.AppendLine($"{F(arc.Radius)}, {F(arc.StartAngle)}, {F(arc.EndAngle)})");
    }

    private void GenerateCircle(StringBuilder sb, Circle circle, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{circle.Handle} = ");
            GenerateCircleConstructor(sb, circle);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, circle);
            sb.AppendLine($"}};");
            GenerateXData(sb, circle, $"entity{circle.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{circle.Handle});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add(");
            GenerateCircleConstructor(sb, circle);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, circle);
            sb.AppendLine($"}}");
            sb.AppendLine($");");
        }
    }

    private void GenerateCircleConstructor(StringBuilder sb, Circle circle)
    {
        sb.AppendLine($"new Circle(");
        sb.AppendLine($"new Vector3({F(circle.Center.X)}, {F(circle.Center.Y)}, {F(circle.Center.Z)}),");
        sb.AppendLine($"{F(circle.Radius)})");
    }

    private void GeneratePoint(StringBuilder sb, PointEntity point, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{point.Handle} = ");
            sb.AppendLine($"new Point(");
            sb.AppendLine($"new Vector3({F(point.Position.X)}, {F(point.Position.Y)}, {F(point.Position.Z)}))");
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, point);
            sb.AppendLine($"}};");
            GenerateXData(sb, point, $"entity{point.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{point.Handle});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add(");
            sb.AppendLine($"new Point(");
            sb.AppendLine($"new Vector3({F(point.Position.X)}, {F(point.Position.Y)}, {F(point.Position.Z)}))");
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, point);
            sb.AppendLine($"}}");
            sb.AppendLine($");");
        }
    }

    private void GeneratePolyline2D(StringBuilder sb, Polyline2D poly2d)
    {
        sb.AppendLine($"doc.Entities.Add(");
        GeneratePolyline2DConstructor(sb, poly2d);
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, poly2d);
        if (Math.Abs(poly2d.Elevation) > 1e-12)
        {
            sb.AppendLine($"Elevation = {F(poly2d.Elevation)},");
        }
        if (poly2d.LinetypeGeneration)
        {
            sb.AppendLine($"LinetypeGeneration = true,");
        }
        if (poly2d.SmoothType != PolylineSmoothType.NoSmooth)
        {
            sb.AppendLine($"SmoothType = PolylineSmoothType.{poly2d.SmoothType},");
        }
        sb.AppendLine($"}}");
        sb.AppendLine($");");
    }

    private void GeneratePolyline2DConstructor(StringBuilder sb, Polyline2D poly2d)
    {
        sb.AppendLine($"new Polyline2D(new List<Polyline2DVertex>()");
        sb.AppendLine($"{{");
        foreach (var vertex in poly2d.Vertexes)
        {
            var bulgeStr = Math.Abs(vertex.Bulge) > 1e-12 ? $" {{ Bulge = {F(vertex.Bulge)} }}" : "";
            sb.AppendLine($"new Polyline2DVertex({F(vertex.Position.X)}, {F(vertex.Position.Y)}){bulgeStr},");
        }
        sb.AppendLine($"}}, {(poly2d.IsClosed ? "true" : "false")})");
    }

    private void GeneratePolyline3D(StringBuilder sb, Polyline3D poly3d, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var polyline3DEntity = new Polyline3D(new List<Vector3>()");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add(new Polyline3D(new List<Vector3>()");
        }
        sb.AppendLine($"{{");
        foreach (var v in poly3d.Vertexes)
        {
            sb.AppendLine($"new Vector3({F(v.X)}, {F(v.Y)}, {F(v.Z)}),");
        }
        sb.AppendLine($"}}, {(poly3d.IsClosed ? "true" : "false")})");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, poly3d);
        if (poly3d.LinetypeGeneration)
        {
            sb.AppendLine($"LinetypeGeneration = true,");
        }
        if (poly3d.SmoothType != PolylineSmoothType.NoSmooth)
        {
            sb.AppendLine($"SmoothType = PolylineSmoothType.{poly3d.SmoothType},");
        }
        sb.AppendLine($"}}");
        if (asVariable)
        {
            sb.AppendLine($");");
        }
        else
        {
            sb.AppendLine($");");
        }
    }

    private void GenerateSpline(StringBuilder sb, Spline spline)
    {
        // Generate spline with complete definition: control points, weights, and degree
        sb.AppendLine($"{{");
        sb.AppendLine($"var controlPoints = new List<Vector3>");
        sb.AppendLine($"{{");
        foreach (var cp in spline.ControlPoints)
        {
            sb.AppendLine($"new Vector3({F(cp.X)}, {F(cp.Y)}, {F(cp.Z)}),");
        }
        sb.AppendLine($"}};");
        sb.AppendLine();

        sb.AppendLine($"var weights = new List<double>");
        sb.AppendLine($"{{");
        foreach (var weight in spline.Weights)
        {
            sb.AppendLine($"{F(weight)},");
        }
        sb.AppendLine($"}};");
        sb.AppendLine();

        sb.AppendLine($"var splineEntity = new Spline(controlPoints, weights, {spline.Degree});");

        // Apply entity properties
        if (spline.Layer != null && _usedLayers.Contains(spline.Layer.Name))
        {
            sb.AppendLine($"splineEntity.Layer = layer{SafeName(spline.Layer.Name)};");
        }

        // Color handling
        if (spline.Color.Index != 256) // Not ByLayer
        {
            if (spline.Color.Index == 0)
                sb.AppendLine($"splineEntity.Color = AciColor.ByBlock;");
            else if (spline.Color.Index is >= 1 and <= 255)
                sb.AppendLine($"splineEntity.Color = new AciColor({spline.Color.Index});");
        }

        // Linetype
        if (spline.Linetype != null && spline.Linetype.Name != "ByLayer" && spline.Linetype.Name != "Continuous")
        {
            if (spline.Linetype.Name == "ByBlock")
                sb.AppendLine($"splineEntity.Linetype = Linetype.ByBlock;");
            else
                sb.AppendLine($"splineEntity.Linetype = linetype{SafeName(spline.Linetype.Name)};");
        }

        // Lineweight
        if (spline.Lineweight != Lineweight.ByLayer)
        {
            sb.AppendLine($"splineEntity.Lineweight = Lineweight.{spline.Lineweight};");
        }

        // Linetype scale
        if (Math.Abs(spline.LinetypeScale - 1.0) > 1e-10)
        {
            sb.AppendLine($"splineEntity.LinetypeScale = {F(spline.LinetypeScale)};");
        }

        // Advanced spline properties
        GenerateSplineAdvancedProperties(sb, spline);

        GenerateXData(sb, spline, "splineEntity");
        sb.AppendLine($"doc.Entities.Add(splineEntity);");
        sb.AppendLine($"}}");
    }

    private void GenerateSplineConstructor(StringBuilder sb, Spline spline)
    {
        sb.AppendLine($"new Spline(");
        sb.AppendLine($"new List<Vector3>()");
        sb.AppendLine($"{{");
        foreach (var cp in spline.ControlPoints)
        {
            sb.AppendLine($"new Vector3({F(cp.X)}, {F(cp.Y)}, {F(cp.Z)}),");
        }
        sb.AppendLine($"}},");
        sb.AppendLine($"new List<double>()");
        sb.AppendLine($"{{");
        foreach (var weight in spline.Weights)
        {
            sb.AppendLine($"{F(weight)},");
        }
        sb.AppendLine($"}},");
        sb.AppendLine($"{spline.Degree})");
    }

    private void GenerateText(StringBuilder sb, Text text, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{text.Handle} = ");
            sb.AppendLine($"new Text(");
            sb.AppendLine($"\"{Escape(text.Value)}\",");
            sb.AppendLine($"new Vector3({F(text.Position.X)}, {F(text.Position.Y)}, {F(text.Position.Z)}),");
            sb.AppendLine($"{F(text.Height)})");
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, text);
            GenerateTextAdvancedProperties(sb, text);
            sb.AppendLine($"}};");
            GenerateXData(sb, text, $"entity{text.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{text.Handle});");
        }
        else
        {
            sb.AppendLine("doc.Entities.Add(");
            sb.AppendLine("new Text(");
            sb.AppendLine($"\"{Escape(text.Value)}\",");
            sb.AppendLine($"new Vector3({F(text.Position.X)}, {F(text.Position.Y)}, {F(text.Position.Z)}),");
            sb.AppendLine($"{F(text.Height)})");
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, text);
            GenerateTextAdvancedProperties(sb, text);
            sb.AppendLine($"}}");
            sb.AppendLine($");");
        }
    }

    private void GenerateMText(StringBuilder sb, MText mtext, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{mtext.Handle} = ");
            sb.AppendLine($"new MText(");
            sb.AppendLine($"\"{EscapeMText(mtext.Value)}\",");
            sb.AppendLine($"new Vector3({F(mtext.Position.X)}, {F(mtext.Position.Y)}, {F(mtext.Position.Z)}),");
            sb.AppendLine($"{F(mtext.Height)})");
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, mtext);
            if (mtext.Style?.Name != "Standard")
            {
                if (mtext.Style != null)
                {
                    sb.AppendLine($"Style = textStyle{SafeName(mtext.Style.Name)},");
                }
            }
            if (mtext.RectangleWidth > 0)
            {
                sb.AppendLine($"RectangleWidth = {F(mtext.RectangleWidth)},");
            }
            if (mtext.AttachmentPoint != MTextAttachmentPoint.TopLeft)
            {
                sb.AppendLine($"AttachmentPoint = MTextAttachmentPoint.{mtext.AttachmentPoint},");
            }
            if (Math.Abs(mtext.Rotation) > 1e-12)
            {
                sb.AppendLine($"Rotation = {F(mtext.Rotation)},");
            }
            if (Math.Abs(mtext.LineSpacingFactor - 1.0) > 1e-12)
            {
                sb.AppendLine($"LineSpacingFactor = {F(mtext.LineSpacingFactor)},");
            }
            if (mtext.LineSpacingStyle != MTextLineSpacingStyle.AtLeast)
            {
                sb.AppendLine($"LineSpacingStyle = MTextLineSpacingStyle.{mtext.LineSpacingStyle},");
            }
            if (mtext.DrawingDirection != MTextDrawingDirection.ByStyle)
            {
                sb.AppendLine($"DrawingDirection = MTextDrawingDirection.{mtext.DrawingDirection},");
            }
            sb.AppendLine($"}};");
            GenerateXData(sb, mtext, $"entity{mtext.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{mtext.Handle});");
        }
        else
        {
            sb.AppendLine("doc.Entities.Add(");
            sb.AppendLine("new MText(");
            sb.AppendLine($"\"{EscapeMText(mtext.Value)}\",");
            sb.AppendLine($"new Vector3({F(mtext.Position.X)}, {F(mtext.Position.Y)}, {F(mtext.Position.Z)}),");
            sb.AppendLine($"{F(mtext.Height)})");
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, mtext);
            if (mtext.Style?.Name != "Standard")
            {
                if (mtext.Style != null)
                {
                    sb.AppendLine($"Style = textStyle{SafeName(mtext.Style.Name!)},");
                }
            }
            if (mtext.RectangleWidth > 0)
            {
                sb.AppendLine($"RectangleWidth = {F(mtext.RectangleWidth)},");
            }
            if (mtext.AttachmentPoint != MTextAttachmentPoint.TopLeft)
            {
                sb.AppendLine($"AttachmentPoint = MTextAttachmentPoint.{mtext.AttachmentPoint},");
            }
            if (Math.Abs(mtext.Rotation) > 1e-12)
            {
                sb.AppendLine($"Rotation = {F(mtext.Rotation)},");
            }
            if (Math.Abs(mtext.LineSpacingFactor - 1.0) > 1e-12)
            {
                sb.AppendLine($"LineSpacingFactor = {F(mtext.LineSpacingFactor)},");
            }
            if (mtext.LineSpacingStyle != MTextLineSpacingStyle.AtLeast)
            {
                sb.AppendLine($"LineSpacingStyle = MTextLineSpacingStyle.{mtext.LineSpacingStyle},");
            }
            if (mtext.DrawingDirection != MTextDrawingDirection.ByStyle)
            {
                sb.AppendLine($"DrawingDirection = MTextDrawingDirection.{mtext.DrawingDirection},");
            }
            sb.AppendLine($"}}");
            sb.AppendLine($");");
        }
    }

    private void GenerateEllipse(StringBuilder sb, Ellipse ellipse, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{ellipse.Handle} = ");
            GenerateEllipseConstructor(sb, ellipse);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, ellipse);
            if (Math.Abs(ellipse.StartAngle) > 1e-12 || Math.Abs(ellipse.EndAngle - 360) > 1e-12)
            {
                sb.AppendLine($"StartAngle = {F(ellipse.StartAngle)},");
                sb.AppendLine($"EndAngle = {F(ellipse.EndAngle)},");
            }
            sb.AppendLine($"}};");
            GenerateXData(sb, ellipse, $"entity{ellipse.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{ellipse.Handle});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add(");
            GenerateEllipseConstructor(sb, ellipse);
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, ellipse);
            if (Math.Abs(ellipse.StartAngle) > 1e-12 || Math.Abs(ellipse.EndAngle - 360) > 1e-12)
            {
                sb.AppendLine($"StartAngle = {F(ellipse.StartAngle)},");
                sb.AppendLine($"EndAngle = {F(ellipse.EndAngle)},");
            }
            sb.AppendLine($"}}");
            sb.AppendLine($");");
        }
    }

    private void GenerateEllipseConstructor(StringBuilder sb, Ellipse ellipse)
    {
        sb.AppendLine($"new Ellipse(");
        sb.AppendLine($"new Vector3({F(ellipse.Center.X)}, {F(ellipse.Center.Y)}, {F(ellipse.Center.Z)}),");
        sb.AppendLine($"{F(ellipse.MajorAxis)}, {F(ellipse.MinorAxis)})");
    }

private void GenerateEntityPropertiesCore(StringBuilder sb, EntityObject entity)
{
        // Layer
        if (entity.Layer != null && _usedLayers.Contains(entity.Layer.Name))
        {
            sb.AppendLine($"Layer = layer{SafeName(entity.Layer.Name)},");
        }

        // Color (if not ByLayer)
        if (entity.Color.UseTrueColor)
        {
            sb.AppendLine($"Color = AciColor.FromTrueColor(System.Drawing.Color.FromArgb({entity.Color.R}, {entity.Color.G}, {entity.Color.B}).ToArgb()),");
        }
        else if (entity.Color.Index != 256) // 256 = ByLayer
        {
            if (entity.Color.Index == 0)
                sb.AppendLine($"Color = AciColor.ByBlock,");
            else if (entity.Color.Index is >= 1 and <= 255)
                sb.AppendLine($"Color = new AciColor({entity.Color.Index}),");
        }

        // Linetype
        if (entity.Linetype != null && entity.Linetype.Name != "ByLayer" && entity.Linetype.Name != "Continuous")
        {
            if (entity.Linetype.Name == "ByBlock")
            {
                sb.AppendLine($"Linetype = Linetype.ByBlock,");
            }
            else
            {
                sb.AppendLine($"Linetype = linetype{SafeName(entity.Linetype.Name)},");
            }
        }

        // Lineweight
        if (entity.Lineweight != Lineweight.ByLayer)
        {
            sb.AppendLine($"Lineweight = Lineweight.{entity.Lineweight},");
        }

        // Linetype scale
        if (Math.Abs(entity.LinetypeScale - 1.0) > 1e-10)
        {
            sb.AppendLine($"LinetypeScale = {F(entity.LinetypeScale)},");
        }

        // Thickness (for entities that support it)
        if (entity is Line line && Math.Abs(line.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(line.Thickness)},");
        }
        else if (entity is Arc arc && Math.Abs(arc.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(arc.Thickness)},");
        }
        else if (entity is Circle circle && Math.Abs(circle.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(circle.Thickness)},");
        }
        else if (entity is Polyline2D poly2d && Math.Abs(poly2d.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(poly2d.Thickness)},");
        }
        else if (entity is PointEntity point && Math.Abs(point.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(point.Thickness)},");
        }
        else if (entity is Ellipse ellipse && Math.Abs(ellipse.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(ellipse.Thickness)},");
        }
        else if (entity is Shape shape && Math.Abs(shape.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(shape.Thickness)},");
        }
        else if (entity is Trace trace && Math.Abs(trace.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(trace.Thickness)},");
        }
        else if (entity is Solid solid && Math.Abs(solid.Thickness) > 1e-10)
        {
            sb.AppendLine($"Thickness = {F(solid.Thickness)},");
        }

        // Transparency (if not default ByLayer)
        if (entity.Transparency.Value != -1)
        {
            if (entity.Transparency.Value == 100)
            {
                sb.AppendLine($"Transparency = Transparency.ByBlock,");
            }
            else if (entity.Transparency.Value is >= 0 and <= 90)
            {
                sb.AppendLine($"Transparency = new Transparency({entity.Transparency.Value}),");
            }
        }

        // IsVisible (if not default true)
        if (!entity.IsVisible)
        {
            sb.AppendLine($"IsVisible = false,");
        }

        // Normal (if not default 0,0,1)
        var n = entity.Normal;
        if (Math.Abs(n.X) > 1e-12 || Math.Abs(n.Y) > 1e-12 || Math.Abs(n.Z - 1.0) > 1e-12)
        {
            sb.AppendLine($"Normal = new Vector3({F(n.X)}, {F(n.Y)}, {F(n.Z)}),");
        }

        // Reactors (if any exist)
        if (entity.Reactors is { Count: > 0 })
        {
            sb.AppendLine($"// Note: Reactors property is read-only and managed internally by netDxf");
            sb.AppendLine($"// {entity.Reactors.Count} reactor(s) attached to this entity");
        }
    }

    private void GenerateAttributeProperties(StringBuilder sb, Attribute attribute)
    {
        // Generate Layer property
        if (attribute.Layer != null && attribute.Layer.Name != "0")
        {
            sb.AppendLine($"Layer = layer{SafeName(attribute.Layer.Name)},");
        }

        // Generate Color property
        if (attribute.Color.Index != 256) // 256 is ByLayer
        {
            sb.AppendLine($"Color = new AciColor({attribute.Color.Index}),");
        }

        // Generate Linetype property
        if (attribute.Linetype != null && attribute.Linetype.Name != "ByLayer" && attribute.Linetype.Name != "Continuous")
        {
            if (attribute.Linetype.Name == "ByBlock")
                sb.AppendLine($"Linetype = Linetype.ByBlock,");
            else
                sb.AppendLine($"Linetype = linetype{SafeName(attribute.Linetype.Name)},");
        }

        // Generate Lineweight property
        if (attribute.Lineweight != Lineweight.ByLayer)
        {
            sb.AppendLine($"Lineweight = Lineweight.{attribute.Lineweight},");
        }

        // Generate LinetypeScale property
        if (Math.Abs(attribute.LinetypeScale - 1.0) > 1e-6)
        {
            sb.AppendLine($"LinetypeScale = {attribute.LinetypeScale.ToString(CultureInfo.InvariantCulture)},");
        }

        // Generate Transparency property
        if (attribute.Transparency.Value != 0)
        {
            sb.AppendLine($"Transparency = new Transparency({attribute.Transparency.Value}),");
        }

        // Generate IsVisible property
        if (!attribute.IsVisible)
        {
            sb.AppendLine($"IsVisible = false,");
        }

        // Generate Normal property (only if not default Z-axis)
        if (Math.Abs(attribute.Normal.X) > 1e-6 || Math.Abs(attribute.Normal.Y) > 1e-6 || Math.Abs(attribute.Normal.Z - 1.0) > 1e-6)
        {
            sb.AppendLine($"Normal = new Vector3({attribute.Normal.X.ToString(CultureInfo.InvariantCulture)}, {attribute.Normal.Y.ToString(CultureInfo.InvariantCulture)}, {attribute.Normal.Z.ToString(CultureInfo.InvariantCulture)}),");
        }
    }

    private void GenerateBlockEntity(StringBuilder sb, EntityObject entity, string blockName, DxfCodeGenerationOptions options)
    {
        // Reuse existing entity generation methods by temporarily modifying the output
        var tempSb = new StringBuilder();
        GenerateEntity(tempSb, entity, options);

        // Replace "doc.Entities.Add(" with "block{blockName}.Entities.Add("
        var entityCode = tempSb.ToString();
        var modifiedCode = entityCode.Replace("doc.Entities.Add(", $"block{SafeName(blockName)}.Entities.Add(");
        sb.Append(modifiedCode);
    }

    private void GenerateInsert(StringBuilder sb, Insert insert, bool asVariable = false)
    {
        var blkName = insert.Block?.Name;
        if (string.IsNullOrEmpty(blkName))
        {
            return;
        }
        var safeBlk = SafeName(blkName);
        var insertVarName = $"ins_{safeBlk}_{_insertCounter++}";

        if (asVariable)
        {
            sb.AppendLine($"var entity{insert.Handle} = new Insert(block{safeBlk})");
            sb.AppendLine($"{{");
            sb.AppendLine($"Position = new Vector3({F(insert.Position.X)}, {F(insert.Position.Y)}, {F(insert.Position.Z)}),");
            if (Math.Abs(insert.Scale.X - 1.0) > 1e-12 || Math.Abs(insert.Scale.Y - 1.0) > 1e-12 || Math.Abs(insert.Scale.Z - 1.0) > 1e-12)
                sb.AppendLine($"Scale = new Vector3({F(insert.Scale.X)}, {F(insert.Scale.Y)}, {F(insert.Scale.Z)}),");
            if (Math.Abs(insert.Rotation) > 1e-12)
                sb.AppendLine($"Rotation = {F(insert.Rotation)},");
            if (insert.Layer != null && _usedLayers.Contains(insert.Layer.Name))
                sb.AppendLine($"Layer = layer{SafeName(insert.Layer.Name)},");
            sb.AppendLine($"}};");

            // Attributes
            if (insert.Attributes is { Count: > 0 })
            {
                foreach (var att in insert.Attributes)
                {
                    // Assign value by tag when possible
                    if (!string.IsNullOrEmpty(att.Tag))
                    {
                        sb.AppendLine($"var attr_{SafeName(att.Tag)} = entity{insert.Handle}.Attributes.AttributeWithTag(\"{Escape(att.Tag)}\");");
                        sb.AppendLine($"if (attr_{SafeName(att.Tag)} != null) attr_{SafeName(att.Tag)}.Value = \"{Escape(att.Value)}\";");
                    }
                }
            }

            GenerateXData(sb, insert, $"entity{insert.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{insert.Handle});");
        }
        else
        {
            sb.AppendLine($"var {insertVarName} = new Insert(block{safeBlk})");
            sb.AppendLine($"{{");
            sb.AppendLine($"Position = new Vector3({F(insert.Position.X)}, {F(insert.Position.Y)}, {F(insert.Position.Z)}),");
            if (Math.Abs(insert.Scale.X - 1.0) > 1e-12 || Math.Abs(insert.Scale.Y - 1.0) > 1e-12 || Math.Abs(insert.Scale.Z - 1.0) > 1e-12)
                sb.AppendLine($"Scale = new Vector3({F(insert.Scale.X)}, {F(insert.Scale.Y)}, {F(insert.Scale.Z)}),");
            if (Math.Abs(insert.Rotation) > 1e-12)
                sb.AppendLine($"Rotation = {F(insert.Rotation)},");
            if (insert.Layer != null && _usedLayers.Contains(insert.Layer.Name))
                sb.AppendLine($"Layer = layer{SafeName(insert.Layer.Name)},");
            sb.AppendLine($"}};");

            // Attributes
            if (insert.Attributes is { Count: > 0 })
            {
                foreach (var att in insert.Attributes)
                {
                    // Assign value by tag when possible
                    if (!string.IsNullOrEmpty(att.Tag))
                    {
                        sb.AppendLine($"var attr_{SafeName(att.Tag)} = {insertVarName}.Attributes.AttributeWithTag(\"{Escape(att.Tag)}\");");
                        sb.AppendLine($"if (attr_{SafeName(att.Tag)} != null) attr_{SafeName(att.Tag)}.Value = \"{Escape(att.Value)}\";");
                    }
                }
            }

            sb.AppendLine($"doc.Entities.Add({insertVarName});");
        }
        sb.AppendLine();
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string EscapeMText(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\P");

    private static string SafeName(string name)
    {
        // Convert layer/table names to safe C# identifiers
        var result = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c))
                result.Append(c);
            else
                result.Append('_');
        }
        var safeName = result.ToString();
        if (char.IsDigit(safeName[0]))
            safeName = "_" + safeName;
        return safeName;
    }

    private void GenerateHatch(StringBuilder sb, Hatch hatch, bool asVariable = false)
    {
        // Generate Hatch with boundary paths reconstruction
        var patternName = hatch.Pattern?.Name ?? "SOLID";
        var isSolid = string.Equals(patternName, "SOLID", StringComparison.OrdinalIgnoreCase) || string.Equals(patternName, "Solid", StringComparison.OrdinalIgnoreCase);

        sb.AppendLine($"{{");
        sb.AppendLine($"// Hatch with boundary paths");
        sb.AppendLine($"var boundaryPaths = new List<HatchBoundaryPath>();");

        // Reconstruct boundary paths from existing hatch boundary paths
        if (hatch.BoundaryPaths is { Count: > 0 })
        {
            for (var i = 0; i < hatch.BoundaryPaths.Count; i++)
            {
                var path = hatch.BoundaryPaths[i];
                sb.AppendLine($"// Boundary path {i + 1}");
                sb.AppendLine($"var pathEntities{i} = new List<EntityObject>();");

                // Extract entities from the boundary path edges
                if (path.Edges.Count > 0)
                {
                    foreach (var edge in path.Edges)
                    {
                        var entityObj = edge.ConvertTo();
                        if (entityObj != null)
                        {
                            GenerateBoundaryEntity(sb, entityObj, i);
                        }
                    }
                }

                sb.AppendLine($"boundaryPaths.Add(new HatchBoundaryPath(pathEntities{i}));");
                sb.AppendLine();
            }
        }

        // Create the hatch with the pattern and boundaries
        if (isSolid)
        {
            sb.AppendLine($"var hatchEntity = new Hatch(HatchPattern.Solid, boundaryPaths, {(hatch.Associative ? "true" : "false")});");
        }
        else
        {
            // Check if it's a gradient pattern
            if (hatch.Pattern is HatchGradientPattern gradientPattern)
            {
                sb.AppendLine($"var gradientPattern = new HatchGradientPattern(");
                if (gradientPattern.SingleColor)
                {
                    sb.AppendLine($"new AciColor({gradientPattern.Color1.Index}), {F(gradientPattern.Tint)}, HatchGradientPatternType.{gradientPattern.GradientType});");
                }
                else
                {
                    sb.AppendLine($"new AciColor({gradientPattern.Color1.Index}), new AciColor({gradientPattern.Color2.Index}), HatchGradientPatternType.{gradientPattern.GradientType});");
                }

                // Set gradient-specific properties if they differ from defaults
                if (Math.Abs(gradientPattern.Angle) > 1e-9)
                    sb.AppendLine($"gradientPattern.Angle = {F(gradientPattern.Angle)};");
                if (Math.Abs(gradientPattern.Scale - 1.0) > 1e-9)
                    sb.AppendLine($"gradientPattern.Scale = {F(gradientPattern.Scale)};");
                if (Math.Abs(gradientPattern.Origin.X) > 1e-9 || Math.Abs(gradientPattern.Origin.Y) > 1e-9)
                    sb.AppendLine($"gradientPattern.Origin = new Vector2({F(gradientPattern.Origin.X)}, {F(gradientPattern.Origin.Y)});");
                if (!gradientPattern.Centered)
                    sb.AppendLine($"gradientPattern.Centered = false;");

                sb.AppendLine($"var hatchEntity = new Hatch(gradientPattern, boundaryPaths, {(hatch.Associative ? "true" : "false")});");
            }
            else
            {
                sb.AppendLine($"var pattern = new HatchPattern(\"{Escape(patternName)}\");");

                if (hatch.Pattern != null)
                {
                    // Set pattern properties if they differ from defaults
                    if (Math.Abs(hatch.Pattern.Angle) > 1e-9)
                    {
                        sb.AppendLine($"pattern.Angle = {F(hatch.Pattern.Angle)};");
                    }

                    if (Math.Abs(hatch.Pattern.Scale - 1.0) > 1e-9)
                    {
                        sb.AppendLine($"pattern.Scale = {F(hatch.Pattern.Scale)};");
                    }

                    if (Math.Abs(hatch.Pattern.Origin.X) > 1e-9 || Math.Abs(hatch.Pattern.Origin.Y) > 1e-9)
                    {
                        sb.AppendLine(
                            $"pattern.Origin = new Vector2({F(hatch.Pattern.Origin.X)}, {F(hatch.Pattern.Origin.Y)});");
                    }
                }

                sb.AppendLine($"var hatchEntity = new Hatch(pattern, boundaryPaths, {(hatch.Associative ? "true" : "false")});");
            }
        }

        sb.AppendLine($"hatchEntity.Elevation = {F(hatch.Elevation)};");

        // Apply entity properties
        if (hatch.Layer != null && _usedLayers.Contains(hatch.Layer.Name))
        {
            sb.AppendLine($"hatchEntity.Layer = layer{SafeName(hatch.Layer.Name)};");
        }

        // Color handling
        if (hatch.Color.Index != 256) // Not ByLayer
        {
            if (hatch.Color.Index == 0)
                sb.AppendLine($"hatchEntity.Color = AciColor.ByBlock;");
            else
                sb.AppendLine($"hatchEntity.Color = new AciColor({hatch.Color.Index});");
        }

        if (!asVariable)
        {
            sb.AppendLine($"doc.Entities.Add(hatchEntity);");
        }
        else
        {
            // Allow attaching additional data (e.g., XData) before adding
            GenerateXData(sb, hatch, "hatchEntity");
            sb.AppendLine($"doc.Entities.Add(hatchEntity);");
        }
        sb.AppendLine($"}}");
        sb.AppendLine();
    }

private void GenerateBoundaryEntity(StringBuilder sb, EntityObject entity, int pathIndex)
{
        // Generate entity creation code for boundary paths without properties
        sb.AppendLine($"pathEntities{pathIndex}.Add(");

        switch (entity)
        {
            case Line line:
                GenerateLineConstructor(sb, line);
                break;
            case Arc arc:
                GenerateArcConstructor(sb, arc);
                break;
            case Circle circle:
                GenerateCircleConstructor(sb, circle);
                break;
            case Polyline2D poly2d:
                GeneratePolyline2DConstructor(sb, poly2d);
                break;
            case Ellipse ellipse:
                GenerateEllipseConstructor(sb, ellipse);
                break;
            case Spline spline:
                GenerateSplineConstructor(sb, spline);
                break;
            default:
                // Fallback for other entity types
                sb.AppendLine($"// Unsupported boundary entity type: {entity.GetType().Name}");
                sb.AppendLine($"null");
                break;
        }

        sb.AppendLine($");");
    }

    private void GenerateWipeout(StringBuilder sb, Wipeout wipeout, bool asVariable = false)
    {
        sb.AppendLine($"{{");
        sb.AppendLine($"// Wipeout entity with clipping boundary");
        sb.AppendLine($"var boundaryVertices = new List<Vector2>();");

        if (wipeout.ClippingBoundary != null && wipeout.ClippingBoundary.Vertexes.Count > 0)
        {
            foreach (var vertex in wipeout.ClippingBoundary.Vertexes)
            {
                sb.AppendLine($"boundaryVertices.Add(new Vector2({F(vertex.X)}, {F(vertex.Y)}));");
            }
        }

        var entityName = asVariable ? $"wipeout{_entityCounter++}" : "wipeoutEntity";
        sb.AppendLine($"var {entityName} = new Wipeout(boundaryVertices);");

        // Apply entity properties
        if (wipeout.Layer != null && _usedLayers.Contains(wipeout.Layer.Name))
        {
            sb.AppendLine($"{entityName}.Layer = layer{SafeName(wipeout.Layer.Name)};");
        }

        if (asVariable)
        {
            GenerateXData(sb, wipeout, entityName);
            sb.AppendLine($"doc.Entities.Add({entityName});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add({entityName});");
        }
        sb.AppendLine($"}}");
        sb.AppendLine();
    }

    private void GenerateLeader(StringBuilder sb, Leader leader, bool asVariable = false)
    {
        if (asVariable)
        {
            sb.AppendLine($"var entity{leader.Handle} = new Leader(new List<Vector2>");
            sb.AppendLine($"{{");
            if (leader.Vertexes is { Count: > 0 })
            {
                for (var i = 0; i < leader.Vertexes.Count; i++)
                {
                    var vertex = leader.Vertexes[i];
                    var comma = i < leader.Vertexes.Count - 1 ? "," : "";
                    sb.AppendLine($"new Vector2({F(vertex.X)}, {F(vertex.Y)}){comma}");
                }
            }
            // Close argument list and start object initializer
            sb.AppendLine($"}})");
            sb.AppendLine("{");
            GenerateEntityPropertiesCore(sb, leader);
            // Use initializer mode for advanced properties
            GenerateLeaderAdvancedProperties(sb, leader, null!);
            sb.AppendLine("};");
            GenerateXData(sb, leader, $"entity{leader.Handle}");
            sb.AppendLine($"doc.Entities.Add(entity{leader.Handle});");
        }
        else
        {
            sb.AppendLine("doc.Entities.Add(");
            sb.AppendLine("new Leader(new List<Vector2>");
            sb.AppendLine($"{{");
            if (leader.Vertexes is { Count: > 0 })
            {
                for (var i = 0; i < leader.Vertexes.Count; i++)
                {
                    var vertex = leader.Vertexes[i];
                    var comma = i < leader.Vertexes.Count - 1 ? "," : "";
                    sb.AppendLine($"new Vector2({F(vertex.X)}, {F(vertex.Y)}){comma}");
                }
            }
            sb.AppendLine($"}})");
            sb.AppendLine($"{{");
            GenerateEntityPropertiesCore(sb, leader);
            GenerateLeaderAdvancedProperties(sb, leader, null!);
            sb.AppendLine($"}});");
        }
    }

    private void GenerateFace3D(StringBuilder sb, Face3D face3d)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new Face3D(");
        sb.AppendLine($"new Vector3({F(face3d.FirstVertex.X)}, {F(face3d.FirstVertex.Y)}, {F(face3d.FirstVertex.Z)}),");
        sb.AppendLine($"new Vector3({F(face3d.SecondVertex.X)}, {F(face3d.SecondVertex.Y)}, {F(face3d.SecondVertex.Z)}),");
        sb.AppendLine($"new Vector3({F(face3d.ThirdVertex.X)}, {F(face3d.ThirdVertex.Y)}, {F(face3d.ThirdVertex.Z)}),");
        sb.AppendLine($"new Vector3({F(face3d.FourthVertex.X)}, {F(face3d.FourthVertex.Y)}, {F(face3d.FourthVertex.Z)}))");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, face3d);

        // EdgeFlags
        if (face3d.EdgeFlags != Face3DEdgeFlags.None)
        {
            var edgeFlagsStr = GenerateEnumFlags(face3d.EdgeFlags);
            sb.AppendLine($"EdgeFlags = {edgeFlagsStr},");
        }

        sb.AppendLine($"}}");
        sb.AppendLine($");");
    }

private void GenerateLinearDimension(StringBuilder sb, LinearDimension dimension)
{
        var entityVar = $"linearDim{_entityCounter++}";
        sb.AppendLine($"var {entityVar} = new LinearDimension(");
        sb.AppendLine($"new Vector2({F(dimension.FirstReferencePoint.X)}, {F(dimension.FirstReferencePoint.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.SecondReferencePoint.X)}, {F(dimension.SecondReferencePoint.Y)}),");
        sb.AppendLine($"{F(dimension.Offset)}, {F(dimension.Rotation)})");
        sb.AppendLine($"{{");
    GenerateEntityPropertiesCore(sb, dimension);
    GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}};");
    GenerateDimensionStyleOverrides(sb, dimension, entityVar);
        sb.AppendLine($"doc.Entities.Add({entityVar});");
    }

private void GenerateAlignedDimension(StringBuilder sb, AlignedDimension dimension)
{
        var entityVar = $"alignedDim{_entityCounter++}";
        sb.AppendLine($"var {entityVar} = new AlignedDimension(");
        sb.AppendLine($"new Vector2({F(dimension.FirstReferencePoint.X)}, {F(dimension.FirstReferencePoint.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.SecondReferencePoint.X)}, {F(dimension.SecondReferencePoint.Y)}),");
        sb.AppendLine($"{F(dimension.Offset)})");
        sb.AppendLine($"{{");
    GenerateEntityPropertiesCore(sb, dimension);
    GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}};");
    GenerateDimensionStyleOverrides(sb, dimension, entityVar);
        sb.AppendLine($"doc.Entities.Add({entityVar});");
    }

private void GenerateRadialDimension(StringBuilder sb, RadialDimension dimension)
{
        var entityVar = $"radialDim{_entityCounter++}";
        sb.AppendLine($"var {entityVar} = new RadialDimension(");
        sb.AppendLine($"new Vector2({F(dimension.CenterPoint.X)}, {F(dimension.CenterPoint.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.ReferencePoint.X)}, {F(dimension.ReferencePoint.Y)})");
        sb.AppendLine($")");
        sb.AppendLine($"{{");
    GenerateEntityPropertiesCore(sb, dimension);
    GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}};");
    GenerateDimensionStyleOverrides(sb, dimension, entityVar);
        sb.AppendLine($"doc.Entities.Add({entityVar});");
    }

private void GenerateDiametricDimension(StringBuilder sb, DiametricDimension dimension)
{
        var entityVar = $"diametricDim{_entityCounter++}";
        sb.AppendLine($"var {entityVar} = new DiametricDimension(");
        sb.AppendLine($"new Vector2({F(dimension.CenterPoint.X)}, {F(dimension.CenterPoint.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.ReferencePoint.X)}, {F(dimension.ReferencePoint.Y)})");
        sb.AppendLine($")");
        sb.AppendLine($"{{");
    GenerateEntityPropertiesCore(sb, dimension);
    GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}};");
    GenerateDimensionStyleOverrides(sb, dimension, entityVar);
        sb.AppendLine($"doc.Entities.Add({entityVar});");
    }

private void GenerateAngular2LineDimension(StringBuilder sb, Angular2LineDimension dimension)
{
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new Angular2LineDimension(");
        sb.AppendLine($"new Vector2({F(dimension.StartFirstLine.X)}, {F(dimension.StartFirstLine.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.EndFirstLine.X)}, {F(dimension.EndFirstLine.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.StartSecondLine.X)}, {F(dimension.StartSecondLine.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.EndSecondLine.X)}, {F(dimension.EndSecondLine.Y)}),");
        sb.AppendLine($"{F(dimension.Offset)})");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, dimension);
        GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}}");
        sb.AppendLine($");");
    }

    private void GenerateAngular3PointDimension(StringBuilder sb, Angular3PointDimension dimension)
    {
        var entityVar = $"angular3PointDim{_entityCounter++}";
        sb.AppendLine($"var {entityVar} = new Angular3PointDimension(");
        sb.AppendLine($"new Vector2({F(dimension.CenterPoint.X)}, {F(dimension.CenterPoint.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.StartPoint.X)}, {F(dimension.StartPoint.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.EndPoint.X)}, {F(dimension.EndPoint.Y)}),");
        sb.AppendLine($"{F(dimension.Offset)})");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, dimension);
        GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}};");
        GenerateDimensionStyleOverrides(sb, dimension, entityVar);
        sb.AppendLine($"doc.Entities.Add({entityVar});");
    }

    private void GenerateOrdinateDimension(StringBuilder sb, OrdinateDimension dimension)
    {
        var entityVar = $"ordinateDim{_entityCounter++}";
        sb.AppendLine($"var {entityVar} = new OrdinateDimension(");
        sb.AppendLine($"Vector2.Zero,");
        sb.AppendLine($"new Vector2({F(dimension.FeaturePoint.X)}, {F(dimension.FeaturePoint.Y)}),");
        sb.AppendLine($"new Vector2({F(dimension.LeaderEndPoint.X)}, {F(dimension.LeaderEndPoint.Y)}),");
        sb.AppendLine($"OrdinateDimensionAxis.{dimension.Axis},");

        // Add DimensionStyle parameter
        if (dimension.Style != null && dimension.Style.Name != "Standard" && _usedDimensionStyles.Contains(dimension.Style.Name))
        {
            sb.AppendLine($"dimStyle{SafeName(dimension.Style.Name)})");
        }
        else
        {
            sb.AppendLine($"DimensionStyle.Default)");
        }

        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, dimension);
        GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}};");
        GenerateDimensionStyleOverrides(sb, dimension, entityVar);
        sb.AppendLine($"doc.Entities.Add({entityVar});");
    }

    private void GenerateArcLengthDimension(StringBuilder sb, ArcLengthDimension dimension)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new ArcLengthDimension(");
        sb.AppendLine($"new Vector2({F(dimension.CenterPoint.X)}, {F(dimension.CenterPoint.Y)}),");
        sb.AppendLine($"{F(dimension.Radius)},");
        sb.AppendLine($"{F(dimension.StartAngle)},");
        sb.AppendLine($"{F(dimension.EndAngle)},");
        sb.AppendLine($"{F(dimension.Offset)})");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, dimension);
        GenerateDimensionStyleProperties(sb, dimension);
        sb.AppendLine($"}}");
        sb.AppendLine($");");
    }

    private void GenerateRay(StringBuilder sb, Ray ray)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new Ray(");
        sb.AppendLine($"new Vector3({F(ray.Origin.X)}, {F(ray.Origin.Y)}, {F(ray.Origin.Z)}),");
        sb.AppendLine($"new Vector3({F(ray.Direction.X)}, {F(ray.Direction.Y)}, {F(ray.Direction.Z)})");
        sb.AppendLine($")");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, ray);
        sb.AppendLine($"}}");
        sb.AppendLine($");");
    }

    private void GenerateXLine(StringBuilder sb, XLine xline)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new XLine(");
        sb.AppendLine($"new Vector3({F(xline.Origin.X)}, {F(xline.Origin.Y)}, {F(xline.Origin.Z)}),");
        sb.AppendLine($"new Vector3({F(xline.Direction.X)}, {F(xline.Direction.Y)}, {F(xline.Direction.Z)}))");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, xline);
        sb.AppendLine($"}}");
        sb.AppendLine($");");
    }

    private void GenerateSolid(StringBuilder sb, Solid solid)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new Solid(");
        sb.AppendLine($"new Vector2({F(solid.FirstVertex.X)}, {F(solid.FirstVertex.Y)}),");
        sb.AppendLine($"new Vector2({F(solid.SecondVertex.X)}, {F(solid.SecondVertex.Y)}),");
        sb.AppendLine($"new Vector2({F(solid.ThirdVertex.X)}, {F(solid.ThirdVertex.Y)}),");
        sb.AppendLine($"new Vector2({F(solid.FourthVertex.X)}, {F(solid.FourthVertex.Y)})");
        sb.AppendLine($")");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, solid);
        sb.AppendLine($"}}");
        sb.AppendLine($");");
    }

    private void GenerateMLineStyle(StringBuilder sb, MLineStyle style)
    {
        // Generate MLineStyle elements if any
        if (style.Elements.Count > 0)
        {
            var elementsVarName = $"elements{SafeName(style.Name)}";
            sb.AppendLine($"var {elementsVarName} = new List<MLineStyleElement>();");
            foreach (var element in style.Elements)
            {
                sb.AppendLine($"{elementsVarName}.Add(new MLineStyleElement({F(element.Offset)})");
                sb.AppendLine($"{{");
                if (element.Color.Index != 256) // Not ByLayer
                {
                    if (element.Color.Index == 0)
                        sb.AppendLine($"Color = AciColor.ByBlock,");
                    else
                        sb.AppendLine($"Color = new AciColor({element.Color.Index}),");
                }
                if (element.Linetype != null && element.Linetype.Name != "Continuous" && element.Linetype.Name != "ByLayer" && element.Linetype.Name != "ByBlock")
                {
                    sb.AppendLine($"Linetype = linetype{SafeName(element.Linetype.Name)},");
                }
                sb.AppendLine($"}});");
            }
            sb.AppendLine($"var mlineStyle{SafeName(style.Name)} = new MLineStyle(\"{Escape(style.Name)}\", {elementsVarName});");
        }
        else
        {
            sb.AppendLine($"var mlineStyle{SafeName(style.Name)} = new MLineStyle(\"{Escape(style.Name)}\");");
        }

        // Set additional properties
        if (!string.IsNullOrEmpty(style.Description))
        {
            sb.AppendLine($"mlineStyle{SafeName(style.Name)}.Description = \"{Escape(style.Description)}\";");
        }

        if (style.FillColor.Index != 256) // Not ByLayer
        {
            if (style.FillColor.Index == 0)
                sb.AppendLine($"mlineStyle{SafeName(style.Name)}.FillColor = AciColor.ByBlock;");
            else
                sb.AppendLine($"mlineStyle{SafeName(style.Name)}.FillColor = new AciColor({style.FillColor.Index});");
        }

        if (Math.Abs(style.StartAngle - 90.0) > 1e-10)
        {
            sb.AppendLine($"mlineStyle{SafeName(style.Name)}.StartAngle = {F(style.StartAngle)};");
        }

        if (Math.Abs(style.EndAngle - 90.0) > 1e-10)
        {
            sb.AppendLine($"mlineStyle{SafeName(style.Name)}.EndAngle = {F(style.EndAngle)};");
        }

        sb.AppendLine($"doc.MlineStyles.Add(mlineStyle{SafeName(style.Name)});");
    }

    private void GenerateMLine(StringBuilder sb, MLine mline, bool asVariable = false)
    {
        sb.AppendLine($"{{");
        sb.AppendLine($"// MLine entity with vertices");
        sb.AppendLine($"var mlineVertices = new List<Vector2>();");

        if (mline.Vertexes is { Count: > 0 })
        {
            foreach (var vertex in mline.Vertexes)
            {
                sb.AppendLine($"mlineVertices.Add(new Vector2({F(vertex.Position.X)}, {F(vertex.Position.Y)}));");
            }
        }

        var entityName = asVariable ? $"mline{_entityCounter++}" : "mlineEntity";
        sb.AppendLine($"var {entityName} = new MLine(mlineVertices)");
        sb.AppendLine($"{{");
        if (Math.Abs(mline.Scale - 1.0) > 1e-12)
        {
            sb.AppendLine($"Scale = {F(mline.Scale)},");
        }
        if (mline.Justification != MLineJustification.Zero)
        {
            sb.AppendLine($"Justification = MLineJustification.{mline.Justification},");
        }
        if (Math.Abs(mline.Elevation) > 1e-12)
        {
            sb.AppendLine($"Elevation = {F(mline.Elevation)},");
        }
        if (mline.IsClosed)
        {
            sb.AppendLine($"IsClosed = true,");
        }
        if (mline.NoStartCaps)
        {
            sb.AppendLine($"NoStartCaps = true,");
        }
        if (mline.NoEndCaps)
        {
            sb.AppendLine($"NoEndCaps = true,");
        }
        sb.AppendLine($"}};");

        // Apply MLineStyle if not default
        if (mline.Style != null && mline.Style.Name != "Standard" && _usedMLineStyles.Contains(mline.Style.Name))
        {
            sb.AppendLine($"{entityName}.Style = mlineStyle{SafeName(mline.Style.Name)};");
        }

        // Apply entity properties
        if (mline.Layer != null && _usedLayers.Contains(mline.Layer.Name))
        {
            sb.AppendLine($"{entityName}.Layer = layer{SafeName(mline.Layer.Name)};");
        }

        // Color handling
        if (mline.Color.Index != 256) // Not ByLayer
        {
            if (mline.Color.Index == 0)
                sb.AppendLine($"{entityName}.Color = AciColor.ByBlock;");
            else
                sb.AppendLine($"{entityName}.Color = new AciColor({mline.Color.Index});");
        }

        if (asVariable)
        {
            GenerateXData(sb, mline, entityName);
            sb.AppendLine($"entities.Add({entityName});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add({entityName});");
        }
        sb.AppendLine($"}}");
        sb.AppendLine();
    }

    private void GenerateImage(StringBuilder sb, Image image, bool asVariable = false)
    {
        sb.AppendLine($"{{");
        sb.AppendLine($"var imageDefinition = new ImageDefinition(\"{image.Definition.Name}\", {image.Definition.Width}, {F(image.Definition.HorizontalResolution)}, {image.Definition.Height}, {F(image.Definition.VerticalResolution)}, ImageResolutionUnits.{image.Definition.ResolutionUnits});");
        var entityName = asVariable ? $"image{_entityCounter++}" : "imageEntity";
        sb.AppendLine($"var {entityName} = new Image(imageDefinition,");
        sb.AppendLine($"new Vector3({F(image.Position.X)}, {F(image.Position.Y)}, {F(image.Position.Z)}),");
        sb.AppendLine($"{F(image.Width)}, {F(image.Height)});");

        // Apply entity properties
        if (image.Layer != null && _usedLayers.Contains(image.Layer.Name))
        {
            sb.AppendLine($"{entityName}.Layer = layer{SafeName(image.Layer.Name)};");
        }

        // Color handling
        if (image.Color.Index != 256) // Not ByLayer
        {
            if (image.Color.Index == 0)
                sb.AppendLine($"{entityName}.Color = AciColor.ByBlock;");
            else
                sb.AppendLine($"{entityName}.Color = new AciColor({image.Color.Index});");
        }

        // Image-specific properties
        if (image.Rotation != 0)
        {
            sb.AppendLine($"{entityName}.Rotation = {F(image.Rotation)};");
        }
        if (image.Brightness != 50) // Default brightness is 50
        {
            sb.AppendLine($"{entityName}.Brightness = {image.Brightness};");
        }
        if (image.Contrast != 50) // Default contrast is 50
        {
            sb.AppendLine($"{entityName}.Contrast = {image.Contrast};");
        }
        if (image.Fade != 0) // Default fade is 0
        {
            sb.AppendLine($"{entityName}.Fade = {image.Fade};");
        }

        // Clipping state
        if (image.Clipping)
        {
            sb.AppendLine($"{entityName}.Clipping = true;");
        }

        // DisplayOptions (default is ShowImage | ShowImageWhenNotAlignedWithScreen | UseClippingBoundary = 7)
        var defaultDisplayOptions = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowImageWhenNotAlignedWithScreen | ImageDisplayFlags.UseClippingBoundary;
        if (image.DisplayOptions != defaultDisplayOptions)
        {
            var flags = new List<string>();
            if ((image.DisplayOptions & ImageDisplayFlags.ShowImage) != 0)
                flags.Add("ImageDisplayFlags.ShowImage");
            if ((image.DisplayOptions & ImageDisplayFlags.ShowImageWhenNotAlignedWithScreen) != 0)
                flags.Add("ImageDisplayFlags.ShowImageWhenNotAlignedWithScreen");
            if ((image.DisplayOptions & ImageDisplayFlags.UseClippingBoundary) != 0)
                flags.Add("ImageDisplayFlags.UseClippingBoundary");
            if ((image.DisplayOptions & ImageDisplayFlags.TransparencyOn) != 0)
                flags.Add("ImageDisplayFlags.TransparencyOn");

            if (flags.Count > 0)
            {
                sb.AppendLine($"{entityName}.DisplayOptions = {string.Join(" | ", flags)};");
            }
        }

        // ClippingBoundary
        if (image.ClippingBoundary != null && image.ClippingBoundary.Vertexes.Count > 0)
        {
            if (image.ClippingBoundary.Type == ClippingBoundaryType.Rectangular && image.ClippingBoundary.Vertexes.Count == 2)
            {
                // Rectangular boundary with two opposite corners
                var firstCorner = image.ClippingBoundary.Vertexes[0];
                var secondCorner = image.ClippingBoundary.Vertexes[1];
                sb.AppendLine($"{entityName}.ClippingBoundary = new ClippingBoundary(new Vector2({F(firstCorner.X)}, {F(firstCorner.Y)}), new Vector2({F(secondCorner.X)}, {F(secondCorner.Y)}));");
            }
            else
            {
                // Polygonal boundary with 3 or more vertices
                sb.AppendLine($"var clippingVertices = new List<Vector2>");
                sb.AppendLine($"{{");
                for (var i = 0; i < image.ClippingBoundary.Vertexes.Count; i++)
                {
                    var vertex = image.ClippingBoundary.Vertexes[i];
                    var comma = i < image.ClippingBoundary.Vertexes.Count - 1 ? "," : "";
                    sb.AppendLine($"new Vector2({F(vertex.X)}, {F(vertex.Y)}){comma}");
                }
                sb.AppendLine($"}};");
                sb.AppendLine($"{entityName}.ClippingBoundary = new ClippingBoundary(clippingVertices);");
            }
        }

        if (asVariable)
        {
            sb.AppendLine($"entities.Add({entityName});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add({entityName});");
        }
        sb.AppendLine($"}}");
        sb.AppendLine();
    }

    private void GenerateMesh(StringBuilder sb, Mesh mesh, bool asVariable = false)
    {
        sb.AppendLine($"var meshVertexes = new List<Vector3>");
        sb.AppendLine($"{{");
        for (var i = 0; i < mesh.Vertexes.Count; i++)
        {
            var vertex = mesh.Vertexes[i];
            sb.AppendLine($"new Vector3({F(vertex.X)}, {F(vertex.Y)}, {F(vertex.Z)}){(i < mesh.Vertexes.Count - 1 ? "," : "")}");
        }
        sb.AppendLine($"}};");
        sb.AppendLine();

        sb.AppendLine($"var meshFaces = new List<int[]>");
        sb.AppendLine($"{{");
        for (var i = 0; i < mesh.Faces.Count; i++)
        {
            var face = mesh.Faces[i];
            sb.Append($"new int[] {{ ");
            for (var j = 0; j < face.Length; j++)
            {
                sb.Append(face[j]);
                if (j < face.Length - 1)
                    sb.Append(", ");
            }
            sb.AppendLine($" }}{(i < mesh.Faces.Count - 1 ? "," : "")}");
        }
        sb.AppendLine($"}};");
        sb.AppendLine();

        // Generate mesh edges if they exist
        var entityName = asVariable ? $"mesh{_entityCounter++}" : "mesh";
        if (mesh.Edges is { Count: > 0 })
        {
            sb.AppendLine($"var meshEdges = new List<MeshEdge>");
            sb.AppendLine($"{{");
            for (var i = 0; i < mesh.Edges.Count; i++)
            {
                var edge = mesh.Edges[i];
                sb.AppendLine($"new MeshEdge({edge.StartVertexIndex}, {edge.EndVertexIndex}){(i < mesh.Edges.Count - 1 ? "," : "")}");
            }
            sb.AppendLine($"}};");
            sb.AppendLine();

            sb.AppendLine($"var {entityName} = new Mesh(meshVertexes, meshFaces, meshEdges)");
        }
        else
        {
            sb.AppendLine($"var {entityName} = new Mesh(meshVertexes, meshFaces)");
        }

        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, mesh);
        sb.AppendLine($"SubdivisionLevel = {mesh.SubdivisionLevel}");
        sb.AppendLine($"}};");

        if (asVariable)
        {
            GenerateXData(sb, mesh, entityName);
            sb.AppendLine($"entities.Add({entityName});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add({entityName});");
        }
    }

    private void GeneratePolyfaceMesh(StringBuilder sb, PolyfaceMesh polyfaceMesh, bool asVariable = false)
    {
        sb.AppendLine($"var polyfaceMeshVertexes = new Vector3[]");
        sb.AppendLine($"{{");
        for (var i = 0; i < polyfaceMesh.Vertexes.Length; i++)
        {
            var vertex = polyfaceMesh.Vertexes[i];
            var comma = i < polyfaceMesh.Vertexes.Length - 1 ? "," : "";
            sb.AppendLine($"new Vector3({F(vertex.X)}, {F(vertex.Y)}, {F(vertex.Z)}){comma}");
        }
        sb.AppendLine($"}};");
        sb.AppendLine();

        sb.AppendLine($"var polyfaceMeshFaces = new PolyfaceMeshFace[]");
        sb.AppendLine($"{{");
        for (var i = 0; i < polyfaceMesh.Faces.Count; i++)
        {
            var face = polyfaceMesh.Faces[i];
            var comma = i < polyfaceMesh.Faces.Count - 1 ? "," : "";
            var vertexIndexes = string.Join(", ", face.VertexIndexes);
            sb.AppendLine($"new PolyfaceMeshFace(new short[] {{ {vertexIndexes} }}){comma}");
        }
        sb.AppendLine($"}};");
        sb.AppendLine();

        var entityName = asVariable ? $"polyfaceMesh{_entityCounter++}" : "polyfaceMesh";
        sb.AppendLine($"var {entityName} = new PolyfaceMesh(polyfaceMeshVertexes, polyfaceMeshFaces)");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, polyfaceMesh);
        sb.AppendLine($"}};");

        if (asVariable)
        {
            GenerateXData(sb, polyfaceMesh, entityName);
            sb.AppendLine($"entities.Add({entityName});");
        }
        else
        {
            sb.AppendLine($"doc.Entities.Add({entityName});");
        }
        sb.AppendLine();
    }

    private void GeneratePolygonMesh(StringBuilder sb, PolygonMesh polygonMesh)
    {
        sb.AppendLine($"// Generate PolygonMesh vertexes");
        sb.AppendLine($"var polygonMeshVertexes = new Vector3[]");
        sb.AppendLine($"{{");
        for (var i = 0; i < polygonMesh.Vertexes.Length; i++)
        {
            var vertex = polygonMesh.Vertexes[i];
            var comma = i < polygonMesh.Vertexes.Length - 1 ? "," : "";
            sb.AppendLine($"new Vector3({F(vertex.X)}, {F(vertex.Y)}, {F(vertex.Z)}){comma}");
        }
        sb.AppendLine($"}};");
        sb.AppendLine();

        sb.AppendLine($"var polygonMesh = new PolygonMesh({polygonMesh.U}, {polygonMesh.V}, polygonMeshVertexes)");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, polygonMesh);
        // Ensure DensityU and DensityV are within valid range (3-201)
        var densityU = (short)Math.Max(3, Math.Min(201, (int)polygonMesh.DensityU));
        var densityV = (short)Math.Max(3, Math.Min(201, (int)polygonMesh.DensityV));
        sb.AppendLine($"DensityU = {densityU},");
        sb.AppendLine($"DensityV = {densityV},");
        sb.AppendLine($"SmoothType = PolylineSmoothType.{polygonMesh.SmoothType},");
        sb.AppendLine($"IsClosedInU = {polygonMesh.IsClosedInU.ToString().ToLower()},");
        sb.AppendLine($"IsClosedInV = {polygonMesh.IsClosedInV.ToString().ToLower()}");
        sb.AppendLine($"}};");
        sb.AppendLine($"doc.Entities.Add(polygonMesh);");
        sb.AppendLine();
    }

    private void GenerateShape(StringBuilder sb, Shape shape)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine($"new Shape(\"{shape.Name}\",");
        sb.AppendLine($"new ShapeStyle(\"{shape.Style.Name}\", \"{shape.Style.File}\"),");
        sb.AppendLine($"new Vector3({F(shape.Position.X)}, {F(shape.Position.Y)}, {F(shape.Position.Z)}),");
        sb.AppendLine($"{F(shape.Size)}, {F(shape.Rotation)})");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, shape);
        sb.AppendLine($"}});");
    }

private void GenerateAttribute(StringBuilder sb, Attribute attribute, DxfCodeGenerationOptions options, bool asVariable = false)
{
        if (asVariable)
        {
            sb.AppendLine($"var attribute{_insertCounter++} = new Attribute(");
        }
        else
        {
            sb.AppendLine("doc.Entities.Add(");
            sb.AppendLine("new Attribute(");
        }

        // Create AttributeDefinition if needed
        if (attribute.Definition != null && options.GenerateAttributeDefinitionEntities)
        {
            var styleName = attribute.Style?.Name ?? "Standard";
            if (styleName == "Standard")
            {
                sb.AppendLine($"new AttributeDefinition(\"{Escape(attribute.Tag)}\", {F(attribute.Height)}, TextStyle.Default)");
            }
            else
            {
                sb.AppendLine($"new AttributeDefinition(\"{Escape(attribute.Tag)}\", {F(attribute.Height)}, textStyle{SafeName(styleName)})");
            }
        }
        else
        {
            sb.AppendLine($"\"{Escape(attribute.Tag)}\"");
        }

        sb.AppendLine($")");
        sb.AppendLine($"{{");

        // Set attribute-specific properties
        if (!string.IsNullOrEmpty(attribute.Value))
        {
            sb.AppendLine($"Value = \"{Escape(attribute.Value)}\",");
        }

        if (attribute.Position != Vector3.Zero)
        {
            sb.AppendLine($"Position = new Vector3({F(attribute.Position.X)}, {F(attribute.Position.Y)}, {F(attribute.Position.Z)}),");
        }

        if (Math.Abs(attribute.Height - 1.0) > 1e-10)
        {
            sb.AppendLine($"Height = {F(attribute.Height)},");
        }

        if (Math.Abs(attribute.WidthFactor - 1.0) > 1e-10)
        {
            sb.AppendLine($"WidthFactor = {F(attribute.WidthFactor)},");
        }

        if (Math.Abs(attribute.Rotation) > 1e-10)
        {
            sb.AppendLine($"Rotation = {F(attribute.Rotation)},");
        }

        if (attribute.Alignment != TextAlignment.BaselineLeft)
        {
            sb.AppendLine($"Alignment = TextAlignment.{attribute.Alignment},");
        }

        if (attribute.Style != null && attribute.Style.Name != "Standard" && _usedTextStyles.Contains(attribute.Style.Name))
        {
            sb.AppendLine($"Style = textStyle{SafeName(attribute.Style.Name)},");
        }

        if (attribute.IsBackward)
        {
            sb.AppendLine($"IsBackward = true,");
        }

        if (attribute.IsUpsideDown)
        {
            sb.AppendLine($"IsUpsideDown = true,");
        }

        if (Math.Abs(attribute.ObliqueAngle) > 1e-10)
        {
            sb.AppendLine($"ObliqueAngle = {F(attribute.ObliqueAngle)},");
        }

        if (attribute.Flags != AttributeFlags.None)
        {
            sb.AppendLine($"Flags = AttributeFlags.{attribute.Flags},");
        }

        // Generate common attribute properties
        GenerateAttributeProperties(sb, attribute);

        if (asVariable)
        {
            sb.AppendLine($"}});");
        }
        else
        {
            sb.AppendLine($"}});");
        }
    }

    private void GenerateTolerance(StringBuilder sb, Tolerance tolerance)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new Tolerance(");
        sb.AppendLine($"new ToleranceEntry(),");
        sb.AppendLine($"new Vector3({F(tolerance.Position.X)}, {F(tolerance.Position.Y)}, {F(tolerance.Position.Z)})");
        sb.AppendLine($")");
        sb.AppendLine($"{{");
        sb.AppendLine($"TextHeight = {F(tolerance.TextHeight)},");
        sb.AppendLine($"Rotation = {F(tolerance.Rotation)},");
        GenerateEntityPropertiesCore(sb, tolerance);
        sb.AppendLine($"}});");
    }

    private void GenerateTrace(StringBuilder sb, Trace trace)
    {
        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new Trace(");
        sb.AppendLine($"new Vector2({F(trace.FirstVertex.X)}, {F(trace.FirstVertex.Y)}),");
        sb.AppendLine($"new Vector2({F(trace.SecondVertex.X)}, {F(trace.SecondVertex.Y)}),");
        sb.AppendLine($"new Vector2({F(trace.ThirdVertex.X)}, {F(trace.ThirdVertex.Y)}),");
        sb.AppendLine($"new Vector2({F(trace.FourthVertex.X)}, {F(trace.FourthVertex.Y)})");
        sb.AppendLine($")");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, trace);
        sb.AppendLine($"}});");
    }

    private void GenerateUnderlay(StringBuilder sb, Underlay underlay)
    {
        sb.AppendLine($"// Note: Underlay requires UnderlayDefinition to be added to document first");
        sb.AppendLine($"// Create underlay definition for {underlay.Definition.Type} type");
        // Generate the correct underlay definition class name
        var definitionClassName = underlay.Definition.Type switch
        {
            UnderlayType.DGN => "UnderlayDgnDefinition",
            UnderlayType.DWF => "UnderlayDwfDefinition",
            UnderlayType.PDF => "UnderlayPdfDefinition",
            _ => $"Underlay{underlay.Definition.Type}Definition"
        };
        sb.AppendLine($"var underlayDef = new {definitionClassName}(\"{Escape(underlay.Definition.Name)}\", \"{Escape(underlay.Definition.File)}\");");
        sb.AppendLine();

        sb.AppendLine("doc.Entities.Add(");
        sb.AppendLine("new Underlay(underlayDef)");
        sb.AppendLine($"{{");
        sb.AppendLine($"Position = new Vector3({F(underlay.Position.X)}, {F(underlay.Position.Y)}, {F(underlay.Position.Z)}),");
        sb.AppendLine($"Scale = new Vector2({F(underlay.Scale.X)}, {F(underlay.Scale.Y)}),");
        sb.AppendLine($"Rotation = {F(underlay.Rotation)},");
        sb.AppendLine($"Contrast = {underlay.Contrast},");
        sb.AppendLine($"Fade = {underlay.Fade},");
        sb.AppendLine($"DisplayOptions = UnderlayDisplayFlags.{underlay.DisplayOptions},");
        if (underlay.ClippingBoundary != null && underlay.ClippingBoundary.Vertexes.Count > 0)
        {
            if (underlay.ClippingBoundary.Type == ClippingBoundaryType.Rectangular && underlay.ClippingBoundary.Vertexes.Count == 2)
            {
                var firstCorner = underlay.ClippingBoundary.Vertexes[0];
                var secondCorner = underlay.ClippingBoundary.Vertexes[1];
                sb.AppendLine($"ClippingBoundary = new ClippingBoundary(new Vector2({F(firstCorner.X)}, {F(firstCorner.Y)}), new Vector2({F(secondCorner.X)}, {F(secondCorner.Y)})),");
            }
            else
            {
                sb.AppendLine($"ClippingBoundary = new ClippingBoundary(new List<Vector2>");
                sb.AppendLine($"{{");
                for (var i = 0; i < underlay.ClippingBoundary.Vertexes.Count; i++)
                {
                    var v = underlay.ClippingBoundary.Vertexes[i];
                    var comma = i < underlay.ClippingBoundary.Vertexes.Count - 1 ? "," : "";
                    sb.AppendLine($"new Vector2({F(v.X)}, {F(v.Y)}){comma}");
                }
                sb.AppendLine($"}}),");
            }
        }
        GenerateEntityPropertiesCore(sb, underlay);
        sb.AppendLine($"}});");
        sb.AppendLine();
    }

    private void GenerateGroup(StringBuilder sb, Group group, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Group: {group.Name} ({group.Handle})");
        }

        sb.AppendLine($"var group{group.Handle} = new Group(\"{Escape(group.Name)}\");");

        // Set description if not empty
        if (!string.IsNullOrEmpty(group.Description))
        {
            sb.AppendLine($"group{group.Handle}.Description = \"{Escape(group.Description)}\";");
        }

        // Set IsSelectable if not default (true)
        if (!group.IsSelectable)
        {
            sb.AppendLine($"group{group.Handle}.IsSelectable = false;");
        }

        sb.AppendLine($"doc.Groups.Add(group{group.Handle});");

        // Add entities to group if any
        if (group.Entities.Count > 0)
        {
            foreach (var entity in group.Entities)
            {
                sb.AppendLine($"group{group.Handle}.Entities.Add(entity{entity.Handle});");
            }
        }
        sb.AppendLine();
    }

private void GenerateLayout(StringBuilder sb, Layout layout, DxfCodeGenerationOptions options)
{
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Layout: {layout.Name} ({layout.Handle})");
        }

        sb.AppendLine($"var layout{layout.Handle} = new Layout(\"{Escape(layout.Name)}\");");

        // Set TabOrder if not default (0)
        if (layout.TabOrder != 0)
        {
            sb.AppendLine($"layout{layout.Handle}.TabOrder = {layout.TabOrder};");
        }

        // Set MinLimit if not default
        if (layout.MinLimit.X != 0 || layout.MinLimit.Y != 0)
        {
            sb.AppendLine($"layout{layout.Handle}.MinLimit = new Vector2({F(layout.MinLimit.X)}, {F(layout.MinLimit.Y)});");
        }

        // Set MaxLimit if not default
        if (layout.MaxLimit.X != 0 || layout.MaxLimit.Y != 0)
        {
            sb.AppendLine($"layout{layout.Handle}.MaxLimit = new Vector2({F(layout.MaxLimit.X)}, {F(layout.MaxLimit.Y)});");
        }

        // Set MinExtents if not default
        if (layout.MinExtents.X != 0 || layout.MinExtents.Y != 0 || layout.MinExtents.Z != 0)
        {
            sb.AppendLine($"layout{layout.Handle}.MinExtents = new Vector3({F(layout.MinExtents.X)}, {F(layout.MinExtents.Y)}, {F(layout.MinExtents.Z)});");
        }

        // Set MaxExtents if not default
        if (layout.MaxExtents.X != 0 || layout.MaxExtents.Y != 0 || layout.MaxExtents.Z != 0)
        {
            sb.AppendLine($"layout{layout.Handle}.MaxExtents = new Vector3({F(layout.MaxExtents.X)}, {F(layout.MaxExtents.Y)}, {F(layout.MaxExtents.Z)});");
        }

        // Set BasePoint if not default
        if (layout.BasePoint.X != 0 || layout.BasePoint.Y != 0 || layout.BasePoint.Z != 0)
        {
            sb.AppendLine($"layout{layout.Handle}.BasePoint = new Vector3({F(layout.BasePoint.X)}, {F(layout.BasePoint.Y)}, {F(layout.BasePoint.Z)});");
        }

        // Set Elevation if not default (0)
        if (Math.Abs(layout.Elevation) > 1e-12)
        {
            sb.AppendLine($"layout{layout.Handle}.Elevation = {F(layout.Elevation)};");
        }

        // Set UcsOrigin if not default
        if (layout.UcsOrigin.X != 0 || layout.UcsOrigin.Y != 0 || layout.UcsOrigin.Z != 0)
        {
            sb.AppendLine($"layout{layout.Handle}.UcsOrigin = new Vector3({F(layout.UcsOrigin.X)}, {F(layout.UcsOrigin.Y)}, {F(layout.UcsOrigin.Z)});");
        }

        // Set UcsXAxis if not default (1,0,0)
        if (Math.Abs(layout.UcsXAxis.X - 1.0) > 1e-12 || Math.Abs(layout.UcsXAxis.Y) > 1e-12 || Math.Abs(layout.UcsXAxis.Z) > 1e-12)
        {
            sb.AppendLine($"layout{layout.Handle}.UcsXAxis = new Vector3({F(layout.UcsXAxis.X)}, {F(layout.UcsXAxis.Y)}, {F(layout.UcsXAxis.Z)});");
        }

        // Set UcsYAxis if not default (0,1,0)
        if (Math.Abs(layout.UcsYAxis.X) > 1e-12 || Math.Abs(layout.UcsYAxis.Y - 1.0) > 1e-12 || Math.Abs(layout.UcsYAxis.Z) > 1e-12)
        {
            sb.AppendLine($"layout{layout.Handle}.UcsYAxis = new Vector3({F(layout.UcsYAxis.X)}, {F(layout.UcsYAxis.Y)}, {F(layout.UcsYAxis.Z)});");
        }

        // Note: IsPaperSpace is read-only and determined by the layout type, so we don't generate assignment code for it

        sb.AppendLine($"doc.Layouts.Add(layout{layout.Handle});");
        sb.AppendLine();
    }

    private void GenerateImageDefinition(StringBuilder sb, ImageDefinition imageDef, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Image Definition: {imageDef.Name} ({imageDef.Handle})");
        }

        sb.AppendLine($"var imageDef{imageDef.Handle} = new ImageDefinition(\"{Escape(imageDef.Name)}\", \"{Escape(imageDef.File)}\", {imageDef.Width}, {F(imageDef.HorizontalResolution)}, {imageDef.Height}, {F(imageDef.VerticalResolution)}, ImageResolutionUnits.{imageDef.ResolutionUnits});");
        sb.AppendLine($"doc.ImageDefinitions.Add(imageDef{imageDef.Handle});");
        sb.AppendLine();
    }

    private void GenerateUnderlayDefinition(StringBuilder sb, UnderlayDefinition underlayDef, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Underlay Definition: {underlayDef.Name} ({underlayDef.Handle})");
        }

        var typeName = underlayDef.GetType().Name;
        sb.AppendLine($"var underlayDef{underlayDef.Handle} = new {typeName}(\"{Escape(underlayDef.Name)}\", \"{Escape(underlayDef.File)}\");");

        // Add to appropriate collection based on type
        if (underlayDef is UnderlayDgnDefinition)
        {
            sb.AppendLine($"doc.UnderlayDgnDefinitions.Add((UnderlayDgnDefinition)underlayDef{underlayDef.Handle});");
        }
        else if (underlayDef is UnderlayDwfDefinition)
        {
            sb.AppendLine($"doc.UnderlayDwfDefinitions.Add((UnderlayDwfDefinition)underlayDef{underlayDef.Handle});");
        }
        else if (underlayDef is UnderlayPdfDefinition)
        {
            sb.AppendLine($"doc.UnderlayPdfDefinitions.Add((UnderlayPdfDefinition)underlayDef{underlayDef.Handle});");
        }
        sb.AppendLine();
    }

    private void GenerateRasterVariables(StringBuilder sb, RasterVariables rasterVars, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Raster Variables ({rasterVars.Handle})");
        }

        // Check if any properties differ from defaults
        var hasNonDefaultValues = !rasterVars.DisplayFrame ||
                                  rasterVars.DisplayQuality != ImageDisplayQuality.High ||
                                  rasterVars.Units != ImageUnits.Unitless;

        if (hasNonDefaultValues)
        {
            // Only set properties if they differ from defaults
            if (!rasterVars.DisplayFrame)
            {
                sb.AppendLine($"doc.RasterVariables.DisplayFrame = false;");
            }

            if (rasterVars.DisplayQuality != ImageDisplayQuality.High)
            {
                sb.AppendLine($"doc.RasterVariables.DisplayQuality = ImageDisplayQuality.{rasterVars.DisplayQuality};");
            }

            if (rasterVars.Units != ImageUnits.Unitless)
            {
                sb.AppendLine($"doc.RasterVariables.Units = ImageUnits.{rasterVars.Units};");
            }
        }
        else if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// RasterVariables uses default values");
        }
    }

    private void GenerateLayerState(StringBuilder sb, LayerState layerState, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Layer State: {layerState.Name}");
        }

        sb.AppendLine($"var layerState = new LayerState(\"{Escape(layerState.Name)}\");");

        // Set description if not empty
        if (!string.IsNullOrEmpty(layerState.Description))
        {
            sb.AppendLine($"layerState.Description = \"{Escape(layerState.Description)}\";");
        }

        // Set current layer if specified
        if (!string.IsNullOrEmpty(layerState.CurrentLayer))
        {
            sb.AppendLine($"layerState.CurrentLayer = \"{Escape(layerState.CurrentLayer)}\";");
        }

        // Set paper space flag if true
        if (layerState.PaperSpace)
        {
            sb.AppendLine($"layerState.PaperSpace = true;");
        }

        // Add layer state properties if any
        if (layerState.Properties.Count > 0)
        {
            if (options.GenerateDetailedComments)
            {
                sb.AppendLine($"// Layer state properties");
            }
            foreach (var kvp in layerState.Properties)
            {
                var layerName = kvp.Key;
                var props = kvp.Value;
                sb.AppendLine($"layerState.Properties.Add(\"{Escape(layerName)}\", new LayerStateProperties(\"{Escape(layerName)}\")");
                sb.AppendLine($"{{");
                sb.AppendLine($"Color = AciColor.FromCadIndex({props.Color.Index}),");
                sb.AppendLine($"Lineweight = Lineweight.{props.Lineweight},");
                sb.AppendLine($"LinetypeName = \"{Escape(props.LinetypeName)}\",");
                sb.AppendLine($"Flags = LayerPropertiesFlags.{props.Flags},");
                sb.AppendLine($"Transparency = new Transparency({props.Transparency.Value})");
                sb.AppendLine($"}});");
            }
        }

        sb.AppendLine();
    }

    private void GeneratePlotSettings(StringBuilder sb, PlotSettings plotSettings, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Plot Settings");
        }

        sb.AppendLine($"var plotSettings = new PlotSettings();");

        // Set page setup name if not empty
        if (!string.IsNullOrEmpty(plotSettings.PageSetupName))
        {
            sb.AppendLine($"plotSettings.PageSetupName = \"{Escape(plotSettings.PageSetupName)}\";");
        }

        // Set plotter name if not empty
        if (!string.IsNullOrEmpty(plotSettings.PlotterName))
        {
            sb.AppendLine($"plotSettings.PlotterName = \"{Escape(plotSettings.PlotterName)}\";");
        }

        // Set paper size name if not empty
        if (!string.IsNullOrEmpty(plotSettings.PaperSizeName))
        {
            sb.AppendLine($"plotSettings.PaperSizeName = \"{Escape(plotSettings.PaperSizeName)}\";");
        }

        // Set view name if not empty
        if (!string.IsNullOrEmpty(plotSettings.ViewName))
        {
            sb.AppendLine($"plotSettings.ViewName = \"{Escape(plotSettings.ViewName)}\";");
        }

        // Set current style sheet if not empty
        if (!string.IsNullOrEmpty(plotSettings.CurrentStyleSheet))
        {
            sb.AppendLine($"plotSettings.CurrentStyleSheet = \"{Escape(plotSettings.CurrentStyleSheet)}\";");
        }

        // Set paper margin if not default
        if (plotSettings.PaperMargin.Left != 0 || plotSettings.PaperMargin.Bottom != 0 || plotSettings.PaperMargin.Right != 0 || plotSettings.PaperMargin.Top != 0)
        {
            sb.AppendLine($"plotSettings.PaperMargin = new PaperMargin({F(plotSettings.PaperMargin.Left)}, {F(plotSettings.PaperMargin.Bottom)}, {F(plotSettings.PaperMargin.Right)}, {F(plotSettings.PaperMargin.Top)});");
        }

        // Set paper size if not default
        if (plotSettings.PaperSize.X != 0 || plotSettings.PaperSize.Y != 0)
        {
            sb.AppendLine($"plotSettings.PaperSize = new Vector2({F(plotSettings.PaperSize.X)}, {F(plotSettings.PaperSize.Y)});");
        }

        // Set origin if not default
        if (plotSettings.Origin.X != 0 || plotSettings.Origin.Y != 0)
        {
            sb.AppendLine($"plotSettings.Origin = new Vector2({F(plotSettings.Origin.X)}, {F(plotSettings.Origin.Y)});");
        }

        // Set scale to fit if true
        if (plotSettings.ScaleToFit)
        {
            sb.AppendLine($"plotSettings.ScaleToFit = true;");
        }

        // Set scale if not 1:1
        if (plotSettings.PrintScaleNumerator != 1.0 || plotSettings.PrintScaleDenominator != 1.0)
        {
            sb.AppendLine($"plotSettings.PrintScaleNumerator = {F(plotSettings.PrintScaleNumerator)};");
            sb.AppendLine($"plotSettings.PrintScaleDenominator = {F(plotSettings.PrintScaleDenominator)};");
        }

        // Set plot type if not default
        if (plotSettings.PlotType != PlotType.DrawingExtents)
        {
            sb.AppendLine($"plotSettings.PlotType = PlotType.{plotSettings.PlotType};");
        }

        // Set paper units if not default
        if (plotSettings.PaperUnits != PlotPaperUnits.Milimeters)
        {
            sb.AppendLine($"plotSettings.PaperUnits = PlotPaperUnits.{plotSettings.PaperUnits};");
        }

        // Set rotation if not default
        if (plotSettings.PaperRotation != PlotRotation.NoRotation)
        {
            sb.AppendLine($"plotSettings.PaperRotation = PlotRotation.{plotSettings.PaperRotation};");
        }

        sb.AppendLine();
    }

    private void GenerateDictionaryObjectPlaceholder(StringBuilder sb, DxfCodeGenerationOptions options)
    {
        // DictionaryObject is an internal class in netDxf and is not exposed in the public API
        // It is used internally for managing dictionary entries in DXF files
        // Dictionary objects are typically created and managed automatically by the netDxf library
        // when working with named object dictionaries and are not intended for direct user generation

        // Always generate the basic comment
        sb.AppendLine($"// Dictionary objects are internal to netDxf and not directly accessible");

        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Dictionary objects store key-value pairs for named objects");
            sb.AppendLine($"// They are used internally for organizing objects like layer states");
            sb.AppendLine($"// Example structure:");
            sb.AppendLine($"//   Handle: [object handle]");
            sb.AppendLine($"//   Entries: Dictionary<string, string> of handle/name pairs");
            sb.AppendLine($"//   IsHardOwner: [boolean indicating ownership type]");
            sb.AppendLine($"//   Cloning: [DictionaryCloningFlags]");
            sb.AppendLine($"// Note: DictionaryObject is internal to netDxf and not available for direct generation");
        }
    }

    private void GenerateLayerStatePlaceholder(StringBuilder sb, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// LayerState objects are internal to netDxf and not directly accessible");
            sb.AppendLine($"// LayerState objects store layer property snapshots");
            sb.AppendLine($"//   Name: [layer state name]");
            sb.AppendLine($"//   Description: [optional description]");
            sb.AppendLine($"//   LayerProperties: Dictionary of layer names and their saved properties");
            sb.AppendLine($"// Create a LayerState to save and restore layer properties");
        }

        sb.AppendLine($"var layerState = new LayerState(\"MyLayerState\")");
        sb.AppendLine($"{{");
        sb.AppendLine($"Description = \"Sample layer state description\",");
        sb.AppendLine($"CurrentLayer = \"0\",");
        sb.AppendLine($"PaperSpace = false");
        sb.AppendLine($"}};\n");

        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Add layer properties to the layer state");
        }

        sb.AppendLine($"var layerProperties = new LayerStateProperties(\"0\")");
        sb.AppendLine($"{{");
        sb.AppendLine($"Flags = LayerPropertiesFlags.Plot,");
        sb.AppendLine($"LinetypeName = \"Continuous\",");
        sb.AppendLine($"Color = AciColor.Default,");
        sb.AppendLine($"Lineweight = Lineweight.Default,");
        sb.AppendLine($"Transparency = new Transparency(0)");
        sb.AppendLine($"}};\n");

        sb.AppendLine($"layerState.Properties.Add(layerProperties.Name, layerProperties);\n");

        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Add the layer state to the document's layer state manager");
        }

        sb.AppendLine($"doc.Layers.StateManager.Add(layerState);");
    }

    private void GeneratePlotSettingsPlaceholder(StringBuilder sb, DxfCodeGenerationOptions options)
    {
        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// PlotSettings objects are internal to netDxf and not directly accessible");
            sb.AppendLine($"// PlotSettings objects store plot configuration data");
            sb.AppendLine($"//   PlotConfigurationName: [plotter configuration name]");
            sb.AppendLine($"//   PaperSize: [paper size name]");
            sb.AppendLine($"//   PlotArea: [plot area type]");
            sb.AppendLine($"// Create plot settings for layout configuration");
        }

        sb.AppendLine($"var plotSettings = new PlotSettings()");
        sb.AppendLine($"{{");
        sb.AppendLine($"PageSetupName = \"MyPageSetup\",");
        sb.AppendLine($"PlotterName = \"DWG To PDF.pc3\",");
        sb.AppendLine($"PaperSizeName = \"ISO_A4_(210.00_x_297.00_MM)\",");
        sb.AppendLine($"ViewName = string.Empty,");
        sb.AppendLine($"CurrentStyleSheet = \"monochrome.ctb\",");
        sb.AppendLine($"PaperMargin = new PaperMargin(7.5, 20.0, 7.5, 20.0),");
        sb.AppendLine($"PaperSize = new Vector2(210.0, 297.0),");
        sb.AppendLine($"Origin = Vector2.Zero,");
        sb.AppendLine($"WindowUpRight = Vector2.Zero,");
        sb.AppendLine($"WindowBottomLeft = Vector2.Zero,");
        sb.AppendLine($"ScaleToFit = true,");
        sb.AppendLine($"PrintScaleNumerator = 1.0,");
        sb.AppendLine($"PrintScaleDenominator = 1.0,");
        sb.AppendLine($"Flags = PlotFlags.DrawViewportsFirst | PlotFlags.PrintLineweights | PlotFlags.PlotPlotStyles | PlotFlags.UseStandardScale,");
        sb.AppendLine($"PlotType = PlotType.DrawingExtents,");
        sb.AppendLine($"PaperUnits = PlotPaperUnits.Milimeters,");
        sb.AppendLine($"PaperRotation = PlotRotation.Degrees90,");
        sb.AppendLine($"ShadePlotMode = ShadePlotMode.AsDisplayed,");
        sb.AppendLine($"ShadePlotResolutionMode = ShadePlotResolutionMode.Normal,");
        sb.AppendLine($"ShadePlotDPI = 300,");
        sb.AppendLine($"PaperImageOrigin = Vector2.Zero");
        sb.AppendLine($"}};\n");

        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// Note: PlotSettings is typically used with Layout objects");
            sb.AppendLine($"// Example: layout.PlotSettings = plotSettings;");
        }
    }

    private void GenerateXRecordPlaceholder(StringBuilder sb, DxfCodeGenerationOptions options)
    {
        // XRecord objects are internal to netDxf and not exposed in the public API
        // They are used internally for storing arbitrary data and are not meant for direct user generation

        // Always generate basic comment to satisfy tests
        sb.AppendLine($"// XRecord objects are internal to netDxf and not directly accessible");

        if (options.GenerateDetailedComments)
        {
            sb.AppendLine($"// XRecord objects contain arbitrary data as key-value pairs");
            sb.AppendLine($"// Note: XRecord objects are internal to netDxf and cannot be directly created by users");
            sb.AppendLine($"// They are used internally for storing arbitrary data in DXF files");
        }
    }

    private void GenerateViewPlaceholder(StringBuilder sb, string viewName)
    {
        sb.AppendLine($"// View: {Escape(viewName)} (Views collection is internal in netDxf)");
        sb.AppendLine($"var view{SafeName(viewName)} = new View(\"{Escape(viewName)}\");");
        sb.AppendLine($"view{SafeName(viewName)}.Target = new Vector3(0, 0, 0);");
        sb.AppendLine($"view{SafeName(viewName)}.Camera = new Vector3(0, 0, 1);");
        sb.AppendLine($"view{SafeName(viewName)}.Height = 10.0;");
        sb.AppendLine($"view{SafeName(viewName)}.Width = 10.0;");
        sb.AppendLine($"view{SafeName(viewName)}.Rotation = 0.0;");
        sb.AppendLine($"view{SafeName(viewName)}.ViewMode = ViewModeFlags.Off;");
        sb.AppendLine($"view{SafeName(viewName)}.FieldOfView = 40.0;");
        sb.AppendLine($"view{SafeName(viewName)}.FrontClippingPlane = 0.0;");
        sb.AppendLine($"view{SafeName(viewName)}.BackClippingPlane = 0.0;");
        sb.AppendLine($"// Note: Cannot add to doc.Views as it is internal - doc.Views.Add(view{SafeName(viewName)});");
    }

    private void GenerateViewport(StringBuilder sb, Viewport viewport)
    {
        var statusFlags = GenerateViewportStatusFlags(viewport.Status);
        if (statusFlags.Length > 80) // If the line is too long, use a variable
        {
            sb.AppendLine($"var viewportStatus = {statusFlags};");
        }

        var vpVar = $"viewport{_entityCounter++}";
        sb.AppendLine($"var {vpVar} = new Viewport(");
        sb.AppendLine($"new Vector2({F(viewport.Center.X)}, {F(viewport.Center.Y)}),");
        sb.AppendLine($"{F(viewport.Width)}, {F(viewport.Height)})");
        sb.AppendLine($"{{");
        GenerateEntityPropertiesCore(sb, viewport);
        // Set Center property if Z coordinate is not zero
        if (Math.Abs(viewport.Center.Z) > 1e-6)
        {
            sb.AppendLine($"Center = new Vector3({F(viewport.Center.X)}, {F(viewport.Center.Y)}, {F(viewport.Center.Z)}),");
        }
        sb.AppendLine($"ViewCenter = new Vector2({F(viewport.ViewCenter.X)}, {F(viewport.ViewCenter.Y)}),");
        sb.AppendLine($"ViewHeight = {F(viewport.ViewHeight)},");
        sb.AppendLine($"ViewTarget = new Vector3({F(viewport.ViewTarget.X)}, {F(viewport.ViewTarget.Y)}, {F(viewport.ViewTarget.Z)}),");
        sb.AppendLine($"ViewDirection = new Vector3({F(viewport.ViewDirection.X)}, {F(viewport.ViewDirection.Y)}, {F(viewport.ViewDirection.Z)}),");
        sb.AppendLine($"LensLength = {F(viewport.LensLength)},");
        sb.AppendLine($"TwistAngle = {F(viewport.TwistAngle)},");
        sb.AppendLine($"CircleZoomPercent = {viewport.CircleZoomPercent},");
        if (statusFlags.Length > 80)
        {
            sb.AppendLine($"Status = viewportStatus,");
        }
        else
        {
            sb.AppendLine($"Status = {statusFlags},");
        }
        // Additional properties
        if (Math.Abs(viewport.SnapBase.X) > 1e-12 || Math.Abs(viewport.SnapBase.Y) > 1e-12)
        {
            sb.AppendLine($"SnapBase = new Vector2({F(viewport.SnapBase.X)}, {F(viewport.SnapBase.Y)}),");
        }
        if (Math.Abs(viewport.SnapSpacing.X - 10.0) > 1e-12 || Math.Abs(viewport.SnapSpacing.Y - 10.0) > 1e-12)
        {
            sb.AppendLine($"SnapSpacing = new Vector2({F(viewport.SnapSpacing.X)}, {F(viewport.SnapSpacing.Y)}),");
        }
        if (Math.Abs(viewport.GridSpacing.X - 10.0) > 1e-12 || Math.Abs(viewport.GridSpacing.Y - 10.0) > 1e-12)
        {
            sb.AppendLine($"GridSpacing = new Vector2({F(viewport.GridSpacing.X)}, {F(viewport.GridSpacing.Y)}),");
        }
        if (Math.Abs(viewport.FrontClipPlane) > 1e-12)
        {
            sb.AppendLine($"FrontClipPlane = {F(viewport.FrontClipPlane)},");
        }
        if (Math.Abs(viewport.BackClipPlane) > 1e-12)
        {
            sb.AppendLine($"BackClipPlane = {F(viewport.BackClipPlane)},");
        }
        if (Math.Abs(viewport.SnapAngle) > 1e-12)
        {
            sb.AppendLine($"SnapAngle = {F(viewport.SnapAngle)},");
        }
        if (Math.Abs(viewport.UcsOrigin.X) > 1e-12 || Math.Abs(viewport.UcsOrigin.Y) > 1e-12 || Math.Abs(viewport.UcsOrigin.Z) > 1e-12)
        {
            sb.AppendLine($"UcsOrigin = new Vector3({F(viewport.UcsOrigin.X)}, {F(viewport.UcsOrigin.Y)}, {F(viewport.UcsOrigin.Z)}),");
        }
        if (Math.Abs(viewport.UcsXAxis.X - 1.0) > 1e-12 || Math.Abs(viewport.UcsXAxis.Y) > 1e-12 || Math.Abs(viewport.UcsXAxis.Z) > 1e-12)
        {
            sb.AppendLine($"UcsXAxis = new Vector3({F(viewport.UcsXAxis.X)}, {F(viewport.UcsXAxis.Y)}, {F(viewport.UcsXAxis.Z)}),");
        }
        if (Math.Abs(viewport.UcsYAxis.X) > 1e-12 || Math.Abs(viewport.UcsYAxis.Y - 1.0) > 1e-12 || Math.Abs(viewport.UcsYAxis.Z) > 1e-12)
        {
            sb.AppendLine($"UcsYAxis = new Vector3({F(viewport.UcsYAxis.X)}, {F(viewport.UcsYAxis.Y)}, {F(viewport.UcsYAxis.Z)}),");
        }
        if (Math.Abs(viewport.Elevation) > 1e-12)
        {
            sb.AppendLine($"Elevation = {F(viewport.Elevation)},");
        }
        // Clipping boundary (non-rectangular)
        if (viewport.ClippingBoundary != null)
        {
            // We will try to rebuild the clipping boundary entity if possible
            // For now support Polyline2D and Circle boundaries
            sb.AppendLine($"ClippingBoundary = ");
            if (viewport.ClippingBoundary is Polyline2D poly2d)
            {
                GeneratePolyline2DConstructor(sb, poly2d);
                sb.AppendLine(";");
            }
            else if (viewport.ClippingBoundary is Circle circle)
            {
                GenerateCircleConstructor(sb, circle);
                sb.AppendLine(";");
            }
            else
            {
                sb.AppendLine($"null,");
            }
        }
        sb.AppendLine($"}};");

        // Add viewport to document before modifying collections that require ownership
        sb.AppendLine($"doc.Entities.Add({vpVar});");

        // Frozen layers (requires viewport to be owned by document)
        if (viewport.FrozenLayers is { Count: > 0 })
        {
            foreach (var layer in viewport.FrozenLayers)
            {
                sb.AppendLine($"{vpVar}.FrozenLayers.Add(doc.Layers[\"{Escape(layer.Name)}\"]);");
            }
        }

        // Some properties may be overridden when the entity is added to the document.
        // Ensure Elevation matches the source after ownership is established.
        if (Math.Abs(viewport.Elevation) > 1e-12)
        {
            sb.AppendLine($"{vpVar}.Elevation = {F(viewport.Elevation)};");
        }
    }

    private string GenerateViewportStatusFlags(ViewportStatusFlags flags)
    {
        if ((int)flags == 0)
            return "(ViewportStatusFlags)0";

        var flagNames = new List<string>();

        foreach (ViewportStatusFlags flag in Enum.GetValues(typeof(ViewportStatusFlags)))
        {
            if ((int)flag != 0 && flags.HasFlag(flag))
            {
                flagNames.Add($"ViewportStatusFlags.{flag}");
            }
        }

        return flagNames.Count > 0 ? string.Join(" | ", flagNames) : "(ViewportStatusFlags)0";
    }

    private void GenerateDimensionStyleProperties(StringBuilder sb, Dimension dimension)
    {
        // Generate dimension style properties if they differ from defaults
        if (dimension.Style != null && dimension.Style.Name != "Standard")
        {
            sb.AppendLine($"Style = doc.DimensionStyles[\"{dimension.Style.Name}\"],");
        }

        // Generate text rotation if not zero
        if (Math.Abs(dimension.TextRotation) > 1e-6)
        {
            sb.AppendLine($"TextRotation = {F(dimension.TextRotation)},");
        }

        // Generate user text if specified
        if (!string.IsNullOrEmpty(dimension.UserText))
        {
            sb.AppendLine($"UserText = \"{Escape(dimension.UserText)}\",");
        }

        // Generate attachment point if not default
        if (dimension.AttachmentPoint != MTextAttachmentPoint.TopLeft)
        {
            sb.AppendLine($"AttachmentPoint = MTextAttachmentPoint.{dimension.AttachmentPoint},");
        }

        // Generate line spacing properties if not default
        if (dimension.LineSpacingStyle != MTextLineSpacingStyle.AtLeast)
        {
            sb.AppendLine($"LineSpacingStyle = MTextLineSpacingStyle.{dimension.LineSpacingStyle},");
        }

        if (Math.Abs(dimension.LineSpacingFactor - 1.0) > 1e-6)
        {
            sb.AppendLine($"LineSpacingFactor = {F(dimension.LineSpacingFactor)},");
        }

        // Generate elevation if not zero
        if (Math.Abs(dimension.Elevation) > 1e-6)
        {
            sb.AppendLine($"Elevation = {F(dimension.Elevation)},");
        }
    }

    private void GenerateDimensionStyleOverrides(StringBuilder sb, Dimension dimension, string entityVariableName)
    {
        // Generate style overrides if any exist
        if (dimension.StyleOverrides is { Count: > 0 })
        {
            sb.AppendLine($"// Style overrides:");
            foreach (var kvp in dimension.StyleOverrides)
            {
                var overrideValue = FormatStyleOverrideValue(kvp.Value.Value);
                sb.AppendLine($"{entityVariableName}.StyleOverrides.Add(DimensionStyleOverrideType.{kvp.Key}, {overrideValue});");
            }
        }
    }

    private string FormatStyleOverrideValue(object? value)
    {
        if (value == null)
            return "null";

        switch (value)
        {
            case double d:
                return F(d);
            case float f:
                return F(f);
            case int i:
                return i.ToString();
            case bool b:
                return b.ToString().ToLower();
            case string s:
                return $"\"{Escape(s)}\"";
            case AciColor color:
                if (color.IsByLayer)
                    return "AciColor.ByLayer";
                if (color.IsByBlock)
                    return "AciColor.ByBlock";
                return $"new AciColor({color.Index})";
            case Linetype linetype:
                return $"doc.Linetypes[\"{linetype.Name}\"]";
            case Lineweight lineweight:
                return $"Lineweight.{lineweight}";
            case netDxf.Blocks.Block block:
                return $"doc.Blocks[\"{block.Name}\"]";
            case TextStyle textStyle:
                return $"doc.TextStyles[\"{textStyle.Name}\"]";
            case FractionFormatType fractionType:
                return $"FractionFormatType.{fractionType}";
            case LinearUnitType linearUnit:
                return $"LinearUnitType.{linearUnit}";
            case AngleUnitType angleUnit:
                return $"AngleUnitType.{angleUnit}";
            case Enum enumValue:
                return $"{enumValue.GetType().Name}.{enumValue}";
            default:
                return value.ToString() ?? string.Empty;
        }
    }

    private void GenerateSplineAdvancedProperties(StringBuilder sb, Spline spline)
    {
        // Generate fit tolerance if not default
        if (Math.Abs(spline.FitTolerance - 0.0000000001) > 1e-15)
        {
            sb.AppendLine($"splineEntity.FitTolerance = {F(spline.FitTolerance)};");
        }

        // Generate control point tolerance if not default
        if (Math.Abs(spline.CtrlPointTolerance - 0.0000001) > 1e-10)
        {
            sb.AppendLine($"splineEntity.CtrlPointTolerance = {F(spline.CtrlPointTolerance)};");
        }

        // Generate knot parameterization if not default
        if (spline.KnotParameterization != SplineKnotParameterization.FitChord)
        {
            sb.AppendLine($"splineEntity.KnotParameterization = SplineKnotParameterization.{spline.KnotParameterization};");
        }

        // Generate start tangent if specified
        if (spline.StartTangent.HasValue)
        {
            var tangent = spline.StartTangent.Value;
            sb.AppendLine($"splineEntity.StartTangent = new Vector3({F(tangent.X)}, {F(tangent.Y)}, {F(tangent.Z)});");
        }

        // Generate end tangent if specified
        if (spline.EndTangent.HasValue)
        {
            var tangent = spline.EndTangent.Value;
            sb.AppendLine($"splineEntity.EndTangent = new Vector3({F(tangent.X)}, {F(tangent.Y)}, {F(tangent.Z)});");
        }
    }

    private void GenerateTextAdvancedProperties(StringBuilder sb, Text text)
    {
        // Style
        if (text.Style != null)
        {
            if (text.Style.Name != "Standard")
            {
                sb.AppendLine($"Style = textStyle{SafeName(text.Style.Name!)},");
            }
        }

        // Rotation
        if (Math.Abs(text.Rotation) > 1e-12)
        {
            sb.AppendLine($"Rotation = {F(text.Rotation)},");
        }

        // WidthFactor (default is 1.0)
        if (Math.Abs(text.WidthFactor - 1.0) > 1e-12)
        {
            sb.AppendLine($"WidthFactor = {F(text.WidthFactor)},");
        }

        // ObliqueAngle (default is 0.0)
        if (Math.Abs(text.ObliqueAngle) > 1e-12)
        {
            sb.AppendLine($"ObliqueAngle = {F(text.ObliqueAngle)},");
        }

        // IsBackward (default is false)
        if (text.IsBackward)
        {
            sb.AppendLine($"IsBackward = true,");
        }

        // IsUpsideDown (default is false)
        if (text.IsUpsideDown)
        {
            sb.AppendLine($"IsUpsideDown = true,");
        }
    }

    private void GenerateLeaderAdvancedProperties(StringBuilder sb, Leader leader, string entityName)
    {
        // Style property
        if (leader.Style != null)
        {
            if (leader.Style.Name != "Standard")
            {
                if (string.IsNullOrEmpty(entityName))
                {
                    sb.AppendLine($"Style = dimStyle{SafeName(leader.Style.Name!)},");
                }
                else
                {
                    sb.AppendLine($"{entityName}.Style = dimStyle{SafeName(leader.Style.Name!)};");
                }
            }
        }

        // ShowArrowhead property (default is true)
        if (!leader.ShowArrowhead)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                sb.AppendLine($"ShowArrowhead = false,");
            }
            else
            {
                sb.AppendLine($"{entityName}.ShowArrowhead = false;");
            }
        }

        // PathType property (default is StraightLineSegments)
        if (leader.PathType != LeaderPathType.StraightLineSegments)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                sb.AppendLine($"PathType = LeaderPathType.{leader.PathType},");
            }
            else
            {
                sb.AppendLine($"{entityName}.PathType = LeaderPathType.{leader.PathType};");
            }
        }

        // HasHookline property (default is false)
        if (leader.HasHookline)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                sb.AppendLine($"HasHookline = true,");
            }
            else
            {
                sb.AppendLine($"{entityName}.HasHookline = true;");
            }
        }

        // LineColor property (default is ByLayer)
        if (leader.LineColor.Index != 256) // Not ByLayer
        {
            if (leader.LineColor.Index == 0)
            {
                if (string.IsNullOrEmpty(entityName))
                {
                    sb.AppendLine($"LineColor = AciColor.ByBlock,");
                }
                else
                {
                    sb.AppendLine($"{entityName}.LineColor = AciColor.ByBlock;");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(entityName))
                {
                    sb.AppendLine($"LineColor = new AciColor({leader.LineColor.Index}),");
                }
                else
                {
                    sb.AppendLine($"{entityName}.LineColor = new AciColor({leader.LineColor.Index});");
                }
            }
        }

        // Elevation property (default is 0.0)
        if (Math.Abs(leader.Elevation) > 1e-12)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                sb.AppendLine($"Elevation = {F(leader.Elevation)},");
            }
            else
            {
                sb.AppendLine($"{entityName}.Elevation = {F(leader.Elevation)};");
            }
        }

        // Offset property (default is Vector2.Zero)
        if (Math.Abs(leader.Offset.X) > 1e-12 || Math.Abs(leader.Offset.Y) > 1e-12)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                sb.AppendLine($"Offset = new Vector2({F(leader.Offset.X)}, {F(leader.Offset.Y)}),");
            }
            else
            {
                sb.AppendLine($"{entityName}.Offset = new Vector2({F(leader.Offset.X)}, {F(leader.Offset.Y)});");
            }
        }

        // Direction property (default is Vector2.UnitX)
        if (Math.Abs(leader.Direction.X - 1.0) > 1e-12 || Math.Abs(leader.Direction.Y) > 1e-12)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                sb.AppendLine($"Direction = new Vector2({F(leader.Direction.X)}, {F(leader.Direction.Y)}),");
            }
            else
            {
                sb.AppendLine($"{entityName}.Direction = new Vector2({F(leader.Direction.X)}, {F(leader.Direction.Y)});");
            }
        }
    }

    private void GenerateXData(StringBuilder sb, EntityObject entity, string entityVarName)
    {
        if (entity.XData == null || entity.XData.Count == 0)
            return;

        foreach (var xdata in entity.XData.Values)
        {
            var xdVar = $"xdata{_entityCounter++}";
            sb.AppendLine($"var {xdVar} = new XData(doc.ApplicationRegistries[\"{Escape(xdata.ApplicationRegistry.Name)}\"]);");
            foreach (var record in xdata.XDataRecord)
            {
                // Ensure we emit literals with the exact types required by XDataCode
                string valueStr;
                switch (record.Code)
                {
                    case XDataCode.Int16:
                        // Explicit short cast to avoid boxing as int when value is a constant like 0 or 1
                        valueStr = $"(short){Convert.ToInt16(record.Value, CultureInfo.InvariantCulture)}";
                        break;
                    case XDataCode.Real:
                    case XDataCode.RealX:
                    case XDataCode.RealY:
                    case XDataCode.RealZ:
                        valueStr = F(Convert.ToDouble(record.Value, CultureInfo.InvariantCulture));
                        break;
                    default:
                        valueStr = record.Value switch
                        {
                            string s => $"\"{Escape(s)}\"",
                            double d => F(d),
                            float f => F(f),
                            short sh => $"(short){sh}",
                            int i => i.ToString(CultureInfo.InvariantCulture),
                            byte[] arr => $"new byte[] {{ {string.Join(", ", arr)} }}",
                            _ => $"\"{Escape(record.Value?.ToString() ?? string.Empty)}\""
                        };
                        break;
                }

                sb.AppendLine($"{xdVar}.XDataRecord.Add(new XDataRecord(XDataCode.{record.Code}, {valueStr}));");
            }
            sb.AppendLine($"{entityVarName}.XData.Add({xdVar});");
        }
    }
}
