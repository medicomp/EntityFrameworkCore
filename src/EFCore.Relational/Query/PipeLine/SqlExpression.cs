// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public abstract class SqlExpression : Expression
    {
        private Type _type;

        protected SqlExpression(Type type, RelationalTypeMapping typeMapping, bool condition)
        {
            _type = type;
            IsCondition = condition;
            TypeMapping = typeMapping;
        }

        public SqlExpression MakeNonNullableType()
        {
            _type = _type.UnwrapNullableType();

            return this;
        }

        public SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
        {
            if (typeMapping == null)
            {
                throw new InvalidOperationException("Cannot assign null typeMapping.");
            }

            TypeMapping = typeMapping;

            return this;
        }

        public SqlExpression ApplyCondition(bool condition)
        {
            IsCondition = condition;

            return this;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _type;
        public bool IsCondition { get; private set; }
        public RelationalTypeMapping TypeMapping { get; private set; }
    }
}
