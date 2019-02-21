// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.PipeLine.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public interface IMethodCallTranslatorProvider
    {
        SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments);
    }
}
