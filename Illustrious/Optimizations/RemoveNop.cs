//------------------------------------------------------------------------------------------------- 
// <copyright file="RemoveNop.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the RemoveNop type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious.Optimizations
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// Optimization that removes the <c>nop</c> instruction.
    /// </summary>
    public class RemoveNop : Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(OptimizationWorker worker)
        {
            var instruction = worker.TargetInstruction;
            var opCode = instruction.OpCode;
            if (opCode.Code == Code.Nop)
            {
                worker.DeleteInstruction();
            }
        }
    }
}