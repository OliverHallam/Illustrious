//------------------------------------------------------------------------------------------------- 
// <copyright file="Configuration.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the Configuration type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Inliner
{
    using Mono.Cecil;

    /// <summary>
    /// Provider of configuration settings that control the behaviour of the optimizer.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Returns a boolean value indicating whether the given method should be inlined.
        /// </summary>
        /// <param name="method">The method to test.</param>
        /// <returns><see langword="true"/> if the method should be inlined; <see langword="false"/> otherwise.</returns>
        public bool ShouldInline(MethodDefinition method)
        {
            // TODO: refine these conditions.
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

            if (body.CodeSize > 32)
            {
                return false;
            }

            return true;
        }
    }
}
