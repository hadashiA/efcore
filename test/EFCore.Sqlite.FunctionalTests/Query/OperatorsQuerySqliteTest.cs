// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class OperatorsQuerySqliteTest : OperatorsQueryTestBase
{
    public OperatorsQuerySqliteTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    public override async Task Regression_test1()
    {
        await base.Regression_test1();

        AssertSql("");
    }

    public override async Task Regression_test2()
    {
        await base.Regression_test2();

        AssertSql("");
    }

    public override async Task Regression_test3()
    {
        await base.Regression_test3();

        AssertSql("");
    }

    public override async Task Regression_test4()
    {
        await base.Regression_test4();

        AssertSql("");
    }
}
