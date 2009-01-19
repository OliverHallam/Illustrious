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
            // TODO: allow arguments
            // TODO: allow return values
            // TODO: allow generic methods
            // TODO: allow non-static methods
            var instruction = worker.TargetInstruction;
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
                InlineMethod(worker, method);
            }
        }

        /// <summary>
        /// Inlines the supplied method.
        /// </summary>
        /// <param name="worker">The worker to use to modify the caller, positioned at the call instruction.</param>
        /// <param name="method">The method to inline.</param>
        private static void InlineMethod(OptimizationWorker worker, MethodDefinition method)
        {
            // TODO: patch up copied br instructions.
            worker.Optimize(method);
            worker.DeleteInstruction();

            var instructions = method.Body.Instructions;
            var instructionCount = instructions.Count;
            if (instructionCount == 0)
            {
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

            worker.InsertInstruction(instruction);
            for (var i = 1; i < instructionCount; i++)
            {
                var currentInstruction = instructions[i];

                int location;
                if (currentInstruction.OpCode.FlowControl == FlowControl.Return)
                {
                    // return instructions now just move execution to the instruction after the call
                    
                    // TODO: create best form of br instruction
                    var branch = worker.CilWorker.Create(OpCodes.Br, worker.NextInstruction);
                    worker.InsertInstruction(branch);
                }
                else if (currentInstruction.IsLdloc(out location))
                {
                    var variable = variables[location];

                    // TODO: create the best form of ldloc instruction
                    var ldloc = worker.CilWorker.Create(OpCodes.Ldloc, variable);
                    worker.InsertInstruction(ldloc);
                }
                else if (currentInstruction.IsLdloca(out location))
                {
                    var variable = variables[location];

                    // TODO: create the best form of ldloca instruction
                    var ldloca = worker.CilWorker.Create(OpCodes.Ldloca, variable);
                    worker.InsertInstruction(ldloca);
                }
                else if (currentInstruction.IsStloc(out location))
                {
                    var variable = variables[location];

                    // TODO: create the best form of ldloca instruction
                    var stloc = worker.CilWorker.Create(OpCodes.Stloc, variable);
                    worker.InsertInstruction(stloc);
                }
                else
                {
                    worker.InsertInstruction(currentInstruction);
                }
            }
        }
    }
}