using System.Linq;
using NUnit.Framework;
using PrefabLike;

namespace PrefabLikeTest
{
    public class Tests
    {
        // プリミティブな型をフィールドに持つテストクラス
        class TestNode1 : Node
        {
            public int Value1;
            public float Value2;
            public string Value3;
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void DiffTest1()
        {
            var v = new TestNode1();

            var before = new FieldState();
            before.Store(v);    // ここでオブジェクトのスナップショットが取られる

            v.Value1 = 5;

            var after = new FieldState();
            after.Store(v);    // ここでオブジェクトのスナップショットが取られる

            // after -> before への差分
            {
                var diff = after.GenerateDifference(before);
                var pair = diff.First();
                var group = pair.Key;
                var field = group.Keys[0] as AccessKeyField;
                Assert.AreEqual(1, diff.Count);         // 余計な差分が出ていないこと
                Assert.AreEqual("Value1", field.Name);  // Value1 が差分として出ている
                Assert.AreEqual(5, pair.Value);         // after 側の値として 5 が検出される
            }

            // before -> after への差分
            {
                var diff = before.GenerateDifference(after);
                var pair = diff.First();
                var group = pair.Key;
                var field = group.Keys[0] as AccessKeyField;
                Assert.AreEqual(1, diff.Count);         // 余計な差分が出ていないこと
                Assert.AreEqual("Value1", field.Name);  // Value1 が差分として出ている
                Assert.AreEqual(0, pair.Value);         // before 側の値として 0 が検出される
            }
        }
    }
}