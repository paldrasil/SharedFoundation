using System.Collections.Generic;
using UnityEngine;

namespace Shared.Foundation
{
    public class DataStore: MonoBehaviour
    {
        public delegate void MiddlewareCall(StateCollection state, FluxAction action, NextCall next);
        public delegate void NextCall(FluxAction action);
        public delegate void ActionDoneCall(FluxAction action);
        public delegate void DataStoreChangeCall(StateCollection oldState, StateCollection newState);
        public static event DataStoreChangeCall eventDataStoreChange;

        private static DataStore _instance = null;

        private StateCollection _states;
        private BaseReducer[] _reducers;
        private MiddlewareCall[] _middlewares;

        private Queue<Work> _workQueue = new Queue<Work>();
        private Work _currentWork = null;

        public bool IsIdle => _currentWork == null && _workQueue.Count == 0;

        public static DataStore instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DataStore");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<DataStore>();
                }
                return _instance;
            }
        }

        public IStoreData GetState(string stateName)
        {
            return _states.GetState(stateName);
        }

        public StateCollection GetStore()
        {
            return _states;
        }

        public void Initialize(StateCollection state, MiddlewareCall[] middlewares, BaseReducer[] reducers)
        {
            this._middlewares = middlewares;
            this._reducers = reducers;
            this._states = state;

            for (int i = 0;i < reducers.Length;++i)
            {
                BaseReducer reducer = reducers[i];
                _states.SetState(reducer.GetName(), reducers[i].GetDefault());
            }
        }

        public void Dispatch(FluxAction action, ActionDoneCall callback)
        {
            _workQueue.Enqueue(new Work(_middlewares, action, callback));
        }

        public void CleanQueue()
        {
            _workQueue.Clear();
        }

        public void Update()
        {
            if (_currentWork != null)
            {
                if (!_currentWork.IsWorking())
                {
                    if (_currentWork.IsMiddlewareDone())
                    {
                        var oldState = _states;
                        _states = oldState.CreateNew();

                        for (int i = 0;i < _reducers.Length;++i)
                        {
                            BaseReducer reducer = _reducers[i];
                            _states.SetState(reducer.GetName(), reducer.Reduce(oldState.GetState(reducer.GetName()), _currentWork.GetAction()));
                        }

                        _currentWork.Callback();
                        _currentWork = null;
                        if (eventDataStoreChange != null)
                        {
                            eventDataStoreChange.Invoke(oldState, _states);
                        }
                    }
                    else
                    {
                        _currentWork.SetWorking(true);
                        MiddlewareCall call = _currentWork.CurrentMiddleware();
                        call(_states, _currentWork.GetAction(), new NextCall(_currentWork.NextMiddleware));
                    }
                }
            }

            if (_currentWork == null && _workQueue.Count > 0)
            {
                _currentWork = _workQueue.Dequeue();
            }
        }

        private class Work
        {
            private MiddlewareCall[] _chain;
            private int _chainIndex;
            private FluxAction _action;
            private bool _isWorking;
            private ActionDoneCall _callback;

            public Work(MiddlewareCall[] chain, FluxAction action, ActionDoneCall callback)
            {
                this._chain = chain;
                this._chainIndex = 0;
                this._action = action;
                this._isWorking = false;
                this._callback = callback;
            }

            public bool IsMiddlewareDone()
            {
                return _chainIndex >= _chain.Length && !_isWorking;
            }

            public void NextMiddleware(FluxAction action)
            {
                this._action = action;
                this._isWorking = false;
                this._chainIndex++;
            }

            public MiddlewareCall CurrentMiddleware()
            {
                return this._chain[this._chainIndex];
            }

            public FluxAction GetAction()
            {
                return this._action;
            }

            public void SetWorking(bool val)
            {
                _isWorking = val;
            }

            public bool IsWorking()
            {
                return _isWorking;
            }

            public void Callback()
            {
                this._action.isDone = true;
                this._callback?.Invoke(this._action);
            }
        }
    }
}
