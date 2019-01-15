// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.PipeLine
{
    public class TypeMappingApplyingExpressionVisitor : ITypeMappingApplyingExpressionVisitor
    {
        private readonly RelationalTypeMapping _boolTypeMapping;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public TypeMappingApplyingExpressionVisitor(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        public virtual SqlExpression ApplyTypeMapping(
            SqlExpression expression, RelationalTypeMapping typeMapping, bool condition = false)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.TypeMapping != null)
            {
                if (expression.IsCondition == condition)
                {
                    return expression;
                }

                return expression.ApplyCondition(condition);
            }

            switch (expression)
            {
                case SqlBinaryExpression sqlBinaryExpression:
                    return ApplyTypeMappingOnSqlBinary(sqlBinaryExpression, typeMapping, condition);

                case SqlConstantExpression sqlConstantExpression:
                    return ApplyTypeMappingOnSqlConstant(sqlConstantExpression, typeMapping, condition);

                case SqlFragmentExpression sqlFragmentExpression:
                    return sqlFragmentExpression;

                case SqlFunctionExpression sqlFunctionExpression:
                    return ApplyTypeMappingOnSqlFunction(sqlFunctionExpression, typeMapping, condition);

                case SqlParameterExpression sqlParameterExpression:
                    return ApplyTypeMappingOnSqlParameter(sqlParameterExpression, typeMapping, condition);

                case SqlUnaryExpression sqlUnaryExpression:
                    return ApplyTypeMappingOnSqlUnary(sqlUnaryExpression, typeMapping, condition);

                default:
                    return ApplyTypeMappingOnExtension(expression, typeMapping, condition);

            }
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlBinary(
            SqlBinaryExpression sqlBinaryExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            var left = sqlBinaryExpression.Left;
            var right = sqlBinaryExpression.Right;
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right);

            if (inferredTypeMapping == null)
            {
                if (left is SqlUnaryExpression leftUnary
                    && leftUnary.OperatorType == ExpressionType.Convert)
                {
                    inferredTypeMapping = _typeMappingSource.FindMapping(left.Type);
                }
                else if (right is SqlUnaryExpression rightUnary
                    && rightUnary.OperatorType == ExpressionType.Convert)
                {
                    inferredTypeMapping = _typeMappingSource.FindMapping(right.Type);
                }
                else
                {
                    return null;
                }
            }

            switch (sqlBinaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    {
                        if (sqlBinaryExpression.Type != typeof(bool))
                        {
                            throw new InvalidCastException("Comparison operation should be of type bool.");
                        }

                        left = ApplyTypeMapping(left, inferredTypeMapping, false);
                        right = ApplyTypeMapping(right, inferredTypeMapping, false);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping,
                            true);
                    }

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        left = ApplyTypeMapping(left, inferredTypeMapping, true);
                        right = ApplyTypeMapping(right, inferredTypeMapping, true);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping,
                            true);
                    }

                case ExpressionType.Add:
                    {
                        left = ApplyTypeMapping(left, inferredTypeMapping, false);
                        right = ApplyTypeMapping(right, inferredTypeMapping, false);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            sqlBinaryExpression.Type,
                            inferredTypeMapping,
                            false);
                    }
            }

            return null;
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlFunction(
            SqlFunctionExpression sqlFunctionExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            return sqlFunctionExpression;
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlParameter(
            SqlParameterExpression sqlParameterExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            if (typeMapping == null && condition)
            {
                typeMapping = _boolTypeMapping;
            }

            return sqlParameterExpression.ApplyTypeMapping(typeMapping).ApplyCondition(condition);
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlConstant(
            SqlConstantExpression sqlConstantExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            if (typeMapping == null && condition)
            {
                typeMapping = _boolTypeMapping;
            }

            return sqlConstantExpression.ApplyTypeMapping(typeMapping).ApplyCondition(condition);
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlUnary(
            SqlUnaryExpression sqlUnaryExpression, RelationalTypeMapping typeMapping, bool condition)
        {
            if (sqlUnaryExpression.OperatorType == ExpressionType.Convert
                && typeMapping != null)
            {
                var operand = ApplyTypeMapping(
                    sqlUnaryExpression.Operand,
                    _typeMappingSource.FindMapping(sqlUnaryExpression.Operand.Type),
                    false);

                return new SqlUnaryExpression(
                    sqlUnaryExpression.OperatorType,
                    operand,
                    sqlUnaryExpression.Type,
                    typeMapping,
                    condition);
            }

            return sqlUnaryExpression;
        }

        protected virtual SqlExpression ApplyTypeMappingOnExtension(
            SqlExpression expression, RelationalTypeMapping typeMapping, bool condition)
        {
            return expression;
        }
    }
}
