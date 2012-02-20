﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPhp.Runtime
{
	public sealed class Php54Var
	{
		//private Type Type;
		internal bool IsRef = false;
		private dynamic DynamicValue;

		public enum TypeEnum
		{
			Null,
			Bool,
			Int,
			Double,
			String,
		}

		static private TypeEnum TypeToTypeEnum(Type Type)
		{
			if (Type == typeof(int)) return TypeEnum.Int;
			if (Type == typeof(bool)) return TypeEnum.Bool;
			if (Type == typeof(String)) return TypeEnum.String;
			if (Type == typeof(double)) return TypeEnum.Double;
			if (Type == typeof(DBNull)) return TypeEnum.Null;
			throw(new NotImplementedException());
		}

		static private TypeEnum CombineTypes(TypeEnum Type1, TypeEnum Type2)
		{
			return (TypeEnum)Math.Max((int)Type1, (int)Type2);
		}

		private TypeEnum _Type;

		private TypeEnum Type
		{
			get
			{
				return _Type;
			}
		}

		static public readonly Php54Var Methods = new Php54Var(null, TypeEnum.Null, false);

		public double GetDoubleOrDefault(double DefaultValue)
		{
			if (DynamicValue == null) return DefaultValue;
			return DoubleValue;
		}

		public string GetStringOrDefault(string DefaultValue)
		{
			if (DynamicValue == null) return DefaultValue;
			return StringValue;
		}

		public int GetIntegerOrDefault(int DefaultValue)
		{
			if (DynamicValue == null) return DefaultValue;
			return IntegerValue;
		}

		public bool GetBooleanOrDefault(bool DefaultValue)
		{
			if (DynamicValue == null) return DefaultValue;
			return BoolValue;
		}

		/*
		public dynamic DynamicValue
		{
			get
			{
				return Value;
			}
		}
		*/
		public string StringValue
		{
			get
			{
				if (Type == TypeEnum.Null) return "";
				if (Type == TypeEnum.Bool) return (DynamicValue) ? "1" : "";
				return DynamicValue.ToString();
			}
		}
		public bool BoolValue
		{
			get
			{
				if (Type == TypeEnum.Bool) return DynamicValue;
				return (DynamicValue != 0);
			}
		}
		public double DoubleValue
		{
			get
			{
				if (Type == TypeEnum.Double) return DynamicValue;
				if (Type == TypeEnum.Int) return (double)DynamicValue;
				if (Type == TypeEnum.Null) return 0;
				var Str = StringValue;
				int value = 0;
				for (int n = 0; n < Str.Length; n++)
				{
					if (Str[n] >= '0' && Str[n] <= '9')
					{
						value *= 10;
						value += Str[n] - '0';
					}
					else
					{
						break;
					}
				}
				//return double.Parse(StringValue);
				return value;
			}
		}

		public int IntegerValue
		{
			get
			{
				if (Type == TypeEnum.Int) return DynamicValue;
				if (Type == TypeEnum.Double) return (int)(double)DynamicValue;
				if (Type == TypeEnum.Null) return 0;
				var Str = StringValue;
				int value = 0;
				for (int n = 0; n < Str.Length; n++)
				{
					if (Str[n] >= '0' && Str[n] <= '9')
					{
						value *= 10;
						value += Str[n] - '0';
					}
					else
					{
						break;
					}
				}
				//return double.Parse(StringValue);
				return value;
			}
		}

		public dynamic NumericValue
		{
			get
			{
				if (Type == TypeEnum.Null) return 0;
				if (Type == TypeEnum.Int || Type == TypeEnum.Double) return DynamicValue;
				var Str = StringValue;
				int value = 0;
				for (int n = 0; n < Str.Length; n++)
				{
					if (Str[n] >= '0' && Str[n] <= '9')
					{
						value *= 10;
						value += Str[n] - '0';
					}
					else
					{
						break;
					}
				}
				//return double.Parse(StringValue);
				return value;
			}
		}


		private Php54Var(dynamic Value, TypeEnum Type, bool IsRef = false)
		{
			this.DynamicValue = Value;
			this._Type = Type;
			this.IsRef = IsRef;
			//this.Type = (Value != null) ? Value.GetType() : null;
		}

		static public Php54Var CreateRef(Php54Var ReferencedValue)
		{
			return new Php54Var(ReferencedValue, ReferencedValue.Type, IsRef: true);
		}

		static public Php54Var FromInt(int Value)
		{
			return new Php54Var(Value, TypeEnum.Int);
		}

		static public Php54Var FromString(string Value)
		{
			return new Php54Var(Value, TypeEnum.String);
		}

		static public Php54Var FromTrue()
		{
			return FromBool(true);
		}

		static public Php54Var FromFalse()
		{
			return FromBool(false);
		}

		static public Php54Var FromBool(bool Value)
		{
			return new Php54Var(Value, TypeEnum.Bool);
		}

		static public Php54Var FromObject(object Object)
		{
			return new Php54Var(Object, TypeToTypeEnum(Object.GetType()));
		}

		static public Php54Var FromNull()
		{
			return new Php54Var(null, TypeEnum.Null);
		}

		static public Php54Var BitAnd(Php54Var Left, Php54Var Right)
		{
			return new Php54Var(Left.IntegerValue & Right.IntegerValue, TypeEnum.Int);
		}

		static public Php54Var BitOr(Php54Var Left, Php54Var Right)
		{
			return new Php54Var(Left.IntegerValue | Right.IntegerValue, TypeEnum.Int);
		}

		static public Php54Var UnaryBitNot(Php54Var Left)
		{
			return new Php54Var(~Left.IntegerValue, TypeEnum.Int);
		}

		static public Php54Var UnaryLogicNot(Php54Var Left)
		{
			return new Php54Var(!Left.BoolValue, TypeEnum.Bool);
		}

		static public Php54Var Add(Php54Var Left, Php54Var Right)
		{
			return new Php54Var(Left.NumericValue + Right.NumericValue, CombineTypes(Left.Type, Right.Type));
		}

		static public Php54Var Concat(Php54Var Left, Php54Var Right)
		{
			return new Php54Var(Left.StringValue + Right.StringValue, CombineTypes(Left.Type, Right.Type));
		}

		static public Php54Var Sub(Php54Var Left, Php54Var Right)
		{
			return new Php54Var(Left.DynamicValue - Right.DynamicValue, CombineTypes(Left.Type, Right.Type));
		}

		static public Php54Var Mul(Php54Var Left, Php54Var Right)
		{
			return new Php54Var(Left.DynamicValue * Right.DynamicValue, CombineTypes(Left.Type, Right.Type));
		}

		static public Php54Var Div(Php54Var Left, Php54Var Right)
		{
			return new Php54Var(Left.DynamicValue / Right.DynamicValue, CombineTypes(Left.Type, Right.Type));
		}

		static public Php54Var UnaryPostInc(Php54Var Left, int Count)
		{
			var Old = Left.NumericValue;
			Left.DynamicValue = Old + Count;
			return new Php54Var(Old, Left.Type);
		}

		static public Php54Var UnaryPreInc(Php54Var Left, int Count)
		{
			Left.DynamicValue = Left.NumericValue + Count;
			return new Php54Var(Left.DynamicValue, Left.Type);
		}

		static public Php54Var UnaryAdd(Php54Var Right)
		{
			//return new Php54Var(+Right.DynamicValue, Right.Type);
			return Right;
		}

		static public Php54Var UnarySub(Php54Var Right)
		{
			return new Php54Var(-Right.NumericValue, Right.Type);
		}

		public static bool CompareEquals(Php54Var Left, Php54Var Right)
		{
			return Left.NumericValue == Right.NumericValue;
		}

		public static bool CompareGreaterThan(Php54Var Left, Php54Var Right)
		{
			return Left.NumericValue > Right.NumericValue;
		}

		public static bool CompareLessThan(Php54Var Left, Php54Var Right)
		{
			return Left.NumericValue < Right.NumericValue;
		}

		public static bool CompareLessThan(Php54Var Left, int Right)
		{
			return Left.IntegerValue < Right;
		}

		public static bool CompareNotEquals(Php54Var Left, Php54Var Right)
		{
			return Left.DynamicValue != Right.DynamicValue;
		}

		public static bool LogicalAnd(Php54Var Left, Php54Var Right)
		{
			return Left.BoolValue && Right.BoolValue;
		}

		static public void Assign(Php54Var Left, Php54Var Right)
		{
			if (Left.IsRef)
			{
				Assign(Left.DynamicValue, Right);
				return;
			}

			if (Right.IsRef)
			{
				Left.DynamicValue = Right;
				Left.IsRef = true;
				return;
			}

			Left.DynamicValue = Right.DynamicValue;
			Left._Type = Right.Type;
		}

		public bool ToBool()
		{
			return BoolValue;
		}

		public int ToInt()
		{
			return IntegerValue;
		}

		public override string ToString()
		{
			return StringValue;
		}
	}

}
