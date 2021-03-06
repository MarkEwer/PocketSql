﻿using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static object Evaluate(AssignmentKind kind, object current, object value)
        {
            // TODO: use switch expression, expression bodied member
            switch (kind)
            {
                case AssignmentKind.Equals:
                    return value;
                case AssignmentKind.AddEquals:
                    // TODO: handle numeric conversions properly
                    // TODO: and how do nulls work?
                    if (current == null || value == null)
                        return current ?? value;
                    if (current is int i && value is int j)
                        return i + j;
                    return (decimal)current + (decimal)value;
                case AssignmentKind.SubtractEquals:
                    return (decimal)current - (decimal)value;
                case AssignmentKind.MultiplyEquals:
                    return (decimal)current * (decimal)value;
                case AssignmentKind.DivideEquals:
                    return (decimal)current / (decimal)value;
                case AssignmentKind.ModEquals:
                    return (decimal)current % (decimal)value;
                case AssignmentKind.BitwiseAndEquals:
                    return (int)current & (int)value;
                case AssignmentKind.BitwiseOrEquals:
                    return (int)current | (int)value;
                case AssignmentKind.BitwiseXorEquals:
                    return (int)current ^ (int)value;
                default:
                    throw FeatureNotSupportedException.Value(kind);
            }
        }
    }
}
