namespace Game;
using System.Collections;

public class ReallySortedList<T> : IEnumerable
{

    private class Node
    {
        public T thing;
        public Node next;
        public int metric;
        public Node(int metric, T thing)
        {
            this.metric = metric;
            this.thing = thing;
        }
    }

    private class Enumerator : IEnumerator
    {
        private Node head;
        public Node node;
        public Enumerator(Node head)
        {
            this.head = head;
            node = new(int.MaxValue, default);
            node.next = head;
        }

        public bool MoveNext()
        {
            node = node.next;
            return node is not null;
        }

        public void Reset()
        {
            node = new(int.MaxValue, default);
            node.next = head;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public T Current
        {
            get
            {
                return node.thing;
            }
        }
    }

    private Node head;
    public int Count { get; private set; } = 0;
    public void Add(int metric, T thing)
    {
        Count++;

        if (head is null)
        {
            head = new(metric, thing);
            return;
        }

        if (metric < head.metric)
        {
            Node n = new(metric, thing);
            n.next = head;
            head = n;
            return;
        }
        
        Node lagptr = null;
        Node fptr = head;
        while (fptr is not null)
        {
            if (metric < fptr.metric)
            {
                lagptr.next = new(metric, thing);
                lagptr.next.next = fptr;
                return;
            }

            if (fptr.next is null)
            {
                fptr.next = new(metric, thing);
                return;
            }

            lagptr = fptr;
            fptr = fptr.next;
        }

        lagptr.next = new(metric, thing);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return new Enumerator(head);
    }

}
