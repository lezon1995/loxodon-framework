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

using System;
using System.Collections.Generic;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Log;
#if UNITY_2019_1_OR_NEWER
#endif

namespace Loxodon.Framework.Binding.Builder
{
    public abstract class BindingSetBase : IBindingBuilder
    {
        static readonly ILog log = LogManager.GetLogger(typeof(BindingSetBase));

        protected IBindingContext context;
        protected List<IBindingBuilder> builders = new List<IBindingBuilder>();

        protected BindingSetBase(IBindingContext bindingContext)
        {
            context = bindingContext;
        }

        public virtual void Build()
        {
            foreach (var builder in builders)
            {
                try
                {
                    builder.Build();
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.ErrorFormat("{0}", e);
                }
            }

            builders.Clear();
        }
    }

    public class BindingSet<V, VM> : BindingSetBase where V : class
    {
        V target;

        public BindingSet(IBindingContext context, V _target) : base(context)
        {
            target = _target;
        }

        public virtual BindingBuilder<V, VM> Bind()
        {
            var builder = new BindingBuilder<V, VM>(context, target);
            builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<T, VM> Bind<T>(T target) where T : class
        {
            var builder = new BindingBuilder<T, VM>(context, target);
            builders.Add(builder);
            return builder;
        }

//#if UNITY_2019_1_OR_NEWER
//        public virtual BindingBuilder<T, VM> Bind<T>(string targetName = null) where T : VisualElement
//        {
//            UIDocument document = (target as UnityEngine.Behaviour).GetComponent<UIDocument>();
//            if (document == null)
//                throw new Exception("The UIDocument not found, this is not a UIToolkit view.");

//            VisualElement rootVisualElement = document.rootVisualElement;
//            T target = string.IsNullOrEmpty(targetName) ? rootVisualElement.Q<T>() : rootVisualElement.Q<T>(targetName);
//            var builder = new BindingBuilder<T, VM>(context, target);
//            builders.Add(builder);
//            return builder;
//        }
//#endif
    }

    public class BindingSet<V> : BindingSetBase where V : class
    {
        V target;

        public BindingSet(IBindingContext context, V _target) : base(context)
        {
            target = _target;
        }

        public virtual BindingBuilder<V> Bind()
        {
            var builder = new BindingBuilder<V>(context, target);
            builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<T> Bind<T>(T target) where T : class
        {
            var builder = new BindingBuilder<T>(context, target);
            builders.Add(builder);
            return builder;
        }

//#if UNITY_2019_1_OR_NEWER
//        public virtual BindingBuilder<T> Bind<T>(string targetName = null) where T : VisualElement
//        {
//            UIDocument document = (target as UnityEngine.Behaviour).GetComponent<UIDocument>();
//            if (document == null)
//                throw new Exception("The UIDocument not found, this is not a UIToolkit view.");

//            VisualElement rootVisualElement = document.rootVisualElement;
//            T target = string.IsNullOrEmpty(targetName) ? rootVisualElement.Q<T>() : rootVisualElement.Q<T>(targetName);
//            var builder = new BindingBuilder<T>(context, target);
//            builders.Add(builder);
//            return builder;
//        }
// #endif
    }

    public class BindingSet : BindingSetBase
    {
        object target;

        public BindingSet(IBindingContext context, object _target) : base(context)
        {
            target = _target;
        }

        public virtual BindingBuilder Bind()
        {
            var builder = new BindingBuilder(context, target);
            builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder Bind(object target)
        {
            var builder = new BindingBuilder(context, target);
            builders.Add(builder);
            return builder;
        }
    }
}