//------------------------------------------------------------------------------------------------- 
// <copyright file="RemoveNopOptimization.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the RemoveNopOptimization type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// Optimization that removes the <c>nop</c> instruction.
    /// </summary>
    public class RemoveNopOptimization : Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="optimizer">The optimizer calling this method.</param>
        /// <param name="worker">A CIL worker for the current method.</param>
        /// <param name="instruction">The instruction to be visited.  Receives the first changed instruction of the optimized version.</param>
        /// <returns>
        /// <see langword="true"/> if an optimization was performed; otherwise <see langword="false"/>.
        /// </returns>
        public override bool OptimizeInstruction(Optimizer optimizer, CilWorker worker, ref Instruction instruction)
        {
            var opCode = instruction.OpCode;
            if (opCode.Code == Code.Nop)
            {
                var next = instruction.Next;
                worker.Remove(instruction);
                instruction = next;
                return true;
            }

            return false;
        }
    }
}
