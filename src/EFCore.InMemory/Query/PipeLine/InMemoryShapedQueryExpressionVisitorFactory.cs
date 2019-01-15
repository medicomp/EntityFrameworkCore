﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.PipeLine;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.PipeLine
{
    public class InMemoryShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public InMemoryShapedQueryCompilingExpressionVisitorFactory(IEntityMaterializerSource entityMaterializerSource)
        {
            _entityMaterializerSource = entityMaterializerSource;
        }

        public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext2 queryCompilationContext)
        {
            return new InMemoryShapedQueryCompilingExpressionVisitor(_entityMaterializerSource);
        }
    }

}
