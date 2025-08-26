using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace DxfToCSharp.Services;

/// <summary>
/// Folding strategy for DXF files that creates foldable regions for sections and entities.
/// </summary>
public class DxfFoldingStrategy
{
    /// <summary>
    /// Creates folding regions for DXF content.
    /// </summary>
    public IEnumerable<NewFolding> CreateNewFoldings(TextDocument? document, out int firstErrorOffset)
    {
        firstErrorOffset = -1;
        var foldings = new List<NewFolding>();
            
        if (document == null || document.TextLength == 0)
            return foldings;

        var lines = document.Lines.ToArray();
   
        // Track section starts and ends
        var sectionStack = new Stack<(int startOffset, string sectionName)>();
            
        for (var i = 0; i < lines.Length - 1; i++)
        {
            var currentLine = document.GetText(lines[i]);
            var nextLine = i + 1 < lines.Length ? document.GetText(lines[i + 1]) : "";
                
            // Check for DXF section markers
            if (currentLine.Trim() == "0" && nextLine.Trim() == "SECTION")
            {
                // Look for section name in the next few lines
                var sectionName = "SECTION";
                if (i + 3 < lines.Length)
                {
                    var sectionNameLine = document.GetText(lines[i + 3]);
                    if (!string.IsNullOrWhiteSpace(sectionNameLine))
                    {
                        sectionName = sectionNameLine.Trim();
                    }
                }
                    
                sectionStack.Push((lines[i].Offset, sectionName));
            }
            else if (currentLine.Trim() == "0" && nextLine.Trim() == "ENDSEC")
            {
                // End of section
                if (sectionStack.Count > 0)
                {
                    var (startOffset, sectionName) = sectionStack.Pop();
                    var endOffset = lines[i + 1].EndOffset;
                        
                    if (endOffset > startOffset)
                    {
                        foldings.Add(new NewFolding(startOffset, endOffset)
                        {
                            Name = sectionName,
                            IsDefinition = true
                        });
                    }
                }
            }
            // Check for entity blocks (like LWPOLYLINE, LINE, etc.)
            else if (currentLine.Trim() == "0" && IsEntityType(nextLine.Trim()))
            {
                var entityType = nextLine.Trim();
                var startOffset = lines[i].Offset;
                    
                // Find the end of this entity (next "0" code)
                var endLineIndex = i + 2;
                while (endLineIndex < lines.Length)
                {
                    var checkLine = document.GetText(lines[endLineIndex]);
                    if (checkLine.Trim() == "0")
                        break;
                    endLineIndex++;
                }
                    
                if (endLineIndex < lines.Length && endLineIndex > i + 2)
                {
                    var endOffset = lines[endLineIndex - 1].EndOffset;
                    foldings.Add(new NewFolding(startOffset, endOffset)
                    {
                        Name = entityType,
                        IsDefinition = false
                    });
                }
            }
        }
            
        return foldings.OrderBy(f => f.StartOffset);
    }
        
    /// <summary>
    /// Checks if the given string represents a DXF entity type.
    /// </summary>
    private bool IsEntityType(string value)
    {
        var entityTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "LINE", "CIRCLE", "ARC", "ELLIPSE", "POINT", "TEXT", "MTEXT",
            "LWPOLYLINE", "POLYLINE", "SPLINE", "INSERT", "BLOCK", "ENDBLK",
            "DIMENSION", "LEADER", "HATCH", "SOLID", "3DFACE", "REGION",
            "BODY", "3DSOLID", "SURFACE", "MESH", "LIGHT", "CAMERA",
            "WIPEOUT", "OLEFRAME", "OLE2FRAME", "PROXY", "XLINE", "RAY",
            "MLINE", "TOLERANCE", "SHAPE", "VIEWPORT", "IMAGE", "IMAGEDEF",
            "IMAGEDEF_REACTOR", "PLOTSETTINGS", "LAYOUT", "ACDBDICTIONARYWDFLT",
            "ACDBPLACEHOLDER", "VBA_PROJECT", "MATERIAL", "VISUALSTYLE",
            "TABLESTYLE", "CELLSTYLEMAP", "MENTALRAYRENDERSETTINGS",
            "ACAD_PROXY_ENTITY", "ACAD_PROXY_OBJECT"
        };
            
        return entityTypes.Contains(value);
    }
}
