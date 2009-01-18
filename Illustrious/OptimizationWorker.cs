//------------------------------------------------------------------------------------------------- 
// <copyright file="OptimizationWorker.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the OptimizationWorker type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Provides support for IL optimizations.
    /// </summary>
    public class OptimizationWorker
    {
        /// <summary>
        /// The body of the method;
        /// </summary>
        private readonly MethodBody methodBody;

        /// <summary>
        /// The optimizer that created this optimization worker.
        /// </summary>
        private readonly Optimizer optimizer;

        /// <summary>
        /// The instruction that is currently being optimized.
        /// </summary>
        private Instruction currentInstruction;

        /// <summary>
        /// The instruction following any rewritings the optimization has performed.
        /// </summary>
        private Instruction nextInstruction;

        /// <summary>
        /// The instruction being targeted for optimization.
        /// </summary>
        private Instruction targetInstruction;

        /// <summary>
        /// A dictionary of the targets of all the branch instructions in the method.
        /// </summary>
        private Dictionary<Instruction, List<Instruction>> branchSources;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OptimizationWorker"/> class.
        /// </summary>
        /// <param name="optimizer">The optimizer used for performing optimizations.</param>
        /// <param name="methodBody">The method of the method to be optimized.</param>
        public OptimizationWorker(Optimizer optimizer, MethodBody methodBody)
        {
            this.optimizer = optimizer;
            this.methodBody = methodBody;
            this.currentInstruction = null;
            this.targetInstruction = null;
            this.nextInstruction = null;
        }

        /// <summary>
        /// Gets the target instruction.
        /// </summary>
        /// <value>The target instruction.</value>
        public Instruction TargetInstruction
        {
            get
            {
                return this.targetInstruction;
            }
        }

        /// <summary>
        /// Gets the instruction following any rewriting performed.
        /// </summary>
        /// <value>The instruction following any rewriting performed.</value>
        public Instruction NextInstruction
        {
            get
            {
                return this.nextInstruction;
            }
        }

        /// <summary>
        /// Gets the worker used for IL manipulation.
        /// </summary>
        public CilWorker CilWorker
        {
            get
            {
                return this.methodBody.CilWorker;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any rewritings were performed since the last call to <see cref="NextInstruction"/>.
        /// </summary>
        /// <value><c>true</c> if rewritings have been performed; otherwise, <c>false</c>.</value>
        public bool WasOptimized
        {
            get
            {
                return this.targetInstruction != this.currentInstruction ||
                       this.nextInstruction != this.currentInstruction.Next;
            }
        }

        /// <summary>
        /// Gets the branch sources.
        /// </summary>
        /// <value>The branch sources.</value>
        private Dictionary<Instruction, List<Instruction>> BranchSources
        {
            get
            {
                if (this.branchSources == null)
                {
                    this.FindBranches();
                }

                return this.branchSources;
            }
        }

        /// <summary>
        /// Deletes the current instruction
        /// </summary>
        public void DeleteInstruction()
        {
            if (this.currentInstruction == null)
            {
                throw new NotSupportedException();
            }

            this.DeleteInstruction(this.currentInstruction);
        }

        /// <summary>
        /// Deletes an instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        public void DeleteInstruction(Instruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }

            if (instruction == this.nextInstruction)
            {
                this.nextInstruction = this.nextInstruction.Next;
            }

            if (instruction == this.currentInstruction)
            {
                this.currentInstruction = this.currentInstruction.Previous;
            }

            List<Instruction> branches;
            if (this.BranchSources.TryGetValue(instruction, out branches))
            {
                var branchesLength = branches.Count;
                if (branchesLength > 0)
                {
                    var followingInstruction = instruction.Next;
                    if (followingInstruction == null)
                    {
                        throw new NotImplementedException("Convert branch to ret.");
                    }

                    var newTarget = instruction.Next;

                    for (int i = 0; i < branchesLength; i++)
                    {
                        branches[i].Operand = newTarget;
                    }

                    // now update the cache to reflect changes.
                    List<Instruction> newTargetBranches;
                    if (this.BranchSources.TryGetValue(newTarget, out newTargetBranches))
                    {
                        newTargetBranches.AddRange(branches);
                    }
                    else
                    {
                        this.BranchSources[newTarget] = branches;
                    }

                    this.BranchSources.Remove(instruction);
                }
            }

            this.CilWorker.Remove(instruction);
        }

        /// <summary>
        /// Inserts an instruction into the IL stream, following the target instruction and any other instructions added.
        /// </summary>
        /// <param name="instruction">The instruction to insert.</param>
        public void InsertInstruction(Instruction instruction)
        {
            if (this.nextInstruction == null)
                throw new NotImplementedException("inserting to the end of a stream");

            this.CilWorker.InsertBefore(this.nextInstruction, instruction);
            
            // if we are adding a branch instruction then update our cache.
            var opCode = instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Branch || opCode.FlowControl == FlowControl.Cond_Branch)
            {
                if (this.branchSources != null)
                {
                    this.AddBranch(instruction);
                }
            }
        }

        /// <summary>
        /// Moves the worker to the next instruction.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the worker was successfully moved to the next instruction;
        /// <see langword="false"/> if the worker reached the end of the method.
        /// </returns>
        public bool MoveNext()
        {
            if (this.currentInstruction == null)
            {
                if (this.methodBody.Instructions.Count != 0)
                {
                    this.currentInstruction = this.methodBody.Instructions[0];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this.currentInstruction = this.currentInstruction.Next;
            
                if (this.currentInstruction == null)
                {
                    return false;
                }
            }

            this.targetInstruction = this.currentInstruction;
            this.nextInstruction = this.currentInstruction.Next;
            return true;
        }

        /// <summary>
        /// Optimizes the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        public void Optimize(MethodDefinition method)
        {
            this.optimizer.Visit(method);
        }

        /// <summary>
        /// Adds as branch to the branch index.
        /// </summary>
        /// <param name="instruction">The branch instruction.</param>
        private void AddBranch(Instruction instruction)
        {
            var opCode = instruction.OpCode;
            if (opCode.OperandType != OperandType.InlineBrTarget &&
                opCode.OperandType != OperandType.ShortInlineBrTarget)
            {
                throw new NotImplementedException();
            }

            var target = (Instruction)instruction.Operand;
            List<Instruction> branches;
            if (this.branchSources.TryGetValue(target, out branches))
            {
                branches.Add(instruction);
            }
            else
            {
                branches = new List<Instruction> { instruction };
                this.branchSources[target] = branches;
            }
        }

        /// <summary>
        /// Initializes the branch sources map.
        /// </summary>
        private void FindBranches()
        {
            this.branchSources = new Dictionary<Instruction, List<Instruction>>();

            var instructions = this.methodBody.Instructions;
            var instructionCount = instructions.Count;
            for (var i = 0; i < instructionCount; i++)
            {
                var instruction = instructions[i];
                var opCode = instruction.OpCode;
                if (opCode.FlowControl == FlowControl.Branch || opCode.FlowControl == FlowControl.Cond_Branch)
                {
                    this.AddBranch(instruction);
                }
            }
        }
    }
}