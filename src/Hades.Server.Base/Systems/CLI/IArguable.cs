using System.Collections.Generic;

namespace Pyratron.Frameworks.Commands.Parser
{
    /// <summary>
    /// Represents an object that has multiple arguments/parameters.
    /// </summary>
    internal interface IArguable // Calm down! Why are we arguing?!
    {
        /// <summary>
        /// The arguments the object contains. Arguments may be nested inside others to create links of arguments.
        /// </summary>
        List<Argument> Arguments { get; }
    }
}