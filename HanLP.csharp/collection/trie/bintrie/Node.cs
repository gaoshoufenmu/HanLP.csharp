using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanLP.csharp.interfaces;
using HanLP.csharp.utility;

namespace HanLP.csharp.collection.trie.bintrie
{
    public class Node<V> : BaseNode<V>
    {
        public Node() { }
        public Node(char c, Status status, V v)
        {
            this.c = c;
            this.status = status;
            this.v = v;
        }
        public override BaseNode<V> GetChild(char c)
        {
            if (child == null) return null;
            int index = ArrayHelper.BinarySearchByHashcode(child, c);
            if (index < 0) return null;
            return child[index];
        }

        public override bool AddChild(BaseNode<V> node)
        {
            var add = false;
            if (child == null)
                child = new BaseNode<V>[0];

            int index = ArrayHelper.BinarySearch(child, node);
            if(index >= 0)      // 如果已经存在，则分情况做修改
            {
                var target = child[index];  // 已经存在的目标对象
                switch(node.Status)
                {
                    case Status.UNDEFINED:      // 这其实是一种软删除方式
                        if(target.Status != Status.NOT_WORD)
                        {
                            target.Status = Status.NOT_WORD;
                            target.Value = default(V);
                            add = true;
                        }
                        break;
                    case Status.NOT_WORD:   // 要添加的节点不是词语结尾，那么已存在的节点如果是词语结尾，则修改状态为词语中间，并且还可以继续
                        if (target.Status == Status.WORD_END)
                            target.Status = Status.WORD_MIDDLE;
                        break;
                    case Status.WORD_END:
                        if (target.Status != Status.WORD_END)
                            target.Status = Status.WORD_MIDDLE;

                        if (target.Value == null)
                            add = true;
                        target.Value = node.Value;
                        break;
                }
            }
            else
            {
                var newChild = new BaseNode<V>[child.Length + 1];
                var insertIdx = -index - 1;
                Array.Copy(child, 0, newChild, 0, insertIdx);
                newChild[insertIdx] = node;
                Array.Copy(child, insertIdx, newChild, insertIdx + 1, child.Length - insertIdx);
                child = newChild;
                add = true;
            }
            return add;
        }
    }
}
