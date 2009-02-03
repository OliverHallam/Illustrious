//------------------------------------------------------------------------------------------------- 
// <copyright file="RemoveDeadCode.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the RemoveDeadCode optimization.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious.Optimizations
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// Removes instructions from a method which can never be executed.
    /// </summary>
    public class RemoveDeadCode : Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="instruction">The instruction to target.</param>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(Instruction instruction, OptimizationWorker worker)
        {
            // the first instruction in a method is not dead code
            if (instruction.Previous == null)
            {
                return;
            }

            var sources = worker.SourceInstructions(instruction);
            if (sources.Count == 0)
            {
                worker.DeleteInstruction(instruction);
            }
        }
    }
}
