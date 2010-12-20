/*------------------------------------------------------------------------
 *  Copyright 2010 (c) Jonas Finnemann Jensen <jopsen@gmail.com>
 * 
 *  This file is part of the ZBar CIL Wrapper.
 *
 *  The ZBar CIL Wrapper is free software; you can redistribute it
 *  and/or modify it under the terms of the GNU Lesser Public License as
 *  published by the Free Software Foundation; either version 2.1 of
 *  the License, or (at your option) any later version.
 *
 *  The ZBar CIL Wrapper is distributed in the hope that it will be
 *  useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 *  of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser Public License
 *  along with the ZBar CIL Wrapper; if not, write to the Free
 *  Software Foundation, Inc., 51 Franklin St, Fifth Floor,
 *  Boston, MA  02110-1301  USA
 * 
 *------------------------------------------------------------------------*/
using System;
using NUnit.Framework;
using ZBar;

namespace tests
{
	/// <summary>
	/// Tests to see if Image.FourCC works
	/// </summary>
	[TestFixture()]
	public class FourCC
	{		
		[Test()]
		public void TestRGB3(){
			Assert.AreEqual(0x33424752, Image.FourCC('R', 'G', 'B', '3'), "FourCC code are not computed correctly!");
		}
		[Test()]
		public void TestY800(){
			Assert.AreEqual(0x30303859, Image.FourCC('Y', '8', '0', '0'), "FourCC code are not computed correctly!");
		}
	}
}