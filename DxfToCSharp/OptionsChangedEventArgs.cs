using System;
using DxfToCSharp.Core;

namespace DxfToCSharp
{
    /// <summary>
    /// Event arguments for options changed events
    /// </summary>
    public class OptionsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The updated DXF code generation options
        /// </summary>
        public DxfCodeGenerationOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the OptionsChangedEventArgs class
        /// </summary>
        /// <param name="options">The updated options</param>
        public OptionsChangedEventArgs(DxfCodeGenerationOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}