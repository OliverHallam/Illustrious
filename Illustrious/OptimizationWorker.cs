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
    using System.Diagnostics.CodeAnalysis;
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
        /// The branches in the current method.
        /// </summary>
        private BranchManager branches;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptimizationWorker"/> class.
        /// </summary>
        /// <param name="optimizer">The optimizer used for performing optimizations.</param>
        /// <param name="methodBody">The method of the method to be optimized.</param>
        public OptimizationWorker(Optimizer optimizer, MethodBody methodBody)
        {
            this.optimizer = optimizer;
            this.methodBody = methodBody;
        }


        /// <summary>
        /// Gets or sets a value indicating whether an rewriting has been performed.
        /// </summary>
        /// <value><see langword="true" /> if a rewriting has been performed; otherwise, <see langword="false"/>.</value>
        public bool WasOptimized
        {
            get;
            set;
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
        /// Gets the number of local variables used in the method.
        /// </summary>
        /// <value>The number of local variables used in the method.</value>
        public int LocalVariableCount
        {
            get
            {
                return this.methodBody.Variables.Count;
            }
        }

        /// <summary>
        /// Gets the branch sources.
        /// </summary>
        /// <value>The branch sources.</value>
        private BranchManager Branches
        {
            get
            {
                if (this.branches == null)
                {
                    this.FindBranches();
                }

                return this.branches;
            }
        }

        /// <summary>
        /// Adds a local variable to the current method.
        /// </summary>
        /// <param name="definition">The definition of the new local variable.</param>
        public void AddLocalVariable(VariableDefinition definition)
        {
            var variables = this.methodBody.Variables;
            variables.Add(definition);
        }

        /// <summary>
        /// Copies an instruction from another instruction.
        /// </summary>
        /// <param name="instruction">The instruction to copy.</param>
        /// <returns>A copy of <paramref name="instruction"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "False positive")]
        public Instruction CopyInstruction(Instruction instruction)
        {
            var opCode = instruction.OpCode;
            var operand = instruction.Operand;
            switch (opCode.OperandType)
            {
                case OperandType.InlineNone:
                    return this.CilWorker.Create(opCode);

                case OperandType.InlineType:
                    return this.CilWorker.Create(opCode, (TypeReference) operand);

                case OperandType.InlineMethod:
                    return this.CilWorker.Create(opCode, (MethodReference) operand);

                case OperandType.InlineField:
                    return this.CilWorker.Create(opCode, (FieldReference) operand);

                case OperandType.InlineParam:
                case OperandType.ShortInlineParam:
                    return this.CilWorker.Create(opCode, (ParameterDefinition) operand);
                
                case OperandType.InlineVar:
                case OperandType.ShortInlineVar:
                    return this.CilWorker.Create(opCode, (VariableDefinition) operand);

                case OperandType.InlineTok:
                    var typeReference = operand as TypeReference;
                    if (typeReference != null)
                    {
                        return this.CilWorker.Create(opCode, typeReference);
                    }

                    var methodReference = operand as MethodReference;
                    if (methodReference != null)
                    {
                        return this.CilWorker.Create(opCode, methodReference);
                    }

                    return this.CilWorker.Create(opCode, (FieldReference)operand);

                case OperandType.InlineBrTarget:
                case OperandType.ShortInlineBrTarget:
                    return this.CilWorker.Create(opCode, (Instruction)operand);

                case OperandType.InlineSwitch:
                    return this.CilWorker.Create(opCode, (Instruction[])operand);

                case OperandType.ShortInlineI:
                    return this.CilWorker.Create(opCode, (sbyte) operand);
                
                case OperandType.InlineI:
                    return this.CilWorker.Create(opCode, (int) operand);

                case OperandType.InlineI8:
                    return this.CilWorker.Create(opCode, (long) operand);

                case OperandType.ShortInlineR:
                    return this.CilWorker.Create(opCode, (float)operand);

                case OperandType.InlineR:
                    return this.CilWorker.Create(opCode, (double) operand);

                case OperandType.InlineString:
                    return this.CilWorker.Create(opCode, (string)operand);

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Deletes an instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <exception cref="ArgumentNullException"><c>instruction</c> is null.</exception>
        /// <remarks>
        /// Any branch or conditional branch instructions targeting <paramref name="instruction" /> will be modified
        /// to target the instruction following it.
        /// </remarks>
        public void DeleteInstruction(Instruction instruction)
        {
            this.WasOptimized = true;

            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }

            if (instruction.OpCode.FlowControl == FlowControl.Branch ||
                instruction.OpCode.FlowControl == FlowControl.Cond_Branch)
            {
                this.Branches.Remove(instruction);
            }

            var followingInstruction = instruction.Next;
            if (followingInstruction == null)
            {
                // TODO: convert branches to ret?
            }

            this.Branches.Retarget(instruction, followingInstruction);

            this.CilWorker.Remove(instruction);
        }

        /// <summary>
        /// Replaces the given instruction with another instruction.
        /// </summary>
        /// <param name="instruction">The instruction to replace</param>
        /// <param name="replacement">The replacement.</param>
        /// <exception cref="ArgumentNullException"><c>instruction</c> or <c>replacement</c> are null.</exception>
        /// <remarks>
        /// Any branch or conditional branch instructions targeting <paramref name="instruction" /> will be modified
        /// to target <c>replacement</c>.
        /// </remarks>
        public void ReplaceInstruction(Instruction instruction, Instruction replacement)
        {
            this.WasOptimized = true;

            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }

            if (instruction.OpCode.FlowControl == FlowControl.Branch ||
                instruction.OpCode.FlowControl == FlowControl.Cond_Branch)
            {
                this.Branches.Remove(instruction);
            }

            this.Branches.Retarget(instruction, replacement);

            if (replacement.OpCode.FlowControl == FlowControl.Branch ||
                replacement.OpCode.FlowControl == FlowControl.Cond_Branch)
            {
                this.branches.Add(replacement);
            }

            this.CilWorker.Replace(instruction, replacement);
        }

        /// <summary>
        /// Inserts an instruction into the IL stream, following the target instruction and any other instructions added.
        /// </summary>
        /// <param name="target">The target to insert before.</param>
        /// <param name="instruction">The instruction to insert.</param>
        public void InsertBefore(Instruction target, Instruction instruction)
        {
            this.WasOptimized = true;

            // if we are adding a branch instruction then update our cache.
            var opCode = instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Branch || opCode.FlowControl == FlowControl.Cond_Branch)
            {
                this.Branches.Add(instruction);
            }

            if (target == null)
            {
                // insert to the end of the stream.
                this.CilWorker.Append(instruction);
            }
            else
            {
                this.CilWorker.InsertBefore(target, instruction);
            }
        }

        /// <summary>
        /// Modifies any branch instruction in the method which targets a particular instruction to
        /// target another instruction instead.
        /// </summary>
        /// <param name="oldTarget">The old target.</param>
        /// <param name="newTarget">The new target.</param>
        public void RetargetBranches(Instruction oldTarget, Instruction newTarget)
        {
            this.Branches.Retarget(oldTarget, newTarget);
        }

        /// <summary>
        /// Returns a list of all the instruction in the method that may move execution to the specified instruction.
        /// </summary>
        /// <param name="target">The target instruction.</param>
        /// <returns>The set of instructions that may transfer execution </returns>
        public ICollection<Instruction> SourceInstructions(Instruction target)
        {
            var sources = new List<Instruction>();

            var previous = target.Previous;
            if (previous != null)
            {
                var previousOpCode = previous.OpCode;
                var previousFlowControl = previousOpCode.FlowControl;

                // NOTE: if previous is a (conditional) branch to current this will be handled with branches
                if (previousFlowControl != FlowControl.Branch &&
                    previousFlowControl != FlowControl.Return &&
                    previousFlowControl != FlowControl.Throw &&
                    (previousFlowControl != FlowControl.Cond_Branch || previous.Operand != target))
                {
                    sources.Add(previous);
                }
            }

            sources.AddRange(this.Branches.FindBranches(target));
            return sources;
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
        /// Initializes the branch sources map.
        /// </summary>
        private void FindBranches()
        {
            this.branches = new BranchManager();
            
            var instructions = this.methodBody.Instructions;
            var instructionCount = instructions.Count;
            for (var i = 0; i < instructionCount; i++)
            {
                var instruction = instructions[i];
                var opCode = instruction.OpCode;
                if (opCode.FlowControl == FlowControl.Branch || opCode.FlowControl == FlowControl.Cond_Branch)
                {
                    this.branches.Add(instruction);
                }
            }
        }
    }
}