﻿
using System;
using System.Collections.Generic;

namespace Orleans.Providers.Streams.Common
{
    /// <summary>
    /// Simple object pool that uses a stack to store available objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IObjectPool<T>
        where T : PooledResource<T>
    {
        private const int DefaultPoolCapacity = 1 << 10; // 1k
        private readonly Stack<T> pool;
        private readonly Func<IObjectPool<T>, T> factoryFunc;

        /// <summary>
        /// Simple object pool
        /// </summary>
        /// <param name="factoryFunc"></param>
        /// <param name="initialCapacity"></param>
        public ObjectPool(Func<IObjectPool<T>, T> factoryFunc, int initialCapacity = DefaultPoolCapacity)
        {
            if (factoryFunc == null)
            {
                throw new ArgumentNullException("factoryFunc");
            }
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException("initialCapacity");
            }
            this.factoryFunc = factoryFunc;
            pool = new Stack<T>(initialCapacity);
        }

        /// <summary>
        /// Allocates a pooled resource
        /// </summary>
        /// <returns></returns>
        public virtual T Allocate()
        {
            return pool.Count != 0
                ? pool.Pop()
                : factoryFunc(this);
        }

        /// <summary>
        /// Returns a resource to the pool
        /// </summary>
        /// <param name="resource"></param>
        public virtual void Free(T resource)
        {
            pool.Push(resource);
        }
    }
}
