﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Diagnostics;

namespace NPhp.Codegen
{
	public partial class SafeILGenerator
	{
		private ILGenerator ILGenerator;
		TypeStackClass TypeStack;
		List<SafeLabel> Labels = new List<SafeLabel>();
		bool OverflowCheck = false;
		bool DoEmit = true;
		bool TrackStack = true;
		public bool DoDebug { get; private set; }

		public SafeILGenerator(ILGenerator ILGenerator, bool DoDebug)
		{
			this.ILGenerator = ILGenerator;
			this.TypeStack = new TypeStackClass(this);
			this.DoDebug = DoDebug;
		}

		public TypeStackClass GetCurrentTypeStack()
		{
			return TypeStack;
		}

		public enum UnaryOperatorEnum
		{
			Negate,
			Not
		}

		public enum PointerAttributesSet
		{
			Unaligned = 1,
			Volatile = 2,
		}

		public enum BinaryOperatorEnum
		{
			AdditionSigned,
			AdditionUnsigned,
			And,
			DivideSigned,
			DivideUnsigned,
			MultiplySigned,
			MultiplyUnsigned,
			Or,
			RemainingSigned,
			RemainingUnsigned,
			ShiftLeft,
			ShiftRightSigned,
			ShiftRightUnsigned,
			SubstractionSigned,
			SubstractionUnsigned,
			Xor,
		}

		public enum BinaryComparison2Enum
		{
			Equals,
			GreaterThanSigned,
			GreaterThanUnsigned,
			LessThanSigned,
			LessThanUnsigned,
		}

		public enum BinaryComparisonEnum
		{
			Equals,
			NotEquals,
			GreaterOrEqualSigned,
			GreaterOrEqualUnsigned,
			GreaterThanSigned,
			GreaterThanUnsigned,
			LessOrEqualSigned,
			LessOrEqualUnsigned,
			LessThanSigned,
			LessThanUnsigned,
		}

		public enum UnaryComparisonEnum
		{
			False,
			True,
		}

		public class TypeStackClass
		{
			//public List<Type> List;
			private LinkedList<Type> Stack = new LinkedList<Type>();
			private SafeILGenerator SafeILGenerator;

			internal TypeStackClass(SafeILGenerator SafeILGenerator)
			{
				this.SafeILGenerator = SafeILGenerator;
			}

			public int Count
			{
				get
				{
					return Stack.Count;
				}
			}

			public void Pop(int Count)
			{
				while (Count-- > 0) Pop();
			}

			public Type Pop()
			{
				if (SafeILGenerator.DoDebug)
				{
					Debug.WriteLine(String.Format("## TypeStackClass.Pop: {0}", Stack.First.Value.Name));
				}
				try
				{
					return Stack.First.Value;
				}
				finally
				{
					Stack.RemoveFirst();
				}
			}

			public void Push(Type Type)
			{
				if (SafeILGenerator.DoDebug)
				{
					Debug.WriteLine(String.Format("## TypeStackClass.Push: {0}", Type.Name));
				}
				Stack.AddFirst(Type);
			}

			public TypeStackClass Clone()
			{
				var NewTypeStack = new TypeStackClass(SafeILGenerator);
				NewTypeStack.Stack = new LinkedList<Type>(Stack);
				return NewTypeStack;
			}

			public Type GetLastest()
			{
				return Stack.First.Value;
			}

			public Type[] GetLastestList(int Count)
			{
				return Stack.Take(Count).Reverse().ToArray();
			}
		}

		public TypeStackClass CaptureStackInformation(Action Action)
		{
			var OldTypeStack = TypeStack;
			var NewTypeStack = TypeStack.Clone();
			var OldDoEmit = DoEmit;
			TypeStack = NewTypeStack;
			DoEmit = false;
			try
			{
				Action();
				return NewTypeStack;
			}
			finally
			{
				DoEmit = OldDoEmit;
				TypeStack = OldTypeStack;
			}
		}

		public class SafeLabel
		{
			private SafeILGenerator SafeILGenerator;
			internal System.Reflection.Emit.Label ReflectionLabel { get; private set; }
			public bool Marked { get; private set; }
			public string Name;

			internal SafeLabel(SafeILGenerator SafeILGenerator, string Name)
			{
				this.SafeILGenerator = SafeILGenerator;
				this.ReflectionLabel = SafeILGenerator.ILGenerator.DefineLabel();
				this.Name = Name;
			}

			public void Mark()
			{
				SafeILGenerator.ILGenerator.MarkLabel(ReflectionLabel);
				Marked = true;
			}

			public override string ToString()
			{
				return String.Format("Label({0})", Name);
			}
		}

		public SafeLabel CreateLabel(string Name)
		{
			var Label = new SafeLabel(this, Name);
			Labels.Add(Label);
			return Label;
		}

		public void DoOverflowCheck(Action Action)
		{
			var OldOverflowCheck = OverflowCheck;
			OverflowCheck = true;
			try
			{
				Action();
			}
			finally
			{
				OverflowCheck = OldOverflowCheck;
			}
		}


		public void PendingOpcodes()
		{
			//OpCodes.Endfilter;
			//OpCodes.Endfinally;
			//OpCodes.Initblk;
			//OpCodes.Initobj;
			//OpCodes.Isinst;
			//OpCodes.Newarr;
			//OpCodes.Newobj;
			//OpCodes.Prefix1;
			//OpCodes.Prefix2;
			//OpCodes.Prefix3;
			//OpCodes.Prefix4;
			//OpCodes.Prefix5;
			//OpCodes.Prefix6;
			//OpCodes.Prefix7;
			//OpCodes.Prefixref;
			//OpCodes.Readonly
			//OpCodes.Refanytype
			//OpCodes.Refanyval
			//OpCodes.Sizeof
			//OpCodes.Unaligned
			//OpCodes.Unbox
			//OpCodes.Volatile
			//OpCodes.Stobj
			//OpCodes.Stfld
			//OpCodes.Stind_Ref
			//OpCodes.Stelem_Ref
			//OpCodes.Ldobj;
			//OpCodes.Ldsfld;
			//OpCodes.Ldsflda;
			//OpCodes.Ldtoken;
			//OpCodes.Ldvirtftn;
			//OpCodes.Leave;
			//OpCodes.Leave_S;
			//OpCodes.Localloc;
			//OpCodes.Mkrefany;
			//OpCodes.Ldarga;
			//OpCodes.Ldarga_S;
			//OpCodes.
			throw (new NotImplementedException());
		}

		public LocalBuilder DeclareLocal<TType>()
		{
			return ILGenerator.DeclareLocal(typeof(TType));
		}

		public void CheckAndFinalize()
		{
			foreach (var Label in Labels)
			{
				if (!Label.Marked) throw(new InvalidOperationException("Label '" + Label + "' not marked"));
			}
			ResetStack();
		}

		public void Comment(string Comment)
		{
		}

		public int StackCount
		{
			get
			{
				return TypeStack.Count;
			}
		}
	}
}
