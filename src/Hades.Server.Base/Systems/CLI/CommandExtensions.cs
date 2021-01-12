using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pyratron.Frameworks.Commands.Parser
{
    /// <summary>
    /// Provides extension methods for arguments and commands that can be used with the library.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Retrieves an argument's value by it's name from an <c>Argument</c> collection or array.
        /// </summary>
        /// <example>
        ///     <code>
        ///          private static void OnCommandExecuted(Argument[] args) {
        ///          var user = args.FromName("user");
        ///     </code>
        /// </example>
        public static string FromName(this IEnumerable<Argument> arguments, string name)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "Argument name may not be empty");

            string value = FromNameRecurse(arguments, name);
            if (!value.Equals(string.Empty))
                return value;

            throw new InvalidOperationException(string.Format("No argument of name {0} found.", name));
        }

        private static string FromNameRecurse(IEnumerable<Argument> arguments, string name)
        {
            //Search top level arguments first
            var enumerable = arguments as Argument[] ?? arguments.ToArray();
            foreach (var arg in enumerable)
            {
                if (arg.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return arg.Value;
            }

            //Recursively search children
            foreach (var arg in enumerable)
            {
                if (arg.Arguments.Count > 0) //If argument has nested args, recursively search
                {
                    string value = FromNameRecurse(arg.Arguments, name);
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }

            }
            return string.Empty;
        }

        /// <summary>
        /// Generates an readable argument string for the given arguments. (Ex: "&lt;player&gt; &lt;item&gt; [amount]")
        /// </summary>
        public static string GenerateArgumentString(this List<Argument> arguments)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");

            var sb = new StringBuilder();
            arguments.WriteArguments(sb);
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Generates an readable argument string for the given arguments. (Ex: "&lt;player&gt; &lt;item&gt; [amount]")
        /// (Different than <see cref="GenerateArgumentString" />, which is for public use and creates a <c>StringBuilder</c>)
        /// </summary>
        private static void WriteArguments(this List<Argument> arguments, StringBuilder sb)
        {
            if (arguments == null) throw new ArgumentNullException("arguments");

            for (var i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];

                //Write bracket, name, and closing bracket for each argument.
                sb.Append(arg.Optional ? '[' : '<');
                if (arg.Enum) //Print possible values if "enum".
                {
                    for (var j = 0; j < arg.Arguments.Count; j++)
                    {
                        var possibility = arg.Arguments[j];
                        sb.Append(possibility.Name.ToLower().Replace('_', ' '));
                        if (arg.Arguments[j].Arguments.Count >= 1) //Child arguments (Print each possible value).
                        {
                            sb.Append(' ');
                            WriteArguments(arg.Arguments[j].Arguments, sb);
                        }
                        if (j < arg.Arguments.Count - 1 && arg.Arguments.Count > 1) //Print "or".
                            sb.Append('|');
                    }
                }
                else
                {
                    sb.Append(arg.Name.ToLower().Replace('_', ' '));
                    if (arg.Arguments.Count >= 1) //Child arguments.
                    {
                        sb.Append(' ');
                        WriteArguments(arg.Arguments, sb);
                    }
                }

                //Closing tag
                sb.Append(arg.Optional ? "]" : ">");
                if (i != arguments.Count - 1)
                    sb.Append(' ');
            }
        }
    }
}