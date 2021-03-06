﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace PocketSql.Tests
{
    public class AsOfAttribute : NUnitAttribute, IParameterDataSource
    {
        private int Version { get; }

        public AsOfAttribute(int version) => Version = version;

        private static readonly Dictionary<Type, Func<int, object>> Builders = new Dictionary<Type, Func<int, object>>
        {
            { typeof(int), v => v },
            { typeof(Engine), v => new Engine(v) },
            { typeof(EngineConnection), v => new Engine(v).GetConnection() },
            { typeof(IDbConnection), v => new Engine(v).GetConnection() }
        };

        public IEnumerable GetData(IParameterInfo parameter) =>
            Builders.TryGetValue(parameter.ParameterType, out var builder)
                ? Enumerable.Range(Version, 15 - Version + 1).Select(builder)
                : throw new ArgumentException(
                    $"{nameof(AsOfAttribute)} can't be used on {parameter.ParameterType} parameter");
    }

    public class AllAttribute : AsOfAttribute
    {
        public AllAttribute() : base(8) { }
    }
}
