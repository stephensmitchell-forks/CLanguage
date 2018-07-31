﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLanguage.Types;

using CLanguage.Interpreter;

namespace CLanguage.Syntax
{
    public class VariableExpression : Expression
    {
        public string VariableName { get; private set; }

        public VariableExpression(string val)
        {
            VariableName = val;
        }

		public override CType GetEvaluatedCType (EmitContext ec)
		{
			var v = ec.ResolveVariable (VariableName, null);
			if (v != null) {
				return v.VariableType;
			}
			else {
				return CBasicType.SignedInt;
			}
        }

        protected override void DoEmit(EmitContext ec)
        {
			var variable = ec.ResolveVariable (VariableName, null);

			if (variable != null) {

				if (variable.Scope == VariableScope.Function) {
                    ec.Emit (OpCode.LoadConstant, Value.FunctionPointer (variable.Index));
				}
				else {
                    if (variable.VariableType is CBasicType ||
                        variable.VariableType is CPointerType) {

                        if (variable.Scope == VariableScope.Arg) {
                            ec.Emit (OpCode.LoadArg, variable.Index);
                        }
                        else if (variable.Scope == VariableScope.Global) {
                            ec.Emit (OpCode.LoadGlobal, variable.Index);
                        }
                        else if (variable.Scope == VariableScope.Local) {
                            ec.Emit (OpCode.LoadLocal, variable.Index);
                        }
                        else {
                            throw new NotSupportedException ("Cannot evaluate variable scope '" + variable.Scope + "'");
                        }
                    }
                    else if (variable.VariableType is CArrayType arrayType) {

                        if (variable.Scope == VariableScope.Arg) {
                            ec.Emit (OpCode.LoadConstant, Value.ArgPointer (variable.Index));
                        }
                        else if (variable.Scope == VariableScope.Global) {
                            ec.Emit (OpCode.LoadConstant, Value.GlobalPointer (variable.Index));
                        }
                        else if (variable.Scope == VariableScope.Local) {
                            ec.Emit (OpCode.LoadConstant, Value.LocalPointer (variable.Index));
                        }
                        else {
                            throw new NotSupportedException ("Cannot evaluate array variable scope '" + variable.Scope + "'");
                        }
                    }
					else {
						throw new NotSupportedException ("Cannot evaluate variable type '" + variable.VariableType + "'");
					}
				}
			}
			else {
                ec.Report.Error (103, $"The name `{VariableName}` does not exist in the current context.");
				ec.Emit (OpCode.LoadConstant, 0);
			}
        }

        protected override void DoEmitPointer (EmitContext ec)
        {
            var res = ec.ResolveVariable (VariableName, null);

            if (res != null) {
                res.Emit (ec);
            }
            else {
                ec.Report.Error (103, $"The name `{VariableName}` does not exist in the current context.");
                ec.Emit (OpCode.LoadConstant, 0);
            }
        }

        public override string ToString()
        {
            return VariableName.ToString();
        }
    }
}
