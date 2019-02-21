// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine.SqlExpressions
{
    public class LikeExpression : SqlExpression
    {
        public LikeExpression(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar,
            Type type, RelationalTypeMapping typeMapping, bool condition)
            : base(type, typeMapping, condition)
        {
            Match = match;
            Pattern = pattern;
            EscapeChar = escapeChar;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var match = (SqlExpression)visitor.Visit(Match);
            var pattern = (SqlExpression)visitor.Visit(Pattern);
            var escapeChar = (SqlExpression)visitor.Visit(EscapeChar);

            return match != Match || pattern != Pattern || escapeChar != EscapeChar
                ? new LikeExpression(match, pattern, escapeChar, Type, TypeMapping, IsCondition)
                : this;
        }

        public SqlExpression Match { get; }
        public SqlExpression Pattern { get; }
        public SqlExpression EscapeChar { get; }
    }
}
