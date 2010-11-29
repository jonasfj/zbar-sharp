using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ZBar
{
	public class SymbolSet
	{
		private List<Symbol> symbols_ = new List<Symbol>();

		public unsafe SymbolSet(zbar_symbol_set_t * set)
		{
			if(set == null)
				throw new ArgumentNullException("set");

			this.Size = set->nsyms;

			var p_symbol = set->head;

			if(p_symbol != IntPtr.Zero)
			{
				do
				{
					this.symbols_.Add(new Symbol(p_symbol));				

					p_symbol = Symbol.zbar_symbol_next(p_symbol);
				}while(p_symbol != IntPtr.Zero);
			}
		}

		public ReadOnlyCollection<Symbol> Symbols
		{
			get
			{
				if(this.symbols_ == null)
					return null;

				return this.symbols_.AsReadOnly();
			}
		}

		public long Size
		{
			get;
			set;
		}
		
		#region Native Types
		
		public unsafe struct zbar_symbol_set_t
		{
			public long refcnt;				// refcnt_t; Reference count.
			public int nsyms;				  // Count of symbols in the set.
			public IntPtr head;
			public IntPtr tail;
		}
		
		#endregion
	}
}
