using System.Text.RegularExpressions;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace DxfToCSharp.Services;

/// <summary>
/// Folding strategy for C# code that creates foldable regions for methods, classes, namespaces, and other code blocks.
/// </summary>
public class CSharpFoldingStrategy
{
    /// <summary>
    /// Creates folding regions for C# code.
    /// </summary>
    public IEnumerable<NewFolding> CreateNewFoldings(TextDocument? document, out int firstErrorOffset)
    {
        firstErrorOffset = -1;
        var foldings = new List<NewFolding>();

        if (document == null || document.TextLength == 0)
        {
            return foldings;
        }

        var lines = document.Lines.ToArray();

        // Stack to track opening braces and their contexts
        var braceStack = new Stack<(int offset, string context, int lineNumber)>();

        // Regex patterns for different C# constructs
        var namespacePattern = new Regex(@"^\s*namespace\s+([\w\.]+)\s*\{?", RegexOptions.Multiline);
        var classPattern = new Regex(@"^\s*(public|private|protected|internal)?\s*(static|abstract|sealed)?\s*(partial)?\s*(class|interface|struct|enum)\s+([\w<>]+).*\{?", RegexOptions.Multiline);
        var methodPattern = new Regex(@"^\s*(public|private|protected|internal)?\s*(static|virtual|override|abstract)?\s*([\w<>\[\]]+)\s+([\w]+)\s*\([^\)]*\)\s*\{?", RegexOptions.Multiline);
        var propertyPattern = new Regex(@"^\s*(public|private|protected|internal)?\s*(static|virtual|override|abstract)?\s*([\w<>\[\]]+)\s+([\w]+)\s*\{\s*$", RegexOptions.Multiline);
        var regionPattern = new Regex(@"^\s*#region\s+(.*)$", RegexOptions.Multiline);
        var endRegionPattern = new Regex(@"^\s*#endregion", RegexOptions.Multiline);

        // Track #region blocks
        var regionStack = new Stack<(int offset, string name)>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = document.GetText(lines[i]);
            var trimmedLine = line.Trim();

            // Handle #region and #endregion
            var regionMatch = regionPattern.Match(line);
            if (regionMatch.Success)
            {
                var regionName = regionMatch.Groups[1].Value.Trim();
                regionStack.Push((lines[i].Offset, regionName));
                continue;
            }

            var endRegionMatch = endRegionPattern.Match(line);
            if (endRegionMatch.Success && regionStack.Count > 0)
            {
                var (startOffset, regionName) = regionStack.Pop();
                var endOffset = lines[i].EndOffset;

                foldings.Add(new NewFolding(startOffset, endOffset)
                {
                    Name = $"#region {regionName}",
                    IsDefinition = true
                });
                continue;
            }

            // Handle braces and code blocks
            var openBraceIndex = line.IndexOf('{');
            var closeBraceIndex = line.IndexOf('}');

            // Determine context for opening brace
            if (openBraceIndex >= 0)
            {
                var context = "block";

                // Check what kind of block this is
                var namespaceMatch = namespacePattern.Match(line);
                var classMatch = classPattern.Match(line);
                var methodMatch = methodPattern.Match(line);
                var propertyMatch = propertyPattern.Match(line);

                if (namespaceMatch.Success)
                {
                    context = $"namespace {namespaceMatch.Groups[1].Value}";
                }
                else if (classMatch.Success)
                {
                    var classType = classMatch.Groups[4].Value; // class, interface, struct, enum
                    var className = classMatch.Groups[5].Value;
                    context = $"{classType} {className}";
                }
                else if (methodMatch.Success)
                {
                    var methodName = methodMatch.Groups[4].Value;
                    context = $"method {methodName}";
                }
                else if (propertyMatch.Success)
                {
                    var propertyName = propertyMatch.Groups[4].Value;
                    context = $"property {propertyName}";
                }
                else if (trimmedLine.Contains("get") || trimmedLine.Contains("set"))
                {
                    context = "accessor";
                }
                else if (trimmedLine.Contains("if") || trimmedLine.Contains("else") ||
                         trimmedLine.Contains("for") || trimmedLine.Contains("foreach") ||
                         trimmedLine.Contains("while") || trimmedLine.Contains("do") ||
                         trimmedLine.Contains("switch") || trimmedLine.Contains("try") ||
                         trimmedLine.Contains("catch") || trimmedLine.Contains("finally"))
                {
                    context = "control block";
                }

                var braceOffset = lines[i].Offset + openBraceIndex;
                braceStack.Push((braceOffset, context, i + 1));
            }

            // Handle closing brace
            if (closeBraceIndex >= 0 && braceStack.Count > 0)
            {
                var (startOffset, context, lineNumber) = braceStack.Pop();
                var endOffset = lines[i].Offset + closeBraceIndex + 1;

                // Only create folding for blocks that span multiple lines and are significant
                if (i + 1 > lineNumber + 1 && !context.Equals("accessor") && !context.Equals("control block"))
                {
                    foldings.Add(new NewFolding(startOffset, endOffset)
                    {
                        Name = context,
                        IsDefinition = context.StartsWith("namespace") || context.StartsWith("class") ||
                                       context.StartsWith("interface") || context.StartsWith("struct") ||
                                       context.StartsWith("enum")
                    });
                }
            }
        }

        // Handle using statements block
        var usingFolding = CreateUsingStatementsFolding(document, lines);
        if (usingFolding != null)
        {
            foldings.Add(usingFolding);
        }

        return foldings.OrderBy(f => f.StartOffset);
    }

    /// <summary>
    /// Creates a folding region for using statements at the top of the file.
    /// </summary>
    private NewFolding? CreateUsingStatementsFolding(TextDocument document, DocumentLine[] lines)
    {
        var firstUsingLine = -1;
        var lastUsingLine = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = document.GetText(lines[i]).Trim();

            if (line.StartsWith("using ") && line.EndsWith(";"))
            {
                if (firstUsingLine == -1)
                    firstUsingLine = i;
                lastUsingLine = i;
            }
            else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//") && firstUsingLine != -1)
            {
                // Stop when we hit non-using, non-comment, non-empty line
                break;
            }
        }

        // Create folding if we have multiple using statements
        if (firstUsingLine != -1 && lastUsingLine > firstUsingLine)
        {
            return new NewFolding(lines[firstUsingLine].Offset, lines[lastUsingLine].EndOffset)
            {
                Name = "using statements",
                IsDefinition = false
            };
        }

        return null;
    }
}
