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
        /// Performs the optimization starting at the specified instruction.
        /// </summary>
        /// <param name="instruction">The instruction to target.</param>
        /// <param name="worker">The worker for optimization actions.</param>
        public abstract void OptimizeInstruction(Instruction instruction, OptimizationWorker worker);
    }
}