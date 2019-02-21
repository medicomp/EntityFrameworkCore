// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine.SqlExpressions
{
    public class SqlConstantExpression : SqlExpression
    {
        public SqlConstantExpression(ConstantExpression constantExpression, RelationalTypeMapping typeMapping, bool condition)
            : base(constantExpression.Type, typeMapping, condition)
        {
            Value = constantExpression.Value;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public object Value { get; }
    }
}
