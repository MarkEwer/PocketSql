﻿using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static bool Evaluate(BooleanExpression expr, DataRow row, Env env)
        {
            switch (expr)
            {
                case BooleanBinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, row, env),
                        Evaluate(binaryExpr.SecondExpression, row, env));
                case BooleanComparisonExpression compareExpr:
                    return Evaluate(
                        compareExpr.ComparisonType,
                        Evaluate(compareExpr.FirstExpression, row, env),
                        Evaluate(compareExpr.SecondExpression, row, env));
                case BooleanNotExpression notExpr:
                    return !Evaluate(notExpr.Expression, row, env);
                case BooleanIsNullExpression isNullExpr:
                    return Evaluate(isNullExpr.Expression, row, env) == null;
                case InPredicate inExpr:
                    var value = Evaluate(inExpr.Expression, row, env);
                    return value != null && inExpr.Values.Any(x => value.Equals(Evaluate(x, row, env)));
                case ExistsPredicate existsExpr:
                    return Evaluate(existsExpr.Subquery.QueryExpression, env).ResultSet.Rows.Count > 0;
            }

            throw new NotImplementedException();
        }
    }
}