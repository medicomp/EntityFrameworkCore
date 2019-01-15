// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlBinaryExpression : SqlExpression
    {
        public SqlBinaryExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            Type type,
            RelationalTypeMapping typeMapping,
            bool condition)
            : base(type, typeMapping, condition)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            OperatorType = operatorType;
            Left = left;
            Right = right;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var left = (SqlExpression)visitor.Visit(Left);
            var right = (SqlExpression)visitor.Visit(Right);

            return left != Left || right != Right
                ? new SqlBinaryExpression(OperatorType, left, right, Type, TypeMapping, IsCondition)
                : this;
        }

        public ExpressionType OperatorType { get; }
        public SqlExpression Left { get; }
        public SqlExpression Right { get; }
    }
}
