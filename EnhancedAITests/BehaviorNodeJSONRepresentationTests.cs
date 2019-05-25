﻿using EnhancedAI.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnhancedAITests
{
    [TestClass]
    public class BehaviorNodeJSONRepresentationTests
    {
        [TestMethod]
        public void SerializeDeserializeRepReserializeTest()
        {
            var root = CoreAI_BT.InitRootNode(null, null, null);

            var repFirst = SerializableBehaviorNode.FromNode(root);
            var jsonFirst = repFirst.ToJSONString();

            var repSecond = SerializableBehaviorNode.FromJSON(jsonFirst);
            var jsonSecond = repSecond.ToJSONString();

            Assert.AreEqual(jsonFirst, jsonSecond);
        }

        [TestMethod]
        public void SerializeDeserializeNodeReserializeTest()
        {
            var root = CoreAI_BT.InitRootNode(null, null, null);

            var repFirst = SerializableBehaviorNode.FromNode(root);
            var jsonFirst = repFirst.ToJSONString();

            var reconstructedRoot = repFirst.ToNode(null, null);

            var repSecond = SerializableBehaviorNode.FromNode(reconstructedRoot);
            var jsonSecond = repSecond.ToJSONString();

            Assert.AreEqual(jsonFirst, jsonSecond);
        }
    }
}
