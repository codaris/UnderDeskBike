// <copyright file="ViewModelExtensions.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Codaris.Wpf
{
    /// <summary>
    /// Extension methods for view model.
    /// </summary>
    public static class ViewModelExtensions
    {
        /// <summary>
        /// Propagates the property changed event of another object.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        /// <param name="propertyLambda">The property lambda.</param>
        /// <param name="localPropertyLambdas">The local property lambdas.</param>
        public static void PropagatePropertyChanged<TSource, TDestination>(this TDestination destination, TSource source, Expression<Func<TSource, object>> propertyLambda, params Expression<Func<TDestination, object>>[] localPropertyLambdas)
            where TDestination : BaseViewModel
            where TSource : INotifyPropertyChanged
        {
            Contract.Requires(propertyLambda != null);
            Contract.Requires(destination != null);

            string propertyName = propertyLambda.GetMemberName();
            List<string> localProperties = new List<string>();
            foreach (var localPropertyLambda in localPropertyLambdas)
            {
                localProperties.Add(localPropertyLambda.GetMemberName());
            }
            destination.PropagatePropertyChanged(source, propertyName, localProperties.ToArray());
        }

        /// <summary>
        /// Handles the property changed event for the specified properties.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="propertyChangedHandler">The property changed handler.</param>
        /// <param name="propertyNames">The property names.</param>
        public static void HandlePropertyChanged(this INotifyPropertyChanged source, Action<string> propertyChangedHandler, params string[] propertyNames)
        {
            Contract.Requires(source != null);
            source.PropertyChanged += (sender, e) =>
            {
                if (propertyNames.Contains(e.PropertyName)) propertyChangedHandler(e.PropertyName);
            };
        }

        /// <summary>
        /// Handles the property changed for the specified properties.
        /// </summary>
        /// <typeparam name="T">The type of the source object.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyChangedHandler">The property changed handler.</param>
        /// <param name="propertyLambdas">The property lambdas.</param>
        public static void HandlePropertyChanged<T>(this T source, Action<string> propertyChangedHandler, params Expression<Func<T, object>>[] propertyLambdas)
            where T : INotifyPropertyChanged
        {
            source.HandlePropertyChanged(propertyChangedHandler, propertyLambdas.Select(p => p.GetMemberName()).ToArray());
        }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        /// <param name="memberSelector">The member selector.</param>
        /// <returns>The member name from the Lambda expression.</returns>
        private static string GetMemberName(this LambdaExpression memberSelector)
        {
            static string NameSelector(Expression e)
            {
                return e.NodeType switch
                {
                    ExpressionType.Parameter => ((ParameterExpression)e).Name,
                    ExpressionType.MemberAccess => ((MemberExpression)e).Member.Name,
                    ExpressionType.Call => ((MethodCallExpression)e).Method.Name,
                    ExpressionType.Convert or ExpressionType.ConvertChecked => NameSelector(((UnaryExpression)e).Operand),
                    ExpressionType.Invoke => NameSelector(((InvocationExpression)e).Expression),
                    ExpressionType.ArrayLength => "Length",
                    _ => throw new Exception("not a proper member selector"),
                };
            }

            return NameSelector(memberSelector.Body);
        }
    }
}
