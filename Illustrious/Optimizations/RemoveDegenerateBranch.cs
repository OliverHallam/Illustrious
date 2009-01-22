//------------------------------------------------------------------------------------------------- 
// <copyright file="RemoveDegenerateBranch.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the RemoveDegenerateBranch type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious.Optimizations
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// Optimization that removes branch or conditional branch instructions that jump to the 
    /// following instruction.
    /// </summary>
    public class RemoveDegenerateBranch : Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(OptimizationWorker worker)
        {
            // TODO: currently if an optimization removes an instruction between a branch and its
            // target, this will not trigger, since the target instruction will be the one after the
            // branch.
            //   This can be solved by adding a property to the optimization indicating how many
            // instructions ahead can affect its application.
            var instruction = worker.TargetInstruction;
            var opCode = instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Branch ||
                opCode.FlowControl == FlowControl.Cond_Branch)
            {
                var target = instruction.Operand as Instruction;
                if (target == instruction.Next)
                {
                    worker.DeleteInstruction();
                }
            }
        }
    }
}