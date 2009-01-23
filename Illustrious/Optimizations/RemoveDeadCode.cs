//------------------------------------------------------------------------------------------------- 
// <copyright file="RemoveDeadCode.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the RemoveDeadCode optimization.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious.Optimizations
{
    /// <summary>
    /// Removes instructions from a method which can never be executed.
    /// </summary>
    public class RemoveDeadCode : Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(OptimizationWorker worker)
        {
            var targetInstruction = worker.TargetInstruction;

            // the first instruction in a method is not dead code
            if (targetInstruction.Previous == null)
                return;

            var sources = worker.SourceInstructions(targetInstruction);
            if (sources.Count == 0)
            {
                worker.DeleteInstruction(targetInstruction);
            }
        }
    }
}
