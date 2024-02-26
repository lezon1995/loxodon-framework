/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using UnityEngine;
using System.Collections.Generic;
using Loxodon.Framework.Binding.Binders;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding.Builder;
using Loxodon.Framework.Contexts;
using System;

namespace Loxodon.Framework.Binding
{
    public static class BehaviourBindingExtension
    {
        private static IBinder binder;

        public static IBinder Binder
        {
            get
            {
                if (binder == null)
                    binder = Context.GetApplicationContext().GetService<IBinder>();

                if (binder == null)
                    throw new Exception("Data binding service is not initialized,please create a BindingServiceBundle service before using it.");

                return binder;
            }
        }

        public static IBindingContext BindingContext(this Behaviour behaviour)
        {
            return GetContext(behaviour);
        }

        static IBindingContext GetContext(Behaviour behaviour)
        {
            if (behaviour == null || behaviour.gameObject == null)
                return null;

            if (!behaviour.TryGetComponent<BindingContextLifecycle>(out var ctxLifecycle))
            {
                ctxLifecycle = behaviour.gameObject.AddComponent<BindingContextLifecycle>();
            }

            IBindingContext context = ctxLifecycle.BindingContext;
            if (context == null)
            {
                context = new BindingContext(behaviour, Binder);
                ctxLifecycle.BindingContext = context;
            }

            return context;
        }

        public static BindingSet<V, VM> CreateBindingSet<V, VM>(this V behaviour) where V : Behaviour
        {
            return new BindingSet<V, VM>(GetContext(behaviour), behaviour);
        }

        public static BindingSet<V, VM> CreateBindingSet<V, VM>(this V behaviour, VM dataContext) where V : Behaviour
        {
            var context = GetContext(behaviour);
            context.DataContext = dataContext;
            return new BindingSet<V, VM>(context, behaviour);
        }

        public static BindingSet<V> CreateBindingSet<V>(this V behaviour) where V : Behaviour
        {
            return new BindingSet<V>(GetContext(behaviour), behaviour);
        }

        public static BindingSet CreateSimpleBindingSet(this Behaviour behaviour)
        {
            return new BindingSet(GetContext(behaviour), behaviour);
        }

        public static void SetDataContext(this Behaviour behaviour, object dataContext)
        {
            GetContext(behaviour).DataContext = dataContext;
        }

        public static object GetDataContext(this Behaviour behaviour)
        {
            return GetContext(behaviour).DataContext;
        }

        public static void AddBinding(this Behaviour behaviour, BindingDescription bindingDescription)
        {
            GetContext(behaviour).Add(behaviour, bindingDescription);
        }

        public static void AddBindings(this Behaviour behaviour, IEnumerable<BindingDescription> bindingDescriptions)
        {
            GetContext(behaviour).Add(behaviour, bindingDescriptions);
        }

        public static void AddBinding(this Behaviour behaviour, IBinding binding)
        {
            GetContext(behaviour).Add(binding);
        }

        public static void AddBinding(this Behaviour behaviour, IBinding binding, object key = null)
        {
            GetContext(behaviour).Add(binding, key);
        }

        public static void AddBindings(this Behaviour behaviour, IEnumerable<IBinding> bindings, object key = null)
        {
            if (bindings == null)
                return;

            GetContext(behaviour).Add(bindings, key);
        }

        public static void AddBinding(this Behaviour behaviour, object target, BindingDescription bindingDescription, object key = null)
        {
            GetContext(behaviour).Add(target, bindingDescription, key);
        }

        public static void AddBindings(this Behaviour behaviour, object target, IEnumerable<BindingDescription> bindingDescriptions, object key = null)
        {
            GetContext(behaviour).Add(target, bindingDescriptions, key);
        }

        public static void AddBindings(this Behaviour behaviour, IDictionary<object, IEnumerable<BindingDescription>> bindingMap, object key = null)
        {
            if (bindingMap == null)
                return;

            IBindingContext context = GetContext(behaviour);
            foreach (var (o, bindingDescriptions) in bindingMap)
            {
                context.Add(o, bindingDescriptions, key);
            }
        }

        public static void ClearBindings(this Behaviour behaviour, object key)
        {
            GetContext(behaviour).Clear(key);
        }

        public static void ClearAllBindings(this Behaviour behaviour)
        {
            GetContext(behaviour).Clear();
        }
    }
}