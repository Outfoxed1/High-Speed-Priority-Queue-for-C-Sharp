using System;
using System.Collections;
using System.Collections.Generic;

namespace Priority_Queue {
    /// <summary>
    /// A simplified priority queue implementation.  Is stable, auto-resizes, and thread-safe, at the cost of being slightly slower than
    /// FastPriorityQueue
    /// Methods tagged as O(1) or O(log n) are assuming there are no duplicates.  Duplicates may increase the algorithmic complexity.
    /// </summary>
    /// <typeparam name="TItem">The type to enqueue</typeparam>
    /// <typeparam name="TPriority">The priority-type to use for nodes.  Must extend IComparable&lt;TPriority&gt;</typeparam>
    public class SimplePriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority> {
        private class SimpleNode : GenericPriorityQueueNode<TPriority> {
            public TItem Data { get; private set; }
            public SimpleNode(TItem data) { Data = data; }
        }

        private const int INITIAL_QUEUE_SIZE = 10;
        private readonly GenericPriorityQueue<SimpleNode, TPriority> m_queue;
        private readonly Dictionary<TItem, IList<SimpleNode>> m_itemToNodesCache;
        private readonly IList<SimpleNode> m_nullNodesCache;

        #region Constructors
        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        public SimplePriorityQueue() : this(Comparer<TPriority>.Default, EqualityComparer<TItem>.Default) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparer used to compare TPriority values.  Defaults to Comparer&lt;TPriority&gt;.default</param>
        public SimplePriorityQueue(IComparer<TPriority> priorityComparer) : this(priorityComparer.Compare, EqualityComparer<TItem>.Default) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparison function to use to compare TPriority values</param>
        public SimplePriorityQueue(Comparison<TPriority> priorityComparer) : this(priorityComparer, EqualityComparer<TItem>.Default) { }

        /// <summary>
        /// Instantiate a new Priority Queue       
        /// </summary>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimplePriorityQueue(IEqualityComparer<TItem> itemEquality) : this(Comparer<TPriority>.Default, itemEquality) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparer used to compare TPriority values.  Defaults to Comparer&lt;TPriority&gt;.default</param>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimplePriorityQueue(IComparer<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) : this(priorityComparer.Compare, itemEquality) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparison function to use to compare TPriority values</param>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimplePriorityQueue(Comparison<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) {
            m_queue = new GenericPriorityQueue<SimpleNode, TPriority>(INITIAL_QUEUE_SIZE, priorityComparer);
            m_itemToNodesCache = new Dictionary<TItem, IList<SimpleNode>>(itemEquality);
            m_nullNodesCache = new List<SimpleNode>();
        }
        #endregion

        /// <summary>
        /// Given an item of type T, returns the existing SimpleNode in the queue
        /// </summary>
        private SimpleNode GetExistingNode(TItem item) {
            if (item == null) return m_nullNodesCache.Count > 0 ? m_nullNodesCache[0] : null;
            IList<SimpleNode> nodes;
            if (!m_itemToNodesCache.TryGetValue(item, out nodes)) return null;
            return nodes[0];
        }

        /// <summary>
        /// Adds an item to the Node-cache to allow for many methods to be O(1) or O(log n)
        /// </summary>
        private void AddToNodeCache(SimpleNode node) {
            if (node.Data == null) {
                m_nullNodesCache.Add(node);
                return;
            }
            IList<SimpleNode> nodes;
            if (!m_itemToNodesCache.TryGetValue(node.Data, out nodes)) {
                nodes = new List<SimpleNode>();
                m_itemToNodesCache[node.Data] = nodes;
            }
            nodes.Add(node);
        }

        /// <summary>
        /// Removes an item to the Node-cache to allow for many methods to be O(1) or O(log n) (assuming no duplicates)
        /// </summary>
        private void RemoveFromNodeCache(SimpleNode node) {
            if (node.Data == null) {
                m_nullNodesCache.Remove(node);
                return;
            }
            IList<SimpleNode> nodes;
            if (!m_itemToNodesCache.TryGetValue(node.Data, out nodes)) return;
            nodes.Remove(node);
            if (nodes.Count == 0) m_itemToNodesCache.Remove(node.Data);
        }

        /// <summary>
        /// Returns the number of nodes in the queue.
        /// O(1)
        /// </summary>
        public int Count { get { lock (m_queue) return m_queue.Count; } }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// Throws an exception when the queue is empty.
        /// O(1)
        /// </summary>
        public TItem First {
            get {
                lock (m_queue) {
                    if (m_queue.Count <= 0) throw new InvalidOperationException("Cannot call .First on an empty queue");
                    return m_queue.First.Data;
                }
            }
        }

        /// <summary>
        /// Removes every node from the queue.
        /// O(n)
        /// </summary>
        public void Clear() {
            lock (m_queue) {
                m_queue.Clear();
                m_itemToNodesCache.Clear();
                m_nullNodesCache.Clear();
            }
        }

        /// <summary>
        /// Returns whether the given item is in the queue.
        /// O(1)
        /// </summary>
        public bool Contains(TItem item) { lock (m_queue) return item == null ? m_nullNodesCache.Count > 0 : m_itemToNodesCache.ContainsKey(item); }

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
        /// If queue is empty, throws an exception
        /// O(log n)
        /// </summary>
        public TItem Dequeue() {
            lock (m_queue) {
                if (m_queue.Count <= 0) throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
                SimpleNode node = m_queue.Dequeue();
                RemoveFromNodeCache(node);
                return node.Data;
            }
        }

        /// <summary>
        /// Enqueue the item with the given priority, without calling lock(_queue) or AddToNodeCache(node)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private SimpleNode EnqueueNoLockOrCache(TItem item, TPriority priority) {
            SimpleNode node = new SimpleNode(item);
            if (m_queue.Count == m_queue.MaxSize) m_queue.Resize(m_queue.MaxSize * 2 + 1);
            m_queue.Enqueue(node, priority);
            return node;
        }

        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// This queue automatically resizes itself, so there's no concern of the queue becoming 'full'.
        /// Duplicates and null-values are allowed.
        /// O(log n)
        /// </summary>
        public void Enqueue(TItem item, TPriority priority) {
            lock (m_queue) {
                IList<SimpleNode> nodes;
                if (item == null) {
                    nodes = m_nullNodesCache;
                } else if (!m_itemToNodesCache.TryGetValue(item, out nodes)) {
                    nodes = new List<SimpleNode>();
                    m_itemToNodesCache[item] = nodes;
                }
                SimpleNode node = EnqueueNoLockOrCache(item, priority);
                nodes.Add(node);
            }
        }

        /// <summary>
        /// Enqueue a node to the priority queue if it doesn't already exist.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// This queue automatically resizes itself, so there's no concern of the queue becoming 'full'.  Null values are allowed.
        /// Returns true if the node was successfully enqueued; false if it already exists.
        /// O(log n)
        /// </summary>
        public bool EnqueueWithoutDuplicates(TItem item, TPriority priority) {
            lock (m_queue) {
                IList<SimpleNode> nodes;
                if (item == null) {
                    if (m_nullNodesCache.Count > 0) {
                        return false;
                    }
                    nodes = m_nullNodesCache;
                } else if (m_itemToNodesCache.ContainsKey(item)) {
                    return false;
                } else {
                    nodes = new List<SimpleNode>();
                    m_itemToNodesCache[item] = nodes;
                }
                SimpleNode node = EnqueueNoLockOrCache(item, priority);
                nodes.Add(node);
                return true;
            }
        }

        /// <summary>
        /// Removes an item from the queue.  The item does not need to be the head of the queue.  
        /// If the item is not in the queue, an exception is thrown.  If unsure, check Contains() first.
        /// If multiple copies of the item are enqueued, only the first one is removed. 
        /// O(log n)
        /// </summary>
        public void Remove(TItem item) {
            lock (m_queue) {
                SimpleNode removeMe;
                IList<SimpleNode> nodes;
                if (item == null) {
                    if (m_nullNodesCache.Count == 0) throw new InvalidOperationException($"Cannot call Remove() on a node which is not enqueued: {item}");
                    removeMe = m_nullNodesCache[0];
                    nodes = m_nullNodesCache;
                } else {
                    if (!m_itemToNodesCache.TryGetValue(item, out nodes)) throw new InvalidOperationException($"Cannot call Remove() on a node which is not enqueued: {item}");
                    removeMe = nodes[0];
                    if (nodes.Count == 1) m_itemToNodesCache.Remove(item);
                }
                m_queue.Remove(removeMe);
                nodes.Remove(removeMe);
            }
        }

        /// <summary>
        /// Call this method to change the priority of an item.
        /// Calling this method on a item not in the queue will throw an exception.
        /// If the item is enqueued multiple times, only the first one will be updated.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to update all of them, please wrap your items in a wrapper class so they can be distinguished).
        /// O(log n)
        /// </summary>
        public void UpdatePriority(TItem item, TPriority priority) {
            lock (m_queue) {
                SimpleNode updateMe = GetExistingNode(item);
                if (updateMe == null) throw new InvalidOperationException($"Cannot call Remove() on a node which is not enqueued: {item}");
                m_queue.UpdatePriority(updateMe, priority);
            }
        }

        /// <summary>
        /// Returns the priority of the given item.
        /// Calling this method on a item not in the queue will throw an exception.
        /// If the item is enqueued multiple times, only the priority of the first will be returned.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to query all their priorities, please wrap your items in a wrapper class so they can be distinguished).
        /// O(1)
        /// </summary>
        public TPriority GetPriority(TItem item) {
            lock (m_queue) {
                SimpleNode findMe = GetExistingNode(item);
                if (findMe == null) throw new InvalidOperationException($"Cannot call Remove() on a node which is not enqueued: {item}");
                return findMe.Priority;
            }
        }

        #region Try* methods for multithreading
        /// Get the head of the queue, without removing it (use TryDequeue() for that).
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and First
        /// Returns true if successful, false otherwise
        /// O(1)
        public bool TryFirst(out TItem first) {
            if (m_queue.Count > 0) {
                lock (m_queue) {
                    if (m_queue.Count > 0) {
                        first = m_queue.First.Data;
                        return true;
                    }
                }
            }

            first = default(TItem);
            return false;
        }

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and sets it to first.
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and Dequeue()
        /// Returns true if successful; false if queue was empty
        /// O(log n)
        /// </summary>
        public bool TryDequeue(out TItem first) {
            if (m_queue.Count > 0) {
                lock (m_queue) {
                    if (m_queue.Count > 0) {
                        SimpleNode node = m_queue.Dequeue();
                        first = node.Data;
                        RemoveFromNodeCache(node);
                        return true;
                    }
                }
            }

            first = default(TItem);
            return false;
        }

        /// <summary>
        /// Attempts to remove an item from the queue.  The item does not need to be the head of the queue.  
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and Remove()
        /// Returns true if the item was successfully removed, false if it wasn't in the queue.
        /// If multiple copies of the item are enqueued, only the first one is removed. 
        /// O(log n)
        /// </summary>
        public bool TryRemove(TItem item) {
            lock (m_queue) {
                SimpleNode removeMe;
                IList<SimpleNode> nodes;
                if (item == null) {
                    if (m_nullNodesCache.Count == 0) {
                        return false;
                    }
                    removeMe = m_nullNodesCache[0];
                    nodes = m_nullNodesCache;
                } else {
                    if (!m_itemToNodesCache.TryGetValue(item, out nodes)) {
                        return false;
                    }
                    removeMe = nodes[0];
                    if (nodes.Count == 1) {
                        m_itemToNodesCache.Remove(item);
                    }
                }
                m_queue.Remove(removeMe);
                nodes.Remove(removeMe);
                return true;
            }
        }

        /// <summary>
        /// Call this method to change the priority of an item.
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and UpdatePriority()
        /// If the item is enqueued multiple times, only the first one will be updated.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to update all of them, please wrap your items in a wrapper class so they can be distinguished).
        /// Returns true if the item priority was updated, false otherwise.
        /// O(log n)
        /// </summary>
        public bool TryUpdatePriority(TItem item, TPriority priority) {
            lock (m_queue) {
                SimpleNode updateMe = GetExistingNode(item);
                if (updateMe == null) {
                    return false;
                }
                m_queue.UpdatePriority(updateMe, priority);
                return true;
            }
        }

        /// <summary>
        /// Attempt to get the priority of the given item.
        /// Useful for multi-threading, where the queue may become empty between calls to Contains() and GetPriority()
        /// If the item is enqueued multiple times, only the priority of the first will be returned.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to query all their priorities, please wrap your items in a wrapper class so they can be distinguished).
        /// Returns true if the item was found in the queue, false otherwise
        /// O(1)
        /// </summary>
        public bool TryGetPriority(TItem item, out TPriority priority) {
            lock (m_queue) {
                SimpleNode findMe = GetExistingNode(item);
                if (findMe == null) {
                    priority = default(TPriority);
                    return false;
                }
                priority = findMe.Priority;
                return true;
            }
        }
        #endregion

        public IEnumerator<TItem> GetEnumerator() {
            List<TItem> queueData = new List<TItem>();
            lock (m_queue) {
                //Copy to a separate list because we don't want to 'yield return' inside a lock
                foreach (var node in m_queue) {
                    queueData.Add(node.Data);
                }
            }

            return queueData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsValidQueue() {
            lock (m_queue) {
                // Check all items in cache are in the queue
                foreach (IList<SimpleNode> nodes in m_itemToNodesCache.Values) {
                    foreach (SimpleNode node in nodes) {
                        if (!m_queue.Contains(node)) return false;
                    }
                }
                // Check all items in queue are in cache
                foreach (SimpleNode node in m_queue) {
                    if (GetExistingNode(node.Data) == null) return false;
                }
                // Check queue structure itself
                return m_queue.IsValidQueue();
            }
        }
    }

    /// <summary>
    /// A simplified priority queue implementation.  Is stable, auto-resizes, and thread-safe, at the cost of being slightly slower than
    /// FastPriorityQueue
    /// This class is kept here for backwards compatibility.  It's recommended you use SimplePriorityQueue&lt;TItem, TPriority&gt;
    /// </summary>
    /// <typeparam name="TItem">The type to enqueue</typeparam>
    public class SimplePriorityQueue<TItem> : SimplePriorityQueue<TItem, float> {
        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        public SimplePriorityQueue() { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="comparer">The comparer used to compare priority values.  Defaults to Comparer&lt;float&gt;.default</param>
        public SimplePriorityQueue(IComparer<float> comparer) : base(comparer) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="comparer">The comparison function to use to compare priority values</param>
        public SimplePriorityQueue(Comparison<float> comparer) : base(comparer) { }
    }
}