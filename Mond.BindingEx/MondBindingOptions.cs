using System;

namespace Mond.BindingEx
{
    [Flags]
    public enum MondBindingOptions
    {
        None = 0,

        /// <summary>
        ///     Automatically insert the generated bindings into the <see cref="MondState" /> globals if provided.
        /// </summary>
        AutoInsert = 1 << 0,

        /// <summary>
        ///     Automatically lock the generated binding object and prototype.
        /// </summary>
        AutoLock = 1 << 1,

        /// <summary>
        ///     The binder will automatically convert PascalCase and snake_case method and property names to camelCase, setting
        ///     this option preserves the original names.
        /// </summary>
        PreserveNames = 1 << 2
    }
}
