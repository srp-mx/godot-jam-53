using System;
namespace Game.Monads;

public class ErrableList<T>
{
    private class ErrableNode
    {
        public ErrableNode(Errable<T> val) => this.val = val;
        public Errable<T> val;
        public ErrableNode? next;
        public ErrableNode? prev;
    }

    private ErrableNode? head = null;
    private ErrableNode? tail = null;

    public int Count { get; private set; } = 0;
    public Errable<T> GetHead() => head is null ? Errable<T>.Err("[PROBLEM]: Head is null.") : head.val;
    public Errable<T> GetTail() => tail is null ? Errable<T>.Err("[PROBLEM]: Tail is null.") : tail.val;

    public void Append(Errable<T> element)
    {
        Count++;
        if (head is null)
		{
            head = new(element);
            tail = head;
            return;
		}

        for (var node = head; node != null; node = node.next)
        {
            if (node.next is null)
            {
                node.next = new(element);
                node.next.prev = node;
                tail = node.next;
                break;
            }
        }
    }

    public Errable<T> ForAllWithNext(Func<T, T, Errable<T>> Do)
    {
        if (head is null) return Errable<T>.Err("[ERROR][DEV]: Head is null in ForAllWithNext");

        if (Count < 2) return head.val; // Hack to make it valid if nonempty
                                        // but with  Count == 1:
                                        // node.next.next would throw a
                                        // NullReferenceException
        return ForAllWithNext_It(Do, head);
    }

    // NOTE(srp): We can ensure non-null because we only use this method inside ForAllWithNext
    private Errable<T> ForAllWithNext_It(Func<T, T, Errable<T>> Do, ErrableNode lastNode)
    {
        ErrableNode node = lastNode;
        if (node.next.next is null)
            return Errable<T>.ErrableBiMap(
            node.val, node.next.val,
            (curr, nxt) => Do(curr, nxt));

        return Errable<T>.ErrableBiMap(
        node.val, ForAllWithNext_It(Do, node),
        (curr, nxt) => Do(curr, nxt));
    }
}
