﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPhp.Common;

namespace NPhp.Runtime
{
	public sealed class Php54Array
	{
		private int LastNumericIndex = 0;
		private List<Php54Var> Keys = new List<Php54Var>();
		private List<Php54Var> Values = new List<Php54Var>();
		private Dictionary<Php54Var, int> KeyIndices = new Dictionary<Php54Var, int>();
		public bool PureArray { get; private set; }

		public Php54Array()
		{
			PureArray = true;
		}

		public IEnumerable<KeyValuePair<Php54Var, Php54Var>> GetEnumerator()
		{
			var KeysEnumerator = Keys.GetEnumerator();
			var ValuesEnumerator = Values.GetEnumerator();
			while (KeysEnumerator.MoveNext() && ValuesEnumerator.MoveNext())
			{
				yield return new KeyValuePair<Php54Var, Php54Var>(KeysEnumerator.Current, ValuesEnumerator.Current);
			}
		}

		private void UpdateLastNumericIndex(Php54Var Key)
		{
			if ((Key.ReferencedType != Php54Var.TypeEnum.Int) && (Key.ReferencedType != Php54Var.TypeEnum.String)) throw (new InvalidOperationException("Keys must be integers or strings only"));

			if ((Key.ReferencedType == Php54Var.TypeEnum.Int) || (Php54Utils.IsCompletelyNumeric(Key.StringValue)))
			{
				var IntKey = Key.IntegerValue;
				if (IntKey > LastNumericIndex)
				{
					LastNumericIndex = IntKey + 1;
					PureArray = false;
				}
				else
				{
					//PureArray = PureArray; // not changed
				}
			}
			else
			{
				PureArray = false;
			}
		}

		public void AddElement(Php54Var Value)
		{
			var KeyInt = LastNumericIndex++;
			var Key = Php54Var.FromInt(KeyInt);
			Keys.Add(Key);
			Values.Add(Value);
			KeyIndices[Key] = Keys.Count - 1;
		}

		public void AddPair(Php54Var Key, Php54Var Value)
		{
			UpdateLastNumericIndex(Key);
			// Add new
			if (!KeyIndices.ContainsKey(Key))
			{
				Keys.Add(Key);
				Values.Add(Value);
				KeyIndices[Key] = Keys.Count - 1;
			}
			// Update
			else
			{
				int Index = KeyIndices[Key];
				Values[Index] = Value;
			}
		}

		public Php54Var GetElementByKey(Php54Var Key)
		{
			return Values[KeyIndices[Key]];
		}

		public int Count
		{
			get
			{
				return Keys.Count;
			}
		}

		public Php54Array Clone()
		{
			var ClonedArray = new Php54Array();
			ClonedArray.LastNumericIndex = this.LastNumericIndex;
			ClonedArray.Keys = new List<Php54Var>(this.Keys);
			ClonedArray.Values = new List<Php54Var>(this.Values);
			ClonedArray.KeyIndices = new Dictionary<Php54Var, int>(this.KeyIndices);
			ClonedArray.PureArray = this.PureArray;

			for (int n = 0; n < this.Count; n++)
			{
				ClonedArray.Values[n] = ClonedArray.Values[n].Clone();
			}

			return ClonedArray;
		}
	}
}
