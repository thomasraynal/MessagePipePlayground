﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePipePlayground.Sandbox
{
    public static partial class DisposableBag
    {
        public static IDisposable Create(params IDisposable[] disposables)
        {
            return new NthDisposable(disposables);
        }

        sealed class NthDisposable : IDisposable
        {
            bool disposed;
            readonly IDisposable[] disposables;

            public NthDisposable(IDisposable[] disposables)
            {
                this.disposables = disposables;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    foreach (var item in disposables)
                    {
                        item.Dispose();
                    }
                }
            }
        }

        public static SingleAssignmentDisposable CreateSingleAssignment()
        {
            return new SingleAssignmentDisposable();
        }

        public static DisposableBagBuilder CreateBuilder()
        {
            return new DisposableBagBuilder();
        }

        public static DisposableBagBuilder CreateBuilder(int initialCapacity)
        {
            return new DisposableBagBuilder(initialCapacity);
        }

        public static IDisposable Empty => EmptyDisposable.Instance;

        public static void AddTo(this IDisposable disposable, DisposableBagBuilder disposableBag)
        {
            disposableBag.Add(disposable);
        }

        public static SingleAssignmentDisposable SetTo(this IDisposable disposable, SingleAssignmentDisposable singleAssignmentDisposable)
        {
            singleAssignmentDisposable.Disposable = disposable;
            return singleAssignmentDisposable;
        }
    }

    internal class EmptyDisposable : IDisposable
    {
        internal static readonly IDisposable Instance = new EmptyDisposable();

        EmptyDisposable()
        {
        }

        public void Dispose()
        {
        }
    }

    public partial class DisposableBagBuilder
    {
        readonly List<IDisposable> disposables;

        internal DisposableBagBuilder()
        {
            disposables = new List<IDisposable>();
        }

        internal DisposableBagBuilder(int initialCapacity)
        {
            disposables = new List<IDisposable>(initialCapacity);
        }

        public void Add(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        public void Clear()
        {
            foreach (var item in disposables)
            {
                item.Dispose();
            }
            disposables.Clear();
        }



        //public IDisposable Build() in Disposables.tt(Disposables.cs)
    }

    public sealed class SingleAssignmentDisposable : IDisposable
    {
        IDisposable? inner;
        bool isDisposed;
        readonly object gate = new object();

        public IDisposable Disposable
        {
            set
            {
                lock (gate)
                {
                    if (isDisposed)
                    {
                        // already disposed, dispose immediately
                        value.Dispose();
                        return;
                    }
                    else
                    {
                        if (inner == null)
                        {
                            // set new Disposable once.
                            inner = value;
                            return;
                        }
                        else
                        {
                            // set twice is invalid.
                            throw new InvalidOperationException("Set IDisposable twice is invalid.");
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;
            lock (gate)
            {
                isDisposed = true;
                if (inner != null)
                {
                    inner.Dispose();
                    inner = null;
                }
            }
        }
    }
}
