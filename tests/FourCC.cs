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