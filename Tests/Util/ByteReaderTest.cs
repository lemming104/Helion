﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using ByteReader = Helion.Util.ByteReader;

namespace Helion.Test.Util.Container
{
    [TestClass]
    public class ByteReaderTest
    {
        [TestMethod]
        public void ReadFromByteArray()
        {
            byte[] data = { 1, 2, 3 };
            ByteReader reader = new ByteReader(data);

            Assert.AreEqual(1, reader.ReadByte());
            Assert.AreEqual(2, reader.ReadByte());
            Assert.AreEqual(3, reader.ReadByte());

            Assert.AreEqual(data.Length, reader.Length);
        }

        [TestMethod]
        public void ReadFromMemoryStream()
        {
            byte[] data = { 1, 2, 3 };
            ByteReader reader = new ByteReader(new BinaryReader(new MemoryStream(data)));

            Assert.AreEqual(1, reader.ReadByte());
            Assert.AreEqual(2, reader.ReadByte());
            Assert.AreEqual(3, reader.ReadByte());

            Assert.AreEqual(data.Length, reader.Length);
        }

        [TestMethod]
        public void ReadEightByteString()
        {
            byte[] data = { (byte)'a', (byte)'b', (byte)'c', (byte)'D', (byte)'E', (byte)'f', 0, 0 };
            ByteReader reader = new ByteReader(data);

            Assert.AreEqual("abcDEf", reader.ReadEightByteString());
        }

        [TestMethod]
        public void ReadBasedOnStringLength()
        {
            byte[] data = { (byte)'a', (byte)'b', (byte)'c', (byte)'D', (byte)'E', (byte)'f', 0, 0 };
            ByteReader reader = new ByteReader(data);

            Assert.AreEqual("abc", reader.ReadStringLength(3));
            Assert.AreEqual("DEf\0", reader.ReadStringLength(4));
        }

        [TestMethod]
        public void ReadBigEndian32BitInt()
        {
            byte[] data = { 1, 2, 3, 4 };
            ByteReader reader = new ByteReader(data);

            Assert.AreEqual(0x01020304, reader.ReadInt32BE());
        }

        [TestMethod]
        public void CheckIfBytesRemaining()
        {
            byte[] data = { 1, 2, 3 };
            ByteReader reader = new ByteReader(data);

            Assert.IsTrue(reader.HasBytesRemaining(3));
            Assert.IsFalse(reader.HasBytesRemaining(4));

            reader.ReadByte();

            Assert.IsTrue(reader.HasBytesRemaining(2));
            Assert.IsFalse(reader.HasBytesRemaining(3));

            reader.Advance(2);

            Assert.IsFalse(reader.HasBytesRemaining(1));
        }

        [TestMethod]
        public void CanAdvanceStreamPosition()
        {
            byte[] data = { 1, 2, 3 };
            ByteReader reader = new ByteReader(data);

            reader.Advance(1);
            reader.Advance(1);

            Assert.AreEqual(3, reader.ReadByte());
        }

        [TestMethod]
        public void SetToOffset()
        {
            byte[] data = { 1, 2, 3 };
            ByteReader reader = new ByteReader(data);

            reader.Offset(2);

            Assert.AreEqual(3, reader.ReadByte());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Did not throw exception when reading memory badly")]
        public void ReadingPastEndThrowsException()
        {
            byte[] data = { 1, 2, 3 };
            ByteReader reader = new ByteReader(data);
            reader.ReadInt32BE();
        }
    }
}