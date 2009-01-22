//------------------------------------------------------------------------------------------------- 
// <copyright file="RetargetDoubleBranch.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the RetargetDoubleBranch optimization.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious.Optimizations
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// Finds a branch or conditional branch instruction whose operand is a branch, and retargets
    /// it to jump directly.
    /// </summary>
    public class RetargetDoubleBranch : Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(OptimizationWorker worker)
        {
            var instruction = worker.TargetInstruction;
            var opCode = instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Branch ||
                opCode.FlowControl == FlowControl.Cond_Branch)
            {
                var target = (Instruction)instruction.Operand;
                if (target.OpCode.FlowControl == FlowControl.Branch)
                {
                    instruction.Operand = target.Operand;
                }
            }
        }
    }
}
