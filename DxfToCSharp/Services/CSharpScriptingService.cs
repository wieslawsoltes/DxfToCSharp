using DxfToCSharp.Compilation;

namespace DxfToCSharp.Services
{
    public class CSharpScriptingService
    {
        private readonly CompilationService _compilationService;
        
        public record CompilationResult(bool Success, string? AssemblyPath, string Output);
        
        public CSharpScriptingService()
        {
            _compilationService = new CompilationService();
        }

        public CompilationResult Compile(string sourceCode)
        {
            var result = _compilationService.CompileToFile(sourceCode);
            return new CompilationResult(result.Success, result.AssemblyPath, result.Output);
        }

        public string RunCreateMethod(string assemblyPath)
        {
            return _compilationService.RunCreateMethod(assemblyPath);
        }


    }
}