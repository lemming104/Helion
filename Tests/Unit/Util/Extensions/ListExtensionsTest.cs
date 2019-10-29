using System.Collections.Generic;
using System.Linq;
using Helion.Util.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Helion.Test.Unit.Util.Extensions
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void CheckIfEmpty()
        {
            Assert.IsTrue(new List<int>().Empty());
        }

        [TestMethod]
        public void CheckIfNotEmpty()
        {
            Assert.IsFalse(new List<int> { 1 }.Empty());
        }

        [TestMethod]
        public void CanCopyListWithoutElements()
        {
            List<int> list = new List<int>();
            IList<int> copy = list.Copy();

            Assert.AreNotSame(list, copy);
            Assert.IsTrue(copy.Empty());
        }

        [TestMethod]
        public void CanCopyListWithElements()
        {
            List<int> list = new List<int> { 0, 1, 2 };
            IList<int> copy = list.Copy();

            Assert.AreNotSame(list, copy);
            Assert.AreEqual(list.Count, copy.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(i, copy[i]);
        }

        [TestMethod]
        public void IterateInReverse()
        {
            List<int> list = new List<int> { 0, 1, 2 };
            List<int> seen = new List<int>();
            
            list.ForEachReverse(Func);

            Assert.AreEqual(seen.Count, list.Count);
            for (int i = 0; i < seen.Count; i++)
                Assert.AreEqual(list[list.Count - i - 1], seen[i]);
            
            void Func(int value)
            {
                seen.Add(value);
            }
        }

        [TestMethod]
        public void GeneratePairCombinations()
        {
            List<int> list = new List<int> { 0, 1, 2, 3 };
            List<(int, int)> expected = new List<(int, int)> { (0, 1), (0, 2), (0, 3), (1, 2), (1, 3), (2, 3) };
            Assert.IsTrue(expected.SequenceEqual(list.PairCombinations()));
            
            List<(int, int)> emptyExpected = new List<(int, int)>();

            List<int> emptyList = new List<int>();
            List<int> oneElementList = new List<int> { 0 };
            Assert.IsTrue(emptyExpected.SequenceEqual(oneElementList.PairCombinations()));
            Assert.IsTrue(emptyExpected.SequenceEqual(emptyList.PairCombinations()));
        }
    }
}