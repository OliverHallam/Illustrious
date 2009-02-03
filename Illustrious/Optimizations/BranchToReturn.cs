//------------------------------------------------------------------------------------------------- 
// <copyright file="BranchToReturn.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the BranchToReturn optimization.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious.Optimizations
{
    using Mono.Cecil.Cil;

    /// <summary>
    /// Converts a branch instruction targeting a <c>ret</c> or <c>throw</c> instruction to a 
    /// <c>ret</c> or <c>throw</c> instruction.
    /// </summary>
    public class BranchToReturn : Optimization
    {
        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="instruction">The instruction to target.</param>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(Instruction instruction, OptimizationWorker worker)
        {
            var opCode = instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Branch)
            {
                var target = (Instruction)instruction.Operand;
                var targetOpCode = target.OpCode;
                if (targetOpCode.FlowControl == FlowControl.Return ||
                    targetOpCode.FlowControl == FlowControl.Throw)
                {
                    var replacement = worker.CilWorker.Create(targetOpCode);
                    worker.ReplaceInstruction(instruction, replacement);
                }
            }
        }
    }
}
