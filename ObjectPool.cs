/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
////
//// Three kinds of generic object pools to avoid memory deallocations
//// in Unity-based games. See my Gamasutra articles.
//// Released under a Creative Commons Attribution (CC BY) License,
//// see http://creativecommons.org/licenses/
////
//// (c) 2013 Wendelin Reich.
////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace MemoryManagment
{
    public class ObjectPool<T> where T : class, new()
    {
        private Stack<T> m_objectStack;

        private Action<T> m_resetAction;
        private Action<T> m_onetimeInitAction;

        public ObjectPool(int initialBufferSize, Action<T> ResetAction = null, Action<T> OnetimeInitAction = null)
        {
            m_objectStack = new Stack<T>(initialBufferSize);
            m_resetAction = ResetAction;
            m_onetimeInitAction = OnetimeInitAction;
        }

        public T New()
        {
            if (m_objectStack.Count > 0)
            {
                T t = m_objectStack.Pop();

                if (m_resetAction != null)
                    m_resetAction(t);

                return t;
            }
            else
            {
                T t = new T();

                if (m_onetimeInitAction != null)
                    m_onetimeInitAction(t);

                return t;
            }
        }

        public void Store(T obj)
        {
            m_objectStack.Push(obj);
        }
    }

    public interface IResetable
    {
        void Reset();
    }

    public class ObjectPoolWithReset<T> where T : class, IResetable, new()
    {
        private Stack<T> m_objectStack;

        private Action<T> m_resetAction;
        private Action<T> m_onetimeInitAction;

        public ObjectPoolWithReset(int initialBufferSize, Action<T> ResetAction = null, Action<T> OnetimeInitAction = null)
        {
            m_objectStack = new Stack<T>(initialBufferSize);
            m_resetAction = ResetAction;
            m_onetimeInitAction = OnetimeInitAction;
        }

        public T New()
        {
            if (m_objectStack.Count > 0)
            {
                T t = m_objectStack.Pop();

                t.Reset();

                if (m_resetAction != null)
                    m_resetAction(t);

                return t;
            }
            else
            {
                T t = new T();

                if (m_onetimeInitAction != null)
                    m_onetimeInitAction(t);

                return t;
            }
        }

        public void Store(T obj)
        {
            m_objectStack.Push(obj);
        }
    }

    public class ObjectPoolWithCollectiveReset<T> where T : class, IResetable, new()
    {
        private List<T> m_objectList;
        private int m_nextAvailableIndex = 0;

        private Func<T> m_instantiateAction;
        private Action<T> m_resetAction;
        private Action<T> m_onetimeInitAction;

        public ObjectPoolWithCollectiveReset(
            int initialBufferSize,
            Func<T> InstantiateAction = null,
            Action<T> ResetAction = null,
            Action<T> OnetimeInitAction = null)
        {
            m_objectList = new List<T>(initialBufferSize);
            m_instantiateAction = InstantiateAction;
            m_resetAction = ResetAction;
            m_onetimeInitAction = OnetimeInitAction;
        }

        public T New()
        {
            if (m_nextAvailableIndex < m_objectList.Count)
            {
                // an allocated object is already available; just reset it
                T t = m_objectList[m_nextAvailableIndex];
                t.Reset();

                m_nextAvailableIndex++;

                if (m_resetAction != null)
                    m_resetAction(t);

                return t;
            }
            else {
                T t = null;
                // no allocated object is available; create a new one and grow the internal object list
                t = m_instantiateAction != null ? m_instantiateAction() : new T();
                m_objectList.Add(t);
                m_nextAvailableIndex++;

                if (m_onetimeInitAction != null)
                    m_onetimeInitAction(t);

                return t;
            }
        }

        public void ResetAll()
        {
            m_nextAvailableIndex = 0;
        }
    }
}
