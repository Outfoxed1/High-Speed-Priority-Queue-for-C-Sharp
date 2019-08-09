using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Priority_Queue {
    /// <summary>
    /// An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
    /// See https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/wiki/Getting-Started for more information
    /// </summary>
    /// <typeparam name="T">The values in the queue.  Must extend the FastPriorityQueueNode class</typeparam>
    public sealed class FastPriorityQueueInt<T> : IFixedSizePriorityQueue<T, int> where T : FastPriorityQueueNodeInt {
        private int m_numNodes;
        private T[] m_nodes;

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this will cause undefined behavior)</param>
        public FastPriorityQueueInt(int maxNodes) {
            #region Debug
#if DEBUG
            if (maxNodes <= 0) throw new InvalidOperationException("New queue size cannot be smaller than 1");
#endif
            #endregion
            m_numNodes = 0;
            m_nodes = new T[maxNodes + 1];
        }

        /// <summary>
        /// Returns the number of nodes in the queue.
        /// O(1)
        /// </summary>
        public int Count { get => m_numNodes; }

        /// <summary>
        /// Returns the maximum number of items that can be enqueued at once in this queue.  Once you hit this number (ie. once Count == MaxSize),
        /// attempting to enqueue another item will cause undefined behavior.  O(1)
        /// </summary>
        public int MaxSize { get => m_nodes.Length - 1; }

        /// <summary>
        /// Removes every node from the queue.
        /// O(n) (So, don't do this often!)
        /// </summary>
        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        public void Clear() {
            Array.Clear(m_nodes, 1, m_numNodes);
            m_numNodes = 0;
        }

        /// <summary>
        /// Returns (in O(1)!) whether the given node is in the queue.
        /// If node is or has been previously added to another queue, the result is undefined unless oldQueue.ResetNode(node) has been called
        /// O(1)
        /// </summary>
        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        public bool Contains(T node) {
            #region Debug
#if DEBUG
            if (node == null) throw new ArgumentNullException("node");
            if (node.Queue != null && !Equals(node.Queue)) throw new InvalidOperationException("node.Contains was called on a node from another queue.  Please call originalQueue.ResetNode() first");
            if (node.QueueIndex < 0 || node.QueueIndex >= m_nodes.Length) throw new InvalidOperationException("node.QueueIndex has been corrupted. Did you change it manually? Or add this node to another queue?");
#endif
            #endregion
            return (m_nodes[node.QueueIndex] == node);
        }

        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        public void Enqueue(T node, int priority) {
            #region Debug
#if DEBUG
            if (node == null) throw new ArgumentNullException("node");
            if (m_numNodes >= m_nodes.Length - 1) throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
            if (node.Queue != null && !Equals(node.Queue)) throw new InvalidOperationException("node.Enqueue was called on a node from another queue.  Please call originalQueue.ResetNode() first");
            if (Contains(node)) throw new InvalidOperationException("Node is already enqueued: " + node);
            node.Queue = this;
#endif
            #endregion
            node.Priority = priority;
            m_numNodes++;
            m_nodes[m_numNodes] = node;
            node.QueueIndex = m_numNodes;
            CascadeUp(node);
        }

        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        private void CascadeUp(T node) {
            //aka Heapify-up
            int parent;
            if (node.QueueIndex > 1) {
                parent = node.QueueIndex >> 1;
                T parentNode = m_nodes[parent];
                if (HasHigherOrEqualPriority(parentNode, node)) return;
                //Node has lower priority value, so move parent down the heap to make room
                m_nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;
                node.QueueIndex = parent;
            } else {
                return;
            }
            while (parent > 1) {
                parent >>= 1;
                T parentNode = m_nodes[parent];
                if (HasHigherOrEqualPriority(parentNode, node)) break;
                //Node has lower priority value, so move parent down the heap to make room
                m_nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;
                node.QueueIndex = parent;
            }
            m_nodes[node.QueueIndex] = node;
        }

        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        private void CascadeDown(T node) {
            //aka Heapify-down
            int finalQueueIndex = node.QueueIndex;
            int childLeftIndex = 2 * finalQueueIndex;

            // If leaf node, we're done
            if (childLeftIndex > m_numNodes) return;

            // Check if the left-child is higher-priority than the current node
            int childRightIndex = childLeftIndex + 1;
            T childLeft = m_nodes[childLeftIndex];
            if (HasHigherPriority(childLeft, node)) {
                // Check if there is a right child. If not, swap and finish.
                if (childRightIndex > m_numNodes) {
                    node.QueueIndex = childLeftIndex;
                    childLeft.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childLeft;
                    m_nodes[childLeftIndex] = node;
                    return;
                }
                // Check if the left-child is higher-priority than the right-child
                T childRight = m_nodes[childRightIndex];
                if (HasHigherPriority(childLeft, childRight)) {
                    // left is highest, move it up and continue
                    childLeft.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                } else {
                    // right is even higher, move it up and continue
                    childRight.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            } else if (childRightIndex > m_numNodes) {
                // Not swapping with left-child, does right-child exist?
                return;
            } else {
                // Check if the right-child is higher-priority than the current node
                T childRight = m_nodes[childRightIndex];
                if (HasHigherPriority(childRight, node)) {
                    childRight.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                } else {
                    // Neither child is higher-priority than current, so finish and stop.
                    return;
                }
            }

            while (true) {
                childLeftIndex = 2 * finalQueueIndex;
                // If leaf node, we're done
                if (childLeftIndex > m_numNodes) {
                    node.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = node;
                    break;
                }
                // Check if the left-child is higher-priority than the current node
                childRightIndex = childLeftIndex + 1;
                childLeft = m_nodes[childLeftIndex];
                if (HasHigherPriority(childLeft, node)) {
                    // Check if there is a right child. If not, swap and finish.
                    if (childRightIndex > m_numNodes) {
                        node.QueueIndex = childLeftIndex;
                        childLeft.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childLeft;
                        m_nodes[childLeftIndex] = node;
                        break;
                    }
                    // Check if the left-child is higher-priority than the right-child
                    T childRight = m_nodes[childRightIndex];
                    if (HasHigherPriority(childLeft, childRight)) {
                        // left is highest, move it up and continue
                        childLeft.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childLeft;
                        finalQueueIndex = childLeftIndex;
                    } else {
                        // right is even higher, move it up and continue
                        childRight.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                } else if (childRightIndex > m_numNodes) {
                    // Not swapping with left-child, does right-child exist?
                    node.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = node;
                    break;
                } else {
                    // Check if the right-child is higher-priority than the current node
                    T childRight = m_nodes[childRightIndex];
                    if (HasHigherPriority(childRight, node)) {
                        childRight.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    } else {
                        // Neither child is higher-priority than current, so finish and stop.
                        node.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = node;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
        /// Note that calling HasHigherPriority(node, node) (ie. both arguments the same node) will return false
        /// </summary>
        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        private bool HasHigherPriority(T higher, T lower) => (higher.Priority < lower.Priority);

        /// <summary>
        /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
        /// Note that calling HasHigherOrEqualPriority(node, node) (ie. both arguments the same node) will return true
        /// </summary>
        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        private bool HasHigherOrEqualPriority(T higher, T lower) => (higher.Priority <= lower.Priority);

        /// <summary>
        /// Removes the head of the queue and returns it.
        /// If queue is empty, result is undefined
        /// O(log n)
        /// </summary>
        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        public T Dequeue() {
            #region Debug
#if DEBUG
            if (m_numNodes <= 0) throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            if (!IsValidQueue()) throw new InvalidOperationException("Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority() or add the same node to two different queues?)");
#endif
            #endregion
            T returnMe = m_nodes[1];
            //If the node is already the last node, we can remove it immediately
            if (m_numNodes == 1) {
                m_nodes[1] = null;
                m_numNodes = 0;
                return returnMe;
            }

            //Swap the node with the last node
            T formerLastNode = m_nodes[m_numNodes];
            m_nodes[1] = formerLastNode;
            formerLastNode.QueueIndex = 1;
            m_nodes[m_numNodes] = null;
            m_numNodes--;

            //Now bubble formerLastNode (which is no longer the last node) down
            CascadeDown(formerLastNode);
            return returnMe;
        }

        /// <summary>
        /// Resize the queue so it can accept more nodes.  All currently enqueued nodes are remain.
        /// Attempting to decrease the queue size to a size too small to hold the existing nodes results in undefined behavior
        /// O(n)
        /// </summary>
        public void Resize(int maxNodes) {
            #region Debug
#if DEBUG
            if (maxNodes <= 0) throw new InvalidOperationException("Queue size cannot be smaller than 1");
            if (maxNodes < m_numNodes) throw new InvalidOperationException($"Called Resize({maxNodes}), but current queue contains {m_numNodes} nodes");
#endif
            #endregion
            T[] newArray = new T[maxNodes + 1];
            int highestIndexToCopy = Math.Min(maxNodes, m_numNodes);
            Array.Copy(m_nodes, newArray, highestIndexToCopy + 1);
            m_nodes = newArray;
        }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// If the queue is empty, behavior is undefined.
        /// O(1)
        /// </summary>
        public T First {
            get {
                #region Debug
#if DEBUG
                if (m_numNodes <= 0) throw new InvalidOperationException("Cannot call .First on an empty queue");
#endif
                #endregion
                return m_nodes[1];
            }
        }

        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        public void UpdatePriority(T node, int priority) {
            #region Debug
#if DEBUG
            if (node == null) throw new ArgumentNullException("node");
            if (node.Queue != null && !Equals(node.Queue)) throw new InvalidOperationException("node.UpdatePriority was called on a node from another queue");
            if (!Contains(node)) throw new InvalidOperationException($"Cannot call UpdatePriority() on a node which is not enqueued: {node}");
#endif
            #endregion
            node.Priority = priority;
            OnNodeUpdated(node);
        }

        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        private void OnNodeUpdated(T node) {
            //Bubble the updated node up or down as appropriate
            int parentIndex = node.QueueIndex >> 1;

            if (parentIndex > 0 && HasHigherPriority(node, m_nodes[parentIndex])) {
                CascadeUp(node);
            } else {
                //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
                CascadeDown(node);
            }
        }

        /// <summary>
        /// Removes a node from the queue.  The node does not need to be the head of the queue.  
        /// If the node is not in the queue, the result is undefined.  If unsure, check Contains() first
        /// O(log n)
        /// </summary>
        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        public void Remove(T node) {
            #region Debug
#if DEBUG
            if (node == null) throw new ArgumentNullException("node");
            if (node.Queue != null && !Equals(node.Queue)) throw new InvalidOperationException("node.Remove was called on a node from another queue");
            if (!Contains(node)) throw new InvalidOperationException($"Cannot call Remove() on a node which is not enqueued: {node}");
#endif
            #endregion
            //If the node is already the last node, we can remove it immediately
            if (node.QueueIndex == m_numNodes) {
                m_nodes[m_numNodes] = null;
                m_numNodes--;
                return;
            }

            //Swap the node with the last node
            T formerLastNode = m_nodes[m_numNodes];
            m_nodes[node.QueueIndex] = formerLastNode;
            formerLastNode.QueueIndex = node.QueueIndex;
            m_nodes[m_numNodes] = null;
            m_numNodes--;

            //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
            OnNodeUpdated(formerLastNode);
        }

        /// <summary>
        /// By default, nodes that have been previously added to one queue cannot be added to another queue.
        /// If you need to do this, please call originalQueue.ResetNode(node) before attempting to add it in the new queue
        /// If the node is currently in the queue or belongs to another queue, the result is undefined
        /// </summary>
        #region NET_VERSION_4_5
#if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        #endregion
        public void ResetNode(T node) {
            #region Debug
#if DEBUG
            if (node == null) throw new ArgumentNullException("node");
            if (node.Queue != null && !Equals(node.Queue)) throw new InvalidOperationException("node.ResetNode was called on a node from another queue");
            if (Contains(node)) throw new InvalidOperationException("node.ResetNode was called on a node that is still in the queue");
            node.Queue = null;
#endif
            #endregion
            node.QueueIndex = 0;
        }

        public IEnumerator<T> GetEnumerator() {
#if NET_VERSION_4_5 // ArraySegment does not implement IEnumerable before 4.5
            return (new ArraySegment<T>(m_nodes, 1, m_numNodes) as IEnumerable<T>).GetEnumerator();
#else
            for (int i = 1; i <= m_numNodes; i++) yield return m_nodes[i];            
#endif
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// <b>Should not be called in production code.</b>
        /// Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
        /// </summary>
        public bool IsValidQueue() {
            for (int i = 1; i < m_nodes.Length; i++) {
                if (m_nodes[i] != null) {
                    int childLeftIndex = 2 * i;
                    if (childLeftIndex < m_nodes.Length && m_nodes[childLeftIndex] != null && HasHigherPriority(m_nodes[childLeftIndex], m_nodes[i])) return false;
                    int childRightIndex = childLeftIndex + 1;
                    if (childRightIndex < m_nodes.Length && m_nodes[childRightIndex] != null && HasHigherPriority(m_nodes[childRightIndex], m_nodes[i])) return false;
                }
            }
            return true;
        }
    }
}
