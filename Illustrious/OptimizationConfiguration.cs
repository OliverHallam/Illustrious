//------------------------------------------------------------------------------------------------- 
// <copyright file="OptimizationConfiguration.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the OptimizationConfiguration type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System.Diagnostics.CodeAnalysis;
    using Mono.Cecil;

    /// <summary>
    /// Provider of configuration settings that control the behaviour of the optimizer.
    /// </summary>
    public class OptimizationConfiguration
    {
        /// <summary>
        /// Returns a boolean value indicating whether the given method should be inlined.
        /// </summary>
        /// <param name="method">The method to test.</param>
        /// <returns><see langword="true"/> if the method should be inlined; <see langword="false"/> otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Placeholder for proper configuration")]
        public bool ShouldInline(MethodDefinition method)
        {
            // TODO: refine these conditions.
            
            // TODO: prevent inlining if method has too many locals(!)
            var returnType = method.ReturnType;
            if (!(returnType.ReturnType.FullName == "System.Void"))
            {
                return false;
            }

            if (method.IsAbstract || method.IsVirtual)
            {
                return false;
            }

            if (method.Parameters.Count != 0)
            {
                return false;
            }

            var body = method.Body;
            if (body == null)
            {
                return false;
            }

            /*
            if (body.CodeSize > 32)
            {
                return false;
            }
            */

            return true;
        }
    }
}