// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine.SqlExpressions
{
    public class ColumnExpression : SqlExpression
    {
        private readonly IProperty _property;

        public ColumnExpression(IProperty property, TableExpressionBase table)
            : base(property.ClrType, property.FindRelationalMapping(), false)
        {
            _property = property;
            Table = table;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newTable = (TableExpressionBase)visitor.Visit(Table);

            return newTable != Table
                ? new ColumnExpression(_property, newTable)
                : this;
        }

        public string Name => _property.Relational().ColumnName;

        public TableExpressionBase Table { get; }
    }
}
