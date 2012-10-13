using System;
using System.Collections.Generic;
using System.IO;

namespace CLanguage
{
	public class CompiledFunction : BaseFunction
	{
		public List<VariableDeclaration> LocalVariables { get; private set; }
		public List<Instruction> Instructions { get; private set; }

		public CompiledFunction (string name, CFunctionType functionType)
		{
			Name = name;
			FunctionType = functionType;
			LocalVariables = new List<VariableDeclaration> ();
			Instructions = new List<Instruction> ();
		}

		public override string ToString ()
		{
			return Name;
		}

		public string Assembler {
			get {
				var w = new StringWriter ();
				for (var i = 0; i < Instructions.Count; i++) {
					w.WriteLine ("{0}: {1}", i, Instructions[i]);
				}
				return w.ToString ();
			}
		}

		public override void Init (ExecutionState state)
		{
			state.ActiveFrame.AllocateLocals (LocalVariables.Count);
		}

		public override void Step (ExecutionState state)
		{
			var frame = state.ActiveFrame;
			var ip = frame.IP;
			var locals = frame.Locals;
			var stackSize = state.Stack.Length;

			var done = false;

			var a = 0;
			var b = 0;

			while (!done && ip < Instructions.Count && state.RemainingTime > 0) {

				var i = Instructions[ip];

				switch (i.Op) {
				case OpCode.Dup:
					state.Stack[state.SP] = state.Stack[state.SP - 1];
					state.SP++;
					ip++;
					break;
				case OpCode.Pop:
					state.SP--;
					ip++;
					break;
				case OpCode.Call:
					a = state.Stack[state.SP - 1];
					state.SP--;
					ip++;
					state.Call (a);
					done = true;
					break;
				case OpCode.Return:
					state.Return ();
					done = true;
					break;
				case OpCode.LoadFunction:
					state.Stack[state.SP] = i.X;
					state.SP++;
					ip++;
					break;
				case OpCode.LoadValue:
					state.Stack[state.SP] = i.X;
					state.SP++;
					ip++;
					break;
				case OpCode.LoadLocal:
					state.Stack[state.SP] = locals[i.X];
					state.SP++;
					ip++;
					break;
				case OpCode.StoreLocal:
					locals[i.X] = state.Stack[state.SP - 1];
					state.SP--;
					ip++;
					break;
				case OpCode.AddInt16:
					a = state.Stack[state.SP - 2];
					b = state.Stack[state.SP - 1];
					state.Stack[state.SP - 2] = ((short)a + (short)b);
					state.SP--;
					ip++;
					break;
				default:
					throw new NotImplementedException (i.Op.ToString ());
				}

				state.RemainingTime -= state.CpuSpeed;
			}

			frame.IP = ip;

			if (ip >= Instructions.Count) {
				throw new ExecutionException ("Function '" + Name + "' never returned.");
			}
		}
	}
}
