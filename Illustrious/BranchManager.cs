//------------------------------------------------------------------------------------------------- 
// <copyright file="BranchManager.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the BranchManager type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil.Cil;

    /// <summary>
    /// A manager for branch instructions that allows for branch retargeting.
    /// </summary>
    public class BranchManager
    {
        /// <summary>
        /// The mapping from branch target to branch instruction.
        /// </summary>
        private readonly Dictionary<Instruction, List<Instruction>> branchSources =
            new Dictionary<Instruction, List<Instruction>>();

        /// <summary>
        /// Adds a branch to the collection.
        /// </summary>
        /// <param name="instruction">The branch or conditional branch instruction.</param>
        /// <exception cref="ArgumentException"><paramref name="instruction"/> is not a branch or conditional branch.</exception>
        public void Add(Instruction instruction)
        {
            if (instruction.OpCode.FlowControl != FlowControl.Branch &&
                instruction.OpCode.FlowControl != FlowControl.Cond_Branch)
            {
                throw new ArgumentException("instruction is not a branch or conditional branch", "instruction");
            }

            var target = (Instruction) instruction.Operand;
            List<Instruction> sources;
            if (this.branchSources.TryGetValue(target, out sources))
            {
                sources.Add(instruction);
            }
            else
            {
                sources = new List<Instruction> { instruction };
                this.branchSources.Add(target, sources);
            }
        }

        /// <summary>
        /// Removes the specified instruction from the collection.
        /// </summary>
        /// <param name="instruction">The instruction to remove.</param>
        public void Remove(Instruction instruction)
        {
            if (instruction.OpCode.FlowControl != FlowControl.Branch &&
                instruction.OpCode.FlowControl != FlowControl.Cond_Branch)
            {
                throw new ArgumentException("instruction is not a branch or conditional branch", "instruction");
            }

            var target = (Instruction)instruction.Operand;
            var sources = this.branchSources[target];
            if (sources.Count == 1)
            {
                this.branchSources.Remove(target);
            }
            else
            {
                sources.Remove(instruction);
            }
        }

        /// <summary>
        /// Modifies any branch in the collection which targets a particular instruction to target another instead.
        /// </summary>
        /// <param name="oldTarget">The old target.</param>
        /// <param name="newTarget">The new target.</param>
        public void Retarget(Instruction oldTarget, Instruction newTarget)
        {
            if (oldTarget == newTarget)
            {
                return;
            }

            List<Instruction> sources;
            if (!this.branchSources.TryGetValue(oldTarget, out sources))
            {
                return;
            }

            var sourceCount = sources.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                var source = sources[i];

                // TODO: change the instruction to a long branch if required and vice versa.
                source.Operand = newTarget;
            }

            this.branchSources.Remove(oldTarget);
            List<Instruction> newSources;
            if (this.branchSources.TryGetValue(newTarget, out newSources))
            {
                newSources.AddRange(sources);
            }
            else
            {
                this.branchSources.Add(newTarget, sources);
            }
        }
    }
}