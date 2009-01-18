//------------------------------------------------------------------------------------------------- 
// <copyright file="InlineFunctionCallOptimization.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the InlineFunctionCallOptimization type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Inlines function calls.
    /// </summary>
    public class InlineFunctionCallOptimization : Optimization
    {
        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly Configuration configuration;

        /// <summary>
        /// A cache of the value indicating whether a method should be inlined.
        /// </summary>
        private readonly Dictionary<MethodDefinition, bool> shouldInline = new Dictionary<MethodDefinition, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineFunctionCallOptimization"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public InlineFunctionCallOptimization(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Performs the optimization starting at the current instruction.
        /// </summary>
        /// <param name="worker">The worker for optimization actions.</param>
        public override void OptimizeInstruction(OptimizationWorker worker)
        {
            // TODO: allow local variables in inlined method
            // TODO: allow arguments
            // TODO: allow return values
            // TODO: allow generic methods
            // TODO: allow non-static methods
            // TODO: ensure that Br_S instructions before an inlined function are patched up appropriately.
            var instruction = worker.TargetInstruction;
            var opCode = instruction.OpCode;
            if (opCode.FlowControl == FlowControl.Call)
            {
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
                    worker.Optimize(method);

                    worker.DeleteInstruction();

                    var instructions = method.Body.Instructions;
                    var instructionCount = instructions.Count;
                    if (instructionCount == 0)
                    {
                        return;
                    }

                    instruction = instructions[0];
                    if (instruction.OpCode.FlowControl == FlowControl.Return)
                    {
                        return;
                    }

                    worker.InsertInstruction(instruction);
                    for (var i = 1; i < instructionCount; i++)
                    {
                        var currentInstruction = instructions[i];

                        if (currentInstruction.OpCode.FlowControl == FlowControl.Return)
                        {
                            // return instructions now just move execution to the instruction after the call
                            var cilWorker = worker.CilWorker;
                            var branch = cilWorker.Create(OpCodes.Br, worker.NextInstruction);
                            worker.InsertInstruction(branch);
                        }
                        else
                        {
                            worker.InsertInstruction(currentInstruction);
                        }
                    }
                }
            }
        }
    }
}