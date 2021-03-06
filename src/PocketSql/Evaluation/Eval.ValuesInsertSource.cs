﻿using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(
            Table table,
            IList<ColumnReferenceExpression> cols,
            ValuesInsertSource values,
            IArgument arg,
            IOutputSink sink,
            Scope scope)
        {
            foreach (var valuesExpr in values.RowValues)
            {
                var row = table.NewRow(scope.Env);

                for (var i = 0; i < cols.Count; ++i)
                {
                    // TODO: take the last instead of the first? need more robust handling of multi-part names here
                    var name = cols[i].MultiPartIdentifier.Identifiers[0].Value;
                    row.SetValue(name, Evaluate(valuesExpr.ColumnValues[i], arg, scope));
                }

                sink.Inserted(row, scope.Env);
            }

            scope.Env.RowCount = values.RowValues.Count;
            return new EngineResult(values.RowValues.Count);
        }
    }
}
