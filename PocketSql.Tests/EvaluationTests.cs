﻿using System;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    [TestFixture]
    public class EvaluationTests
    {
        [Test]
        public void CreateInsertSelect()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table People (Name varchar(32), Age int)");

                Assert.AreEqual(1, connection.Execute("insert into People (Name, Age) values ('Rob', 30)"));

                var people = connection.Query<Person>("select Name, Age from People").ToList();
                Assert.AreEqual(1, people.Count);
                Assert.AreEqual("Rob", people[0].Name);
                Assert.AreEqual(30, people[0].Age);

                Assert.AreEqual(1, connection.Execute("update People set Age += 1 where Name = @Name", new { Name = "Rob" }));

                people = connection.Query<Person>("select Name, Age from People where Name = 'Rob'").ToList();
                Assert.AreEqual(1, people.Count);
                Assert.AreEqual("Rob", people[0].Name);
                Assert.AreEqual(31, people[0].Age);
            }
        }

        [Test]
        public void InsertSelectOrdered()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table Things (X int, Y varchar(8))");

                Assert.AreEqual(16, connection.Execute(@"
                    insert into Things
                    (X, Y)
                    values
                    (34, 'qwe'),
                    (23, 'wer'),
                    (67, 'ert'),
                    (63, 'rty'),
                    (75, 'tyu'),
                    (17, 'yui'),
                    (83, 'uio'),
                    (47, 'iop'),
                    (95, 'asd'),
                    (36, 'sdf'),
                    (67, 'dfg'),
                    (23, 'fgh'),
                    (50, 'ghj'),
                    (17, 'hjk'),
                    (95, 'jkl'),
                    (92, 'zxc')"));

                Assert.IsTrue(new []
                {
                    new Thing(17, "yui"),
                    new Thing(17, "hjk"),
                    new Thing(23, "wer"),
                    new Thing(23, "fgh"),
                    new Thing(34, "qwe"),
                    new Thing(36, "sdf"),
                    new Thing(47, "iop"),
                    new Thing(50, "ghj"),
                    new Thing(63, "rty"),
                    new Thing(67, "ert"),
                    new Thing(67, "dfg"),
                    new Thing(75, "tyu"),
                    new Thing(83, "uio"),
                    new Thing(92, "zxc"),
                    new Thing(95, "jkl"),
                    new Thing(95, "asd")
                }.SequenceEqual(connection.Query<Thing>("select X, Y from Things order by X, Y desc")));
            }
        }

        private class Thing : IEquatable<Thing>
        {
            public Thing(int x, string y)
            {
                X = x;
                Y = y;
            }

            public int X { get; set; }
            public string Y { get; set; }

            public override bool Equals(object that) => that is Thing && Equals((Thing) that);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ (Y != null ? Y.GetHashCode() : 0);
                }
            }

            public bool Equals(Thing that) => X == that.X && Y == that.Y;
        }

        [Test]
        public void InsertSelectOffsetFetch()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table Numbers (X int)");

                Assert.AreEqual(8, connection.Execute(@"
                    insert into Numbers (X)
                    values (1), (2), (3), (4), (5), (6), (7), (8)"));

                Assert.IsTrue(new int?[] {3, 4, 5, 6}.SequenceEqual(connection.Query<int?>(@"
                    select X
                    from Numbers
                    order by X
                    offset 2 rows
                    fetch next 4 rows only")));
            }
        }

        [Test]
        public void InsertSelectDistinct()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table Things (X int, Y varchar(8))");

                Assert.AreEqual(16, connection.Execute(@"
                    insert into Things
                    (X, Y)
                    values
                    (34, 'qwe'),
                    (23, 'wer'),
                    (67, 'ert'),
                    (63, 'rty'),
                    (75, 'tyu'),
                    (17, 'yui'),
                    (47, 'uio'),
                    (47, 'zxc'),
                    (95, 'asd'),
                    (67, 'ert'),
                    (63, 'rty'),
                    (75, 'tyu'),
                    (95, 'asd'),
                    (17, 'yui'),
                    (23, 'wer'),
                    (92, 'zxc')"));

                Assert.AreEqual(10, connection.Query("select distinct * from Things").Count());
            }
        }

        [Test]
        public void InsertSelectIntoSelect()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table Things (X int, Y varchar(8))");

                Assert.AreEqual(16, connection.Execute(@"
                    insert into Things
                    (X, Y)
                    values
                    (34, 'qwe'),
                    (23, 'wer'),
                    (67, 'ert'),
                    (63, 'rty'),
                    (75, 'tyu'),
                    (17, 'yui'),
                    (47, 'uio'),
                    (47, 'zxc'),
                    (95, 'asd'),
                    (67, 'ert'),
                    (63, 'rty'),
                    (75, 'tyu'),
                    (95, 'asd'),
                    (17, 'yui'),
                    (23, 'wer'),
                    (92, 'zxc')"));

                Assert.AreEqual(16, connection.Execute("select * into Things2 from Things"));

                Assert.AreEqual(16, connection.Query("select * from Things2").Count());
            }
        }

        [Test]
        public void DeclareSetSelect()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                Assert.AreEqual("Rob", connection.ExecuteScalar<string>(@"
                    declare @Name varchar(16)
                    set @Name = 'Rob'
                    select @Name"));
            }
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
