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
using System.Reflection;
using System.Linq.Expressions;
using System.Text;
#if UNITY_IOS || ENABLE_IL2CPP
using Loxodon.Framework.Binding.Expressions;
#endif

namespace Loxodon.Framework.Binding.Paths
{
    public class PathParser : IPathParser
    {
        public virtual Path Parse(string pathText)
        {
            return TextPathParser.Parse(pathText);
        }

        public virtual Path Parse(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Path path = new Path();
            if (expression.Body is MemberExpression body)
            {
                Parse(body, path);
                return path;
            }

            if (expression.Body is MethodCallExpression method)
            {
                Parse(method, path);
                return path;
            }

            if (expression.Body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            {
                Parse(unary.Operand, path);
                return path;
            }

            if (expression.Body is BinaryExpression { NodeType: ExpressionType.ArrayIndex } binary)
            {
                Parse(binary, path);
                return path;
            }

            return path;
            //throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
        }

        private MethodInfo GetDelegateMethodInfo(MethodCallExpression expression)
        {
            var target = expression.Object;
            var arguments = expression.Arguments;
            if (target != null)
            {
                if (target is ConstantExpression { Value: MethodInfo info })
                {
                    return info;
                }
            }
            else
            {
                foreach (var expr in arguments)
                {
                    if (expr is ConstantExpression { Value: MethodInfo info })
                    {
                        return info;
                    }
                }

                return null;
            }

            return null;
        }

        private void Parse(Expression expression, Path path)
        {
            if (expression == null || !(expression is MemberExpression || expression is MethodCallExpression || expression is BinaryExpression))
                return;

            if (expression is MemberExpression memberExpression)
            {
                var memberInfo = memberExpression.Member;
                if (memberInfo.IsStatic())
                {
                    path.Prepend(new MemberNode(memberInfo));
                    return;
                }

                path.Prepend(new MemberNode(memberInfo));
                if (memberExpression.Expression != null)
                {
                    Parse(memberExpression.Expression, path);
                }

                return;
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name.Equals("get_Item") && methodCallExpression.Arguments.Count == 1)
                {
                    var argument = methodCallExpression.Arguments[0];
                    if (argument is not ConstantExpression)
                        argument = ConvertMemberAccessToConstant(argument);

                    object value = ((ConstantExpression)argument).Value;
                    if (value is string s)
                    {
                        path.PrependIndexed(s);
                    }
                    else if (value is int i)
                    {
                        path.PrependIndexed(i);
                    }

                    if (methodCallExpression.Object != null)
                    {
                        Parse(methodCallExpression.Object, path);
                    }

                    return;
                }

                //Delegate.CreateDelegate(Type type, object firstArgument, MethodInfo method)
                if (methodCallExpression.Method.Name.Equals("CreateDelegate"))
                {
                    var info = GetDelegateMethodInfo(methodCallExpression);
                    if (info == null)
                        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

                    if (info.IsStatic)
                    {
                        path.Prepend(new MemberNode(info));
                        return;
                    }

                    path.Prepend(new MemberNode(info));
                    Parse(methodCallExpression.Arguments[1], path);
                    return;
                }

                if (methodCallExpression.Method.ReturnType == typeof(void))
                {
                    var info = methodCallExpression.Method;
                    if (info.IsStatic)
                    {
                        path.Prepend(new MemberNode(info));
                        return;
                    }

                    path.Prepend(new MemberNode(info));
                    if (methodCallExpression.Object != null)
                    {
                        Parse(methodCallExpression.Object, path);
                    }

                    return;
                }

                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
            }

            if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
                {
                    var left = binaryExpression.Left;
                    var right = binaryExpression.Right;
                    if (right is not ConstantExpression)
                    {
                        right = ConvertMemberAccessToConstant(right);
                    }

                    object value = ((ConstantExpression)right).Value;
                    if (value is string s)
                    {
                        path.PrependIndexed(s);
                    }
                    else if (value is int i)
                    {
                        path.PrependIndexed(i);
                    }

                    Parse(left, path);
                    return;
                }

                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
            }
        }

        private static Expression ConvertMemberAccessToConstant(Expression argument)
        {
            if (argument is ConstantExpression)
            {
                return argument;
            }

            var boxed = Expression.Convert(argument, typeof(object));
#if UNITY_IOS || ENABLE_IL2CPP
            var fun = (Func<object[], object>)Expression.Lambda<Func<object>>(boxed).DynamicCompile();
            var constant = fun(new object[] { });
#else
            var fun = Expression.Lambda<Func<object>>(boxed).Compile();
            var constant = fun();
#endif

            return Expression.Constant(constant);
        }

        public virtual Path ParseStaticPath(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            var current = expression.Body;
            if (current is UnaryExpression unary)
            {
                current = unary.Operand;
            }

            switch (current)
            {
                case MemberExpression:
                {
                    Path path = new Path();
                    Parse(current, path);
                    return path;
                }
                case MethodCallExpression:
                {
                    Path path = new Path();
                    Parse(current, path);
                    return path;
                }
                case BinaryExpression { NodeType: ExpressionType.ArrayIndex }:
                {
                    Path path = new Path();
                    Parse(current, path);
                    return path;
                }
                default:
                    throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
            }
        }

        public virtual Path ParseStaticPath(string pathText)
        {
            string typeName = ParserTypeName(pathText);
            string memberName = ParserMemberName(pathText);
            Type type = TypeFinderUtils.FindType(typeName);

            Path path = new Path();
            path.Append(new MemberNode(type, memberName, true));
            return path;
        }

        protected string ParserTypeName(string pathText)
        {
            if (pathText == null)
                throw new ArgumentNullException("pathText");

            pathText = pathText.Replace(" ", "");
            if (string.IsNullOrEmpty(pathText))
                throw new ArgumentException("The pathText is empty");

            int index = pathText.LastIndexOf('.');
            if (index <= 0)
                throw new ArgumentException("pathText");

            return pathText.Substring(0, index);
        }

        protected string ParserMemberName(string pathText)
        {
            if (pathText == null)
                throw new ArgumentNullException("pathText");

            pathText = pathText.Replace(" ", "");
            if (string.IsNullOrEmpty(pathText))
                throw new ArgumentException("The pathText is empty");

            int index = pathText.LastIndexOf('.');
            if (index <= 0)
                throw new ArgumentException("pathText");

            return pathText.Substring(index + 1);
        }

        public virtual string ParseMemberName(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return ParseMemberName0(expression.Body);
        }

        protected string ParseMemberName0(Expression expression)
        {
            if (expression == null || !(expression is MemberExpression || expression is MethodCallExpression || expression is UnaryExpression))
                return null;

            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name.Equals("get_Item") && methodCallExpression.Arguments.Count == 1)
                {
                    string temp = null;
                    var argument = methodCallExpression.Arguments[0];
                    if (argument is not ConstantExpression)
                        argument = ConvertMemberAccessToConstant(argument);

                    object value = ((ConstantExpression)argument).Value;
                    if (value is string strIndex)
                    {
                        temp = string.Format("[\"{0}\"]", strIndex);
                    }
                    else if (value is int intIndex)
                    {
                        temp = string.Format("[{0}]", intIndex);
                    }

                    var memberExpression = methodCallExpression.Object as MemberExpression;
                    if (memberExpression == null || !(memberExpression.Expression is ParameterExpression))
                        return temp;

                    return ParseMemberName0(memberExpression) + temp;
                }

                return methodCallExpression.Method.Name;
            }

            //Delegate.CreateDelegate(Type type, object firstArgument, MethodInfo method)
            //For<TTarget, TResult>(v => v.OnOpenLoginWindow); Support for method name parsing.
            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
            {
                if (unaryExpression.Operand is MethodCallExpression { Method: { Name: "CreateDelegate" } } methodCall)
                {
                    var info = GetDelegateMethodInfo(methodCall);
                    if (info != null)
                        return info.Name;
                }

                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
            }

            if (expression is not MemberExpression { Expression: ParameterExpression } body)
                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

            return body.Member.Name;
        }

        //public virtual string ParseMemberName(LambdaExpression expression)
        //{
        //    if (expression == null)
        //        throw new ArgumentNullException("expression");

        //    var method = expression.Body as MethodCallExpression;
        //    if (method != null)
        //        return method.Method.Name;

        //    //Delegate.CreateDelegate(Type type, object firstArgument, MethodInfo method)
        //    var unary = expression.Body as UnaryExpression;
        //    if (unary != null && unary.NodeType == ExpressionType.Convert)
        //    {
        //        MethodCallExpression methodCall = (MethodCallExpression)unary.Operand;
        //        if (methodCall.Method.Name.Equals("CreateDelegate"))
        //        {
        //            var info = GetDelegateMethodInfo(methodCall);
        //            if (info != null)
        //                return info.Name;
        //        }

        //        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
        //    }

        //    var body = expression.Body as MemberExpression;
        //    if (body == null)
        //        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

        //    if (!(body.Expression is ParameterExpression))
        //        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

        //    return body.Member.Name;
        //}
    }
}