// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class SqlUnaryExpression : SqlExpression
    {
        public SqlUnaryExpression(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping,
            bool condition)
            : base(type, typeMapping, condition)
        {
            Check.NotNull(operand, nameof(operand));

            OperatorType = operatorType;
            Operand = operand;
        }


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = (SqlExpression)visitor.Visit(Operand);

            return operand != Operand
                ? new SqlUnaryExpression(OperatorType, operand, Type, TypeMapping, IsCondition)
                : this;
        }

        public ExpressionType OperatorType { get; }
        public SqlExpression Operand { get; }
    }
}
