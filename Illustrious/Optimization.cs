//------------------------------------------------------------------------------------------------- 
// <copyright file="Optimization.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the Optimization type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// An optimization that is applied to a stream of IL.
    /// </summary>
    public abstract class Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="optimizer">The optimizer calling this method.</param>
        /// <param name="worker">A CIL worker for the current method.</param>
        /// <param name="instruction">
        /// The instruction to be visited.  Receives the first changed instruction of the optimized version.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an optimization was performed; otherwise <see langword="false"/>.
        /// </returns>
        public abstract bool OptimizeInstruction(Optimizer optimizer, CilWorker worker, ref Instruction instruction);
    }
}