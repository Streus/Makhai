using Makhai.ComplexStats;
using NUnit.Framework;

namespace Makhai.Tests.ComplexStats
{
    public class StatTests
    {
        [Test]
        public void Test_MakeStat()
        {
			Stat s = new Stat ();
			Assert.IsTrue (s.Base == 0);

			float startingValue = 5;
			s = new Stat (startingValue);
			Assert.IsTrue (s.Base == startingValue);
        }

		[Test]
		public void Test_ComplexOperation()
		{
			Stat s = new Stat (1);
			s += 4;
			s *= 5;

			Assert.IsTrue (s.Base == 1);
			Assert.IsTrue (s.Additive == 4);
			Assert.IsTrue (s.Multiplier == 5);
			Assert.IsTrue (s.GetValue () == 9);
		}

		[Test]
		public void Test_ValueEquals()
		{
			Stat s1 = new Stat (1);
			s1 += 4;
			s1 *= 5;

			Stat s2 = new Stat (9);

			Assert.IsTrue (s1 == s2);
		}
    }
}
