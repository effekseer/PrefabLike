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
        public void Diff1()
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

        [Test]
        public void Instantiate1()
        {
            var system = new PrefabSyatem();
            var prefab = new EditorNodeInformation();

            // 差分から prefab を作る
            {
                prefab.BaseType = typeof(TestNode1);

                var v = new TestNode1();

                var before = new FieldState();
                before.Store(v);

                v.Value1 = 5;

                var after = new FieldState();
                after.Store(v);

                prefab.Modified.Difference = after.GenerateDifference(before);
            }

            // インスタンスを作ってみる
            var node2 = system.CreateNodeFromPrefab(prefab) as TestNode1;
            Assert.AreEqual(5, node2.Value1);   // prefab が持っている値が設定されていること
        }
    }
}