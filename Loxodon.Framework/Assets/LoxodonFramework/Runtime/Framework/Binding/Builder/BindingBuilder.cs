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
using System.Linq.Expressions;
using Loxodon.Framework.Binding.Contexts;
using Loxodon.Framework.Binding.Converters;
using Loxodon.Framework.Interactivity;
using Loxodon.Log;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Loxodon.Framework.Binding.Builder
{
    public class BindingBuilder<T, VM> : BindingBuilderBase where T : class
    {
        static readonly ILog log = LogManager.GetLogger(typeof(BindingBuilder<T, VM>));

        public BindingBuilder(IBindingContext context, T target) : base(context, target)
        {
            description.TargetType = typeof(T);
        }

        public BindingBuilder<T, VM> For(string targetName)
        {
            description.TargetName = targetName;
            description.UpdateTrigger = null;
            return this;
        }

        public BindingBuilder<T, VM> For(string targetName, string updateTrigger)
        {
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T, VM> For<R>(Expression<Func<T, R>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = null;
            return this;
        }

        public BindingBuilder<T, VM> For<R>(Expression<Func<T, R>> memberExpression, string updateTrigger)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T, VM> For<R, TEvent>(Expression<Func<T, R>> memberExpression, Expression<Func<T, TEvent>> updateTriggerExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            string updateTrigger = PathParser.ParseMemberName(updateTriggerExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T, VM> For(Expression<Func<T, EventHandler<InteractionEventArgs>>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = null;
            OneWayToSource();
            return this;
        }

#if UNITY_2019_1_OR_NEWER
        public BindingBuilder<T, VM> For<R>(Expression<Func<T, R>> memberExpression, Expression<Func<T, Func<EventCallback<ChangeEvent<R>>, bool>>> updateTriggerExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            string updateTrigger = PathParser.ParseMemberName(updateTriggerExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T, VM> For<R>(Expression<Func<T, Func<EventCallback<ChangeEvent<R>>, bool>>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = null;
            OneWayToSource();
            return this;
        }
#endif

        public BindingBuilder<T, VM> To(string path)
        {
            SetMemberPath(path);
            return this;
        }

        public BindingBuilder<T, VM> To<R>(Expression<Func<VM, R>> path)
        {
            SetMemberPath(PathParser.Parse(path));
            return this;
        }

        public BindingBuilder<T, VM> To<TParameter>(Expression<Func<VM, Action<TParameter>>> path)
        {
            SetMemberPath(PathParser.Parse(path));
            return this;
        }

        public BindingBuilder<T, VM> To(Expression<Func<VM, Action>> path)
        {
            SetMemberPath(PathParser.Parse(path));
            return this;
        }

        public BindingBuilder<T, VM> ToExpression<R>(Expression<Func<VM, R>> expression)
        {
            SetExpression(expression);
            OneWay();
            return this;
        }

        public BindingBuilder<T, VM> TwoWay()
        {
            SetMode(BindingMode.TwoWay);
            return this;
        }

        public BindingBuilder<T, VM> OneWay()
        {
            SetMode(BindingMode.OneWay);
            return this;
        }

        public BindingBuilder<T, VM> OneWayToSource()
        {
            SetMode(BindingMode.OneWayToSource);
            return this;
        }

        public BindingBuilder<T, VM> OneTime()
        {
            SetMode(BindingMode.OneTime);
            return this;
        }

        //public BindingBuilder<T, VM> CommandParameter(object parameter)
        //{
        //    SetCommandParameter(parameter);
        //    return this;
        //}

        public BindingBuilder<T, VM> CommandParameter<P>(P parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<T, VM> CommandParameter<TParam>(Func<TParam> parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<T, VM> WithConversion(string converterName)
        {
            var converter = ConverterByName(converterName);
            return WithConversion(converter);
        }

        public BindingBuilder<T, VM> WithConversion(IConverter converter)
        {
            description.Converter = converter;
            return this;
        }

        public BindingBuilder<T, VM> WithScopeKey(object scopeKey)
        {
            SetScopeKey(scopeKey);
            return this;
        }
    }

    public class BindingBuilder<T> : BindingBuilderBase where T : class
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(BindingBuilder<T>));

        public BindingBuilder(IBindingContext context, T target) : base(context, target)
        {
            description.TargetType = typeof(T);
        }

        public BindingBuilder<T> For(string targetPropertyName)
        {
            description.TargetName = targetPropertyName;
            description.UpdateTrigger = null;
            return this;
        }

        public BindingBuilder<T> For(string targetPropertyName, string updateTrigger)
        {
            description.TargetName = targetPropertyName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T> For<R>(Expression<Func<T, R>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = null;
            return this;
        }

        public BindingBuilder<T> For<R>(Expression<Func<T, R>> memberExpression, string updateTrigger)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T> For<R, TEvent>(Expression<Func<T, R>> memberExpression, Expression<Func<T, TEvent>> updateTriggerExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            string updateTrigger = PathParser.ParseMemberName(updateTriggerExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T> For(Expression<Func<T, EventHandler<InteractionEventArgs>>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = null;
            OneWayToSource();
            return this;
        }

#if UNITY_2019_1_OR_NEWER
        public BindingBuilder<T> For<R>(Expression<Func<T, R>> memberExpression, Expression<Func<T, Func<EventCallback<ChangeEvent<R>>, bool>>> updateTriggerExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            string updateTrigger = PathParser.ParseMemberName(updateTriggerExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<T> For<R>(Expression<Func<T, Func<EventCallback<ChangeEvent<R>>, bool>>> memberExpression)
        {
            string targetName = PathParser.ParseMemberName(memberExpression);
            description.TargetName = targetName;
            description.UpdateTrigger = null;
            OneWayToSource();
            return this;
        }
#endif

        public BindingBuilder<T> To(string path)
        {
            SetStaticMemberPath(path);
            OneWay();
            return this;
        }

        public BindingBuilder<T> To<R>(Expression<Func<R>> path)
        {
            SetStaticMemberPath(PathParser.ParseStaticPath(path));
            OneWay();
            return this;
        }

        public BindingBuilder<T> To<TParameter>(Expression<Func<Action<TParameter>>> path)
        {
            SetStaticMemberPath(PathParser.ParseStaticPath(path));
            return this;
        }

        public BindingBuilder<T> To(Expression<Func<Action>> path)
        {
            SetStaticMemberPath(PathParser.ParseStaticPath(path));
            return this;
        }

        public BindingBuilder<T> ToValue(object value)
        {
            SetLiteral(value);
            return this;
        }

        public BindingBuilder<T> ToExpression<R>(Expression<Func<R>> expression)
        {
            SetExpression(expression);
            OneWay();
            return this;
        }

        public BindingBuilder<T> TwoWay()
        {
            SetMode(BindingMode.TwoWay);
            return this;
        }

        public BindingBuilder<T> OneWay()
        {
            SetMode(BindingMode.OneWay);
            return this;
        }

        public BindingBuilder<T> OneWayToSource()
        {
            SetMode(BindingMode.OneWayToSource);
            return this;
        }

        public BindingBuilder<T> OneTime()
        {
            SetMode(BindingMode.OneTime);
            return this;
        }

        //public BindingBuilder<T> CommandParameter(object parameter)
        //{
        //    SetCommandParameter(parameter);
        //    return this;
        //}

        public BindingBuilder<T> CommandParameter<P>(P parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<T> CommandParameter<TParam>(Func<TParam> parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<T> WithConversion(string converterName)
        {
            var converter = ConverterByName(converterName);
            return WithConversion(converter);
        }

        public BindingBuilder<T> WithConversion(IConverter converter)
        {
            description.Converter = converter;
            return this;
        }

        public BindingBuilder<T> WithScopeKey(object scopeKey)
        {
            SetScopeKey(scopeKey);
            return this;
        }
    }

    public class BindingBuilder : BindingBuilderBase
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(BindingBuilder));

        public BindingBuilder(IBindingContext context, object target) : base(context, target)
        {
        }

        public BindingBuilder For(string targetName, string updateTrigger = null)
        {
            description.TargetName = targetName;
            description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder To(string path)
        {
            SetMemberPath(path);
            return this;
        }

        public BindingBuilder ToStatic(string path)
        {
            SetStaticMemberPath(path);
            return this;
        }

        public BindingBuilder ToValue(object value)
        {
            SetLiteral(value);
            return this;
        }

        public BindingBuilder TwoWay()
        {
            SetMode(BindingMode.TwoWay);
            return this;
        }

        public BindingBuilder OneWay()
        {
            SetMode(BindingMode.OneWay);
            return this;
        }

        public BindingBuilder OneWayToSource()
        {
            SetMode(BindingMode.OneWayToSource);
            return this;
        }

        public BindingBuilder OneTime()
        {
            SetMode(BindingMode.OneTime);
            return this;
        }

        public BindingBuilder CommandParameter(object parameter)
        {
            SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder WithConversion(string converterName)
        {
            var converter = ConverterByName(converterName);
            return WithConversion(converter);
        }

        public BindingBuilder WithConversion(IConverter converter)
        {
            description.Converter = converter;
            return this;
        }

        public BindingBuilder WithScopeKey(object scopeKey)
        {
            SetScopeKey(scopeKey);
            return this;
        }
    }
}