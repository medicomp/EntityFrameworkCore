// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine.SqlExpressions
{
    public class SqlParameterExpression : SqlExpression
    {
        public SqlParameterExpression(ParameterExpression parameterExpression, RelationalTypeMapping typeMapping, bool condition)
            : base(parameterExpression.Type, typeMapping, condition)
        {
            Name = parameterExpression.Name;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public string Name { get; }
    }
}
