﻿//------------------------------------------------------------------------------------------------- 
// <copyright file="InlineFunctionCall.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the InlineFunctionCall optimization.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious.Optimizations
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Inlines function calls.
    /// </summary>
    public class InlineFunctionCall : Optimization
    {
        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly OptimizationConfiguration configuration;

        /// <summary>
        /// A cache of the value indicating whether a method should be inlined.
        /// </summary>
        private readonly Dictionary<MethodDefinition, bool> shouldInline = new Dictionary<MethodDefinition, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineFunctionCall"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public InlineFunctionCall(OptimizationConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="instruction">The instruction to target.</param>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(Instruction instruction, OptimizationWorker worker)
        {
            // TODO: allow arguments
            // TODO: allow return values
            // TODO: allow generic methods
            // TODO: allow non-static methods
            var opCode = instruction.OpCode;
            if (opCode.FlowControl != FlowControl.Call)
            {
                return;
            }

            var methodRef = (MethodReference) instruction.Operand;
            var typeRef = methodRef.DeclaringType;
            var module = typeRef.Module;

            var type = module.Types[typeRef.FullName];
            if (type == null)
            {
                return;
            }

            var method = type.Methods.GetMethod(methodRef.Name, methodRef.Parameters);
            bool shouldInlineMethod;
            if (!this.shouldInline.TryGetValue(method, out shouldInlineMethod))
            {
                shouldInlineMethod = this.configuration.ShouldInline(method);
                this.shouldInline[method] = shouldInlineMethod;
            }

            if (shouldInlineMethod)
            {
                InlineMethod(instruction, worker, method);
            }
        }

        /// <summary>
        /// Inlines the supplied method.
        /// </summary>
        /// <param name="callInstruction">The call instruction.</param>
        /// <param name="worker">The worker to use to modify the caller, positioned at the call instruction.</param>
        /// <param name="method">The method to inline.</param>
        private static void InlineMethod(
            Instruction callInstruction, 
            OptimizationWorker worker, 
            MethodDefinition method)
        {
            worker.Optimize(method);

            // replace the call with a nop in order to preserve branches to this call.
            var nop = worker.CilWorker.Create(OpCodes.Nop);
            worker.ReplaceInstruction(callInstruction, nop);

            var nextInstruction = callInstruction.Next;

            var instructions = method.Body.Instructions;
            var instructionCount = instructions.Count;
            if (instructionCount == 0)
            {
                // TODO: can this happen?
                // do all methods end with a ret instruction?
                return;
            }

            var instruction = instructions[0];
            if (instruction.OpCode.FlowControl == FlowControl.Return)
            {
                return;
            }

            // Create local variables to be used by the inlined function
            var variables = method.Body.Variables;
            for (var i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                worker.AddLocalVariable(variable);
            }

            // TODO: try to avoid this extra dictionary.
            
            // This mapping maps an instruction to the inlined version.  This is required in order to
            // patch up backwards branch instructions to reference an instruction in the current method.
            var copiedInstructions = new Dictionary<Instruction, Instruction>();

            for (var i = 0; i < instructionCount; i++)
            {
                var currentInstruction = instructions[i];
                int location;

                Instruction newInstruction;

                if (currentInstruction.OpCode.FlowControl == FlowControl.Return)
                {
                    // return instructions now just move execution to the instruction after the call
                    
                    // TODO: create best form of br instruction
                    newInstruction = worker.CilWorker.Create(OpCodes.Br, nextInstruction);
                }
                else if (currentInstruction.OpCode.FlowControl == FlowControl.Branch ||
                         currentInstruction.OpCode.FlowControl == FlowControl.Cond_Branch)
                {
                    // if we are jumping to an earlier instruction in the method, then create a jump
                    // to the cloned version.
                    var target = (Instruction) currentInstruction.Operand;
                    
                    Instruction inlinedTarget;
                    if (!copiedInstructions.TryGetValue(target, out inlinedTarget))
                    {
                        inlinedTarget = target;
                    }

                    // TODO: if this is a short branch this may need converting to a long branch
                    // as for example "ret" instructions may have been converted to branches in
                    // the intervening space.
                    newInstruction = worker.CilWorker.Create(currentInstruction.OpCode, inlinedTarget);
                }
                else if (currentInstruction.IsLdloc(out location))
                {
                    var variable = variables[location];

                    // TODO: create the best form of ldloc instruction
                    newInstruction = worker.CilWorker.Create(OpCodes.Ldloc, variable);
                }
                else if (currentInstruction.IsLdloca(out location))
                {
                    var variable = variables[location];

                    // TODO: create the best form of ldloca instruction
                    newInstruction = worker.CilWorker.Create(OpCodes.Ldloca, variable);
                }
                else if (currentInstruction.IsStloc(out location))
                {
                    var variable = variables[location];

                    // TODO: create the best form of ldloca instruction
                    newInstruction = worker.CilWorker.Create(OpCodes.Stloc, variable);
                }
                else
                {
                    newInstruction = worker.CopyInstruction(currentInstruction);
                }

                worker.InsertBefore(nextInstruction, newInstruction);
                
                copiedInstructions.Add(currentInstruction, newInstruction);
                worker.RetargetBranches(currentInstruction, newInstruction);
            }
        }
    }
}