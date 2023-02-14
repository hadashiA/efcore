// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OperatorsQueryTestBase : NonSharedModelTestBase
{
    protected readonly List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> Binaries;
    protected readonly List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> Unaries;
    protected readonly Dictionary<Type, Type> PropertyTypeToEntityMap;

    protected OperatorsData ExpectedData { get; init; }
    protected ExpectedQueryRewritingVisitor ExpectedQueryRewriter { get; init; }

    private static readonly MethodInfo _likeMethodInfo
        = typeof(DbFunctionsExtensions).GetRuntimeMethod(
            nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

    private static readonly MethodInfo _stringConcatMethodInfo
        = typeof(string).GetRuntimeMethod(
            nameof(string.Concat), new[] { typeof(string), typeof(string) });


    protected OperatorsQueryTestBase(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        Binaries = new()
        {
            ((typeof(string), typeof(string)), typeof(bool), Expression.Equal),
            ((typeof(string), typeof(string)), typeof(bool), Expression.NotEqual),
            ((typeof(string), typeof(string)), typeof(string), (x, y) => Expression.Add(x, y, _stringConcatMethodInfo)),
            ((typeof(string), typeof(string)), typeof(bool), (x, y) => Expression.Call(
                null,
                _likeMethodInfo,
                Expression.Constant(EF.Functions),
                x,
                y)),

            ((typeof(int), typeof(int)), typeof(int), Expression.Multiply),
            ((typeof(int), typeof(int)), typeof(int), Expression.Divide),
            ((typeof(int), typeof(int)), typeof(int), Expression.Modulo),
            ((typeof(int), typeof(int)), typeof(int), Expression.Add),
            ((typeof(int), typeof(int)), typeof(int), Expression.Subtract),
            ((typeof(int), typeof(int)), typeof(bool), Expression.Equal),
            ((typeof(int), typeof(int)), typeof(bool), Expression.NotEqual),
            ((typeof(int), typeof(int)), typeof(bool), Expression.LessThan),
            ((typeof(int), typeof(int)), typeof(bool), Expression.LessThanOrEqual),
            ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThan),
            ((typeof(int), typeof(int)), typeof(bool), Expression.GreaterThanOrEqual),
            ((typeof(int), typeof(int)), typeof(int), Expression.And),
            ((typeof(int), typeof(int)), typeof(int), Expression.Or),
            //((typeof(int), typeof(int)), typeof(int), Expression.LeftShift),
            //((typeof(int), typeof(int)), typeof(int), Expression.RightShift),

            ((typeof(long), typeof(long)), typeof(long), Expression.Multiply),
            ((typeof(long), typeof(long)), typeof(long), Expression.Divide),
            ((typeof(long), typeof(long)), typeof(long), Expression.Modulo),
            ((typeof(long), typeof(long)), typeof(long), Expression.Add),
            ((typeof(long), typeof(long)), typeof(long), Expression.Subtract),
            ((typeof(long), typeof(long)), typeof(bool), Expression.Equal),
            ((typeof(long), typeof(long)), typeof(bool), Expression.NotEqual),
            ((typeof(long), typeof(long)), typeof(bool), Expression.LessThan),
            ((typeof(long), typeof(long)), typeof(bool), Expression.LessThanOrEqual),
            ((typeof(long), typeof(long)), typeof(bool), Expression.GreaterThan),
            ((typeof(long), typeof(long)), typeof(bool), Expression.GreaterThanOrEqual),
            ((typeof(long), typeof(long)), typeof(long), Expression.And),
            ((typeof(long), typeof(long)), typeof(long), Expression.Or),
            //((typeof(long), typeof(long)), typeof(long), Expression.LeftShift),
            //((typeof(long), typeof(long)), typeof(long), Expression.RightShift),

            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Equal),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.NotEqual),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.AndAlso),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.OrElse),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.And),
            ((typeof(bool), typeof(bool)), typeof(bool), Expression.Or),
        };

        Unaries = new()
        {
            (typeof(string), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(string)))),
            (typeof(string), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(string)))),
            (typeof(string), typeof(bool), x => Expression.Call(
                null,
                _likeMethodInfo,
                Expression.Constant(EF.Functions),
                x,
                Expression.Constant("A%"))),
            (typeof(string), typeof(bool), x => Expression.Call(
                null,
                _likeMethodInfo,
                Expression.Constant(EF.Functions),
                x,
                Expression.Constant("%B"))),

            (typeof(int), typeof(int), Expression.Not),
            (typeof(int), typeof(int), Expression.Negate),
            (typeof(int), typeof(long), x => Expression.Convert(x, typeof(long))),

            (typeof(int?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(int?)))),
            (typeof(int?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(int?)))),

            (typeof(long), typeof(long), Expression.Not),
            (typeof(long), typeof(long), Expression.Negate),
            (typeof(long), typeof(int), x => Expression.Convert(x, typeof(int))),

            (typeof(bool), typeof(bool), Expression.Not),

            (typeof(bool?), typeof(bool), x => Expression.Equal(x, Expression.Constant(null, typeof(bool?)))),
            (typeof(bool?), typeof(bool), x => Expression.NotEqual(x, Expression.Constant(null, typeof(bool?)))),
        };

        PropertyTypeToEntityMap = new()
        {
            { typeof(string), typeof(OperatorEntityString) },
            { typeof(int), typeof(OperatorEntityInt) },
            { typeof(int?), typeof(OperatorEntityNullableInt) },
            { typeof(long), typeof(OperatorEntityLong) },
            { typeof(bool), typeof(OperatorEntityBool) },
            { typeof(bool?), typeof(OperatorEntityNullableBool) },
            { typeof(DateTimeOffset), typeof(OperatorEntityDateTimeOffset) },
        };

        ExpectedData = OperatorsData.Instance;
        ExpectedQueryRewriter = new ExpectedQueryRewritingVisitor();
    }

    protected override string StoreName
        => "OperatorsTest";

    protected virtual void Seed(OperatorsContext ctx)
    {
        ctx.Set<OperatorEntityString>().AddRange(ExpectedData.OperatorEntitiesString);
        ctx.Set<OperatorEntityInt>().AddRange(ExpectedData.OperatorEntitiesInt);
        ctx.Set<OperatorEntityNullableInt>().AddRange(ExpectedData.OperatorEntitiesNullableInt);
        ctx.Set<OperatorEntityLong>().AddRange(ExpectedData.OperatorEntitiesLong);
        ctx.Set<OperatorEntityBool>().AddRange(ExpectedData.OperatorEntitiesBool);
        ctx.Set<OperatorEntityNullableBool>().AddRange(ExpectedData.OperatorEntitiesNullableBool);
        ctx.Set<OperatorEntityDateTimeOffset>().AddRange(ExpectedData.OperatorEntitiesDateTimeOffset);

        ctx.SaveChanges();
    }

    [ConditionalFact(Skip = "issue #30245")]
    public virtual async Task Regression_test1()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from o1 in ExpectedData.OperatorEntitiesString
                        from o2 in ExpectedData.OperatorEntitiesString
                        from o3 in ExpectedData.OperatorEntitiesBool
                        where ((o2.Value == "B" || o3.Value) & (o1.Value != null)) != false
                        select new { Value1 = o1.Value, Value2 = o2.Value, Value3 = o3.Value }).ToList();

        var actual = (from o1 in context.Set<OperatorEntityString>()
                      from o2 in context.Set<OperatorEntityString>()
                      from o3 in context.Set<OperatorEntityBool>()
                      where ((EF.Functions.Like(o2.Value, "B") || o3.Value) & (o1.Value != null)) != false
                      select new { Value1 = o1.Value, Value2 = o2.Value, Value3 = o3.Value }).ToList();

        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Value1, actual[i].Value1);
            Assert.Equal(expected[i].Value2, actual[i].Value2);
            Assert.Equal(expected[i].Value3, actual[i].Value3);
        }
    }

    [ConditionalFact(Skip = "issue #30248")]
    public virtual async Task Regression_test2()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from e0 in ExpectedData.OperatorEntitiesLong
                        from e1 in ExpectedData.OperatorEntitiesLong
                        from e2 in ExpectedData.OperatorEntitiesLong
                        from e3 in ExpectedData.OperatorEntitiesLong
                        where ((((e1.Value % 2) / e0.Value) & (((e3.Value | e2.Value) - e0.Value) - (e2.Value * e2.Value))) >= (((e1.Value / ~(e3.Value)) % (long)((1 + 1))) % (~(e0.Value) + 1)))
                        select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value, Value3 = e3.Value }).ToList();

        var actual = (from e0 in context.Set<OperatorEntityLong>()
                      from e1 in context.Set<OperatorEntityLong>()
                      from e2 in context.Set<OperatorEntityLong>()
                      from e3 in context.Set<OperatorEntityLong>()
                      where ((((e1.Value % 2) / e0.Value) & (((e3.Value | e2.Value) - e0.Value) - (e2.Value * e2.Value))) >= (((e1.Value / ~(e3.Value)) % (long)((1 + 1))) % (~(e0.Value) + 1)))
                      select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value, Value3 = e3.Value }).ToList();


        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Value0, actual[i].Value0);
            Assert.Equal(expected[i].Value1, actual[i].Value1);
            Assert.Equal(expected[i].Value2, actual[i].Value2);
            Assert.Equal(expected[i].Value3, actual[i].Value3);
        }
    }

    [ConditionalFact(Skip = "issue #30248")]
    public virtual async Task Regression_test3()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from e0 in ExpectedData.OperatorEntitiesInt
                        from e1 in ExpectedData.OperatorEntitiesInt
                        from e2 in ExpectedData.OperatorEntitiesBool
                        where (((((e1.Value & (e0.Value + e0.Value)) & e0.Value) / 1) > (e1.Value & (int)((8 + 2)))) && e2.Value)
                        select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value }).ToList();

        var actual = (from e0 in context.Set<OperatorEntityInt>()
                      from e1 in context.Set<OperatorEntityInt>()
                      from e2 in context.Set<OperatorEntityBool>()
                      where (((((e1.Value & (e0.Value + e0.Value)) & e0.Value) / 1) > (e1.Value & (int)((8 + 2)))) && e2.Value)
                      select new { Value0 = e0.Value, Value1 = e1.Value, Value2 = e2.Value }).ToList();


        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Value0, actual[i].Value0);
            Assert.Equal(expected[i].Value1, actual[i].Value1);
            Assert.Equal(expected[i].Value2, actual[i].Value2);
        }
    }

    [ConditionalFact(Skip = "issue #30277")]
    public virtual async Task Regression_test4()
    {
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using var context = contextFactory.CreateContext();

        var expected = (from e3 in ExpectedData.OperatorEntitiesLong
                        from e4 in ExpectedData.OperatorEntitiesLong
                        from e5 in ExpectedData.OperatorEntitiesLong
                        orderby e3.Id, e4.Id, e5.Id
                        select ((~(-(-((e5.Value + e3.Value) + 2))) % (-(e4.Value + e4.Value) - e3.Value)))).ToList();

        var actual = (from e3 in context.Set<OperatorEntityLong>()
                      from e4 in context.Set<OperatorEntityLong>()
                      from e5 in context.Set<OperatorEntityLong>()
                      orderby e3.Id, e4.Id, e5.Id
                      select ((~(-(-((e5.Value + e3.Value) + 2))) % (-(e4.Value + e4.Value) - e3.Value)))).ToList();
       
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }

    //[ConditionalFact]
    public virtual async Task Procedural_predicate_test_six_sources_three_pairs()
    {
        var maxDepth = 7;
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);

            while (true)
            {
                var seed = new Random().Next();
                var random = new Random(seed);

                var possibleTypes = OperatorsData.Instance.ConstantExpressionsPerType.Keys.ToArray();

                var typesUsed = new bool[6];
                var types = new Type[6];
                for (var i = 0; i < types.Length; i++)
                {
                    types[i] = possibleTypes[random.Next(possibleTypes.Length)];
                    types[i + 1] = types[i];
                    i++;
                }

                // dummy input expression and whether is has already been used
                // (we want to prioritize ones that haven't been used yet, so that generated expressions are more interesting)
                var rootEntityExpressions = types.Select((x, i) => new RootEntityExpressionInfo(
                    Expression.Property(
                        Expression.Parameter(PropertyTypeToEntityMap[x], "e" + i),
                        "Value"))).ToArray();

                var testExpression = GenerateTestExpression(
                    random,
                    types,
                    rootEntityExpressions,
                    maxDepth,
                    startingResultType: typeof(bool));

                var roots = rootEntityExpressions.Where(x => x.Used).Select(x => x.Expression).ToArray();
                TestPredicateQuery(
                    seed,
                    actualSetSource,
                    roots,
                    testExpression);
            }
        }
    }

    //[ConditionalFact]
    public virtual async Task Procedural_projection_test_six_sources_two_trios()
    {
        var maxDepth = 7;
        var contextFactory = await InitializeAsync<OperatorsContext>(seed: Seed);
        using (var context = contextFactory.CreateContext())
        {
            var actualSetSource = new ActualSetSource(context);

            while (true)
            {
                var seed = new Random().Next();
                var random = new Random(seed);

                var possibleTypes = OperatorsData.Instance.ConstantExpressionsPerType.Keys.ToArray();

                var typesUsed = new bool[6];
                var types = new Type[6];
                for (var i = 0; i < types.Length; i++)
                {
                    types[i] = possibleTypes[random.Next(possibleTypes.Length)];
                    types[i + 1] = types[i];
                    types[i + 2] = types[i];
                    i += 2;
                }

                // dummy input expression and whether is has already been used
                // (we want to prioritize ones that haven't been used yet, so that generated expressions are more interesting)
                var rootEntityExpressions = types.Select((x, i) => new RootEntityExpressionInfo(
                    Expression.Property(
                        Expression.Parameter(PropertyTypeToEntityMap[x], "e" + i),
                        "Value"))).ToArray();

                var testExpression = GenerateTestExpression(
                    random,
                    types,
                    rootEntityExpressions,
                    maxDepth,
                    startingResultType: null);

                var roots = rootEntityExpressions.Where(x => x.Used).Select(x => x.Expression).ToArray();
                TestProjectionQuery(
                    seed,
                    actualSetSource,
                    roots,
                    testExpression);
            }
        }
    }

    #region test expression generation

    private Expression GenerateTestExpression(
        Random random,
        Type[] types,
        RootEntityExpressionInfo[] rootEntityExpressions,
        int maxDepth,
        Type startingResultType)
    {
        var distinctTypes = types.Distinct().ToList();
        var possibleLeafBinaries = Binaries.Where(x => distinctTypes.Contains(x.InputTypes.Item1) && distinctTypes.Contains(x.InputTypes.Item2)).ToList();
        var possibleLeafUnaries = Unaries.Where(x => distinctTypes.Contains(x.InputType)).ToList();

        // we assume one level of nesting is enough to get to all possible operations
        // this should be true, since all operations either result in bool or the same type as input
        // only exception being convert, which needs one step to get to all possible options: long -> int, or int -> long
        var distinctTypesWithNesting = distinctTypes
            .Concat(possibleLeafBinaries.Select(x => x.ResultType))
            .Concat(possibleLeafUnaries.Select(x => x.ResultType))
            .Distinct()
            .ToList();

        var possibleBinaries = Binaries.Where(x => distinctTypesWithNesting.Contains(x.InputTypes.Item1) && distinctTypesWithNesting.Contains(x.InputTypes.Item2)).ToList();
        var possibleUnaries = Unaries.Where(x => distinctTypesWithNesting.Contains(x.InputType)).ToList();

        var currentDepth = 0;
        var currentResultType = startingResultType
            ?? distinctTypesWithNesting[random.Next(distinctTypesWithNesting.Count)];

        var testExpression = MainLoop(
            random,
            currentResultType,
            currentDepth,
            maxDepth,
            types,
            rootEntityExpressions,
            possibleBinaries,
            possibleUnaries);

        return testExpression;
    }

    private Expression MainLoop(
        Random random,
        Type currentResultType,
        int currentDepth,
        int maxDepth,
        Type[] types,
        RootEntityExpressionInfo[] rootPropertyExpressions,
        List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> possibleBinaries,
        List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> possibleUnaries)
    {
        // see if we want additional level of nesting, the deeper we go the lower the probability
        // we also force nesting if we end up with an expected node that we don't have the root entity for
        // this can happen when we use convert - e.g. we only have int sources, but we expect long
        var rollAddDepth = random.Next(maxDepth);
        if (rollAddDepth >= currentDepth)
        {
            var possibleBinariesForResultType = possibleBinaries.Where(x => x.ResultType == currentResultType).ToList();
            var possibleUnariesForResultType = possibleUnaries.Where(x => x.ResultType == currentResultType).ToList();

            // if we can't go any deeper (no matching operations) then simply return source 
            if (possibleBinariesForResultType.Count == 0 && possibleUnariesForResultType.Count == 0)
            {
                return AddRootPropertyAccess(random, currentResultType, rootPropertyExpressions);
            }

            var operationIndex = random.Next(possibleBinariesForResultType.Count + possibleUnariesForResultType.Count);
            if (operationIndex < possibleBinariesForResultType.Count)
            {
                var operation = possibleBinariesForResultType[operationIndex];
                return AddBinaryOperation(
                    random,
                    currentDepth,
                    maxDepth,
                    operation,
                    types,
                    rootPropertyExpressions,
                    possibleBinaries,
                    possibleUnaries);
            }
            else
            {
                var operation = possibleUnariesForResultType[operationIndex - possibleBinariesForResultType.Count];
                return AddUnaryOperation(
                    random,
                    currentDepth,
                    maxDepth,
                    operation,
                    types,
                    rootPropertyExpressions,
                    possibleBinaries,
                    possibleUnaries);
            }
        }
        else
        {
            return AddRootPropertyAccess(random, currentResultType, rootPropertyExpressions);
        }
    }

    private Expression AddRootPropertyAccess(
        Random random,
        Type currentResultType,
        RootEntityExpressionInfo[] rootEntityExpressions)
    {
        // just pick a source, prioritize sources that were not used yet
        var matchingExpressions = rootEntityExpressions.Where(x => x.Expression.Type == currentResultType).ToList();

        // if we want to break, but don't we don't have any roots that match the criteria just return a constant
        // to simplify the logic here. Otherwise we can get stuck for a long time looking for the correct souce
        // deeper and deeper
        if (matchingExpressions.Count == 0)
        {
            var constants = OperatorsData.Instance.ConstantExpressionsPerType[currentResultType];

            return constants[random.Next(constants.Count)];
        }

        var unusedExpressions = matchingExpressions.Where(x => !x.Used).ToList();
        if (unusedExpressions.Any())
        {
            var chosenExpresion = unusedExpressions[random.Next(unusedExpressions.Count)];
            chosenExpresion.Used = true;

            return chosenExpresion.Expression;
        }
        else
        {
            return matchingExpressions[random.Next(matchingExpressions.Count)].Expression;
        }
    }

    private Expression AddBinaryOperation(
        Random random,
        int currentDepth,
        int maxDepth,
        ((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator) operation,
        Type[] types,
        RootEntityExpressionInfo[] rootPropertyExpressions,
        List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> possibleBinaries,
        List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> possibleUnaries)
    {
        currentDepth++;
        var left = MainLoop(
            random,
            operation.InputTypes.Item1,
            currentDepth,
            maxDepth,
            types,
            rootPropertyExpressions,
            possibleBinaries,
            possibleUnaries);

        Expression right;
        var rollFakeBinary = random.Next(3);
        if (rollFakeBinary > 1)
        {
            var constants = OperatorsData.Instance.ConstantExpressionsPerType[operation.InputTypes.Item2];
            right = constants.Skip(random.Next(constants.Count)).First();
        }
        else
        {
            right = MainLoop(
                random,
                operation.InputTypes.Item2,
                currentDepth,
                maxDepth,
                types,
                rootPropertyExpressions,
                possibleBinaries,
                possibleUnaries);
        }

        return operation.OperatorCreator(left, right);
    }

    private Expression AddUnaryOperation(
        Random random,
        int currentDepth,
        int maxDepth,
        (Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator) operation,
        Type[] types,
        RootEntityExpressionInfo[] rootPropertyExpressions,
        List<((Type, Type) InputTypes, Type ResultType, Func<Expression, Expression, Expression> OperatorCreator)> possibleBinaries,
        List<(Type InputType, Type ResultType, Func<Expression, Expression> OperatorCreator)> possibleUnaries)
    {
        currentDepth++;
        var source = MainLoop(
            random,
            operation.InputType,
            currentDepth,
            maxDepth,
            types,
            rootPropertyExpressions,
            possibleBinaries,
            possibleUnaries);

        return operation.OperatorCreator(source);
    }

    #endregion

    #region projection

    private void TestProjectionQuery(
        int seed,
        ISetSource actualSetSource,
        Expression[] roots,
        Expression resultExpression)
    {
        // if we end up not using any sources
        // this can happen when we don't have any viable operations to perform for a given type
        // but we have gone to max depth so we can't go any deeper - we end up returning a constant
        // if that happens for every leaf, we end up with no sources
        if (roots.Length == 0)
        {
            return;
        }

        var methodName = roots.Length switch
        {
            1 => nameof(TestProjectionQueryWithOneSource),
            2 => nameof(TestProjectionQueryWithTwoSources),
            3 => nameof(TestProjectionQueryWithThreeSources),
            4 => nameof(TestProjectionQueryWithFourSources),
            5 => nameof(TestProjectionQueryWithFiveSources),
            6 => nameof(TestProjectionQueryWithSixSources),
            _ => throw new InvalidOperationException(),
        };

        var method = typeof(OperatorsQueryTestBase).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericArguments = roots.Select(x => PropertyTypeToEntityMap[x.Type]).Concat(new[] { resultExpression.Type }).ToArray();
        var genericMethod = method.MakeGenericMethod(genericArguments);

        var resultRewriter = new ResultExpressionProjectionRewriter(resultExpression, roots);

        genericMethod.Invoke(
            this,
            new object[]
            {
                seed,
                actualSetSource,
                resultRewriter
            });
    }

    private void TestProjectionQueryWithOneSource<TEntity1, TResult>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            orderby e1.Id
            select new OperatorDto1<TEntity1, TResult>(e1, default);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Entity1.Id, a[i].Entity1.Id);
                Assert.Equal(e[i].Result, a[i].Result);
            });
    }

    private void TestProjectionQueryWithTwoSources<TEntity1, TEntity2, TResult>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            orderby e1.Id, e2.Id
            select new OperatorDto2<TEntity1, TEntity2, TResult>(e1, e2, default);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Entity1.Id, a[i].Entity1.Id);
                Assert.Equal(e[i].Entity2.Id, a[i].Entity2.Id);
                Assert.Equal(e[i].Result, a[i].Result);
            });
    }

    private void TestProjectionQueryWithThreeSources<TEntity1, TEntity2, TEntity3, TResult>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            orderby e1.Id, e2.Id, e3.Id
            select new OperatorDto3<TEntity1, TEntity2, TEntity3, TResult>(e1, e2, e3, default);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Entity1.Id, a[i].Entity1.Id);
                Assert.Equal(e[i].Entity2.Id, a[i].Entity2.Id);
                Assert.Equal(e[i].Entity3.Id, a[i].Entity3.Id);
                Assert.Equal(e[i].Result, a[i].Result);
            });
    }

    private void TestProjectionQueryWithFourSources<TEntity1, TEntity2, TEntity3, TEntity4, TResult>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id
            select new OperatorDto4<TEntity1, TEntity2, TEntity3, TEntity4, TResult>(e1, e2, e3, e4, default);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Entity1.Id, a[i].Entity1.Id);
                Assert.Equal(e[i].Entity2.Id, a[i].Entity2.Id);
                Assert.Equal(e[i].Entity3.Id, a[i].Entity3.Id);
                Assert.Equal(e[i].Entity4.Id, a[i].Entity4.Id);
                Assert.Equal(e[i].Result, a[i].Result);
            });
    }

    private void TestProjectionQueryWithFiveSources<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TResult>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            from e5 in ss.Set<TEntity5>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id, e5.Id
            select new OperatorDto5<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TResult>(e1, e2, e3, e4, e5, default);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Entity1.Id, a[i].Entity1.Id);
                Assert.Equal(e[i].Entity2.Id, a[i].Entity2.Id);
                Assert.Equal(e[i].Entity3.Id, a[i].Entity3.Id);
                Assert.Equal(e[i].Entity4.Id, a[i].Entity4.Id);
                Assert.Equal(e[i].Entity5.Id, a[i].Entity5.Id);
                Assert.Equal(e[i].Result, a[i].Result);
            });
    }

    private void TestProjectionQueryWithSixSources<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
        where TEntity6 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            from e5 in ss.Set<TEntity5>()
            from e6 in ss.Set<TEntity6>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id, e5.Id, e6.Id
            select new OperatorDto6<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>(e1, e2, e3, e4, e5, e6, default);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Entity1.Id, a[i].Entity1.Id);
                Assert.Equal(e[i].Entity2.Id, a[i].Entity2.Id);
                Assert.Equal(e[i].Entity3.Id, a[i].Entity3.Id);
                Assert.Equal(e[i].Entity4.Id, a[i].Entity4.Id);
                Assert.Equal(e[i].Entity5.Id, a[i].Entity5.Id);
                Assert.Equal(e[i].Entity6.Id, a[i].Entity6.Id);
                Assert.Equal(e[i].Result, a[i].Result);
            });
    }

    private class ResultExpressionProjectionRewriter : ExpressionVisitor
    {
        private readonly Expression[] _roots;
        private readonly Expression _resultExpression;

        public ResultExpressionProjectionRewriter(Expression resultExpression, Expression[] roots)
        {
            _resultExpression = resultExpression;
            _roots = roots;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            if (newExpression.Constructor is ConstructorInfo ctorInfo
                && ctorInfo.DeclaringType is Type { IsGenericType: true } declaringType)
            {
                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto1<,>))
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(newExpression.Arguments[0], "Value"),
                        }).Visit(_resultExpression);

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        replaced
                    };

                    return newExpression.Update(newArgs);
                }

                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto2<,,>))
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(newExpression.Arguments[0], "Value"),
                            Expression.Property(newExpression.Arguments[1], "Value"),
                        }).Visit(_resultExpression);

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        replaced
                    };

                    return newExpression.Update(newArgs);
                }

                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto3<,,,>))
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(newExpression.Arguments[0], "Value"),
                            Expression.Property(newExpression.Arguments[1], "Value"),
                            Expression.Property(newExpression.Arguments[2], "Value"),
                        }).Visit(_resultExpression);

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        newExpression.Arguments[2],
                        replaced
                    };

                    return newExpression.Update(newArgs);
                }

                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto4<,,,,>))
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(newExpression.Arguments[0], "Value"),
                            Expression.Property(newExpression.Arguments[1], "Value"),
                            Expression.Property(newExpression.Arguments[2], "Value"),
                            Expression.Property(newExpression.Arguments[3], "Value"),
                        }).Visit(_resultExpression);

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        newExpression.Arguments[2],
                        newExpression.Arguments[3],
                        replaced
                    };

                    return newExpression.Update(newArgs);
                }

                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto5<,,,,,>))
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(newExpression.Arguments[0], "Value"),
                            Expression.Property(newExpression.Arguments[1], "Value"),
                            Expression.Property(newExpression.Arguments[2], "Value"),
                            Expression.Property(newExpression.Arguments[3], "Value"),
                            Expression.Property(newExpression.Arguments[4], "Value"),
                        }).Visit(_resultExpression);

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        newExpression.Arguments[2],
                        newExpression.Arguments[3],
                        newExpression.Arguments[4],
                        replaced
                    };

                    return newExpression.Update(newArgs);
                }

                if (declaringType.GetGenericTypeDefinition() == typeof(OperatorDto6<,,,,,,>))
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(newExpression.Arguments[0], "Value"),
                            Expression.Property(newExpression.Arguments[1], "Value"),
                            Expression.Property(newExpression.Arguments[2], "Value"),
                            Expression.Property(newExpression.Arguments[3], "Value"),
                            Expression.Property(newExpression.Arguments[4], "Value"),
                            Expression.Property(newExpression.Arguments[5], "Value"),
                        }).Visit(_resultExpression);

                    var newArgs = new List<Expression>
                    {
                        newExpression.Arguments[0],
                        newExpression.Arguments[1],
                        newExpression.Arguments[2],
                        newExpression.Arguments[3],
                        newExpression.Arguments[4],
                        newExpression.Arguments[5],
                        replaced
                    };

                    return newExpression.Update(newArgs);
                }
            }

            return base.VisitNew(newExpression);
        }
    }

    public class OperatorDto1<TEntity1, TResult>
        where TEntity1 : OperatorEntityBase
    {
        public OperatorDto1(TEntity1 entity1, TResult result)
        {
            Entity1 = entity1;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }

        public TResult Result { get; set; }
    }

    public class OperatorDto2<TEntity1, TEntity2, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
    {
        public OperatorDto2(TEntity1 entity1, TEntity2 entity2, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }

        public TResult Result { get; set; }
    }

    public class OperatorDto3<TEntity1, TEntity2, TEntity3, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
    {
        public OperatorDto3(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Entity3 = entity3;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }
        public TEntity3 Entity3 { get; set; }

        public TResult Result { get; set; }
    }

    public class OperatorDto4<TEntity1, TEntity2, TEntity3, TEntity4, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
    {
        public OperatorDto4(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Entity3 = entity3;
            Entity4 = entity4;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }
        public TEntity3 Entity3 { get; set; }
        public TEntity4 Entity4 { get; set; }

        public TResult Result { get; set; }
    }

    public class OperatorDto5<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
    {
        public OperatorDto5(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4, TEntity5 entity5, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Entity3 = entity3;
            Entity4 = entity4;
            Entity5 = entity5;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }
        public TEntity3 Entity3 { get; set; }
        public TEntity4 Entity4 { get; set; }
        public TEntity5 Entity5 { get; set; }

        public TResult Result { get; set; }
    }

    public class OperatorDto6<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6, TResult>
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
        where TEntity6 : OperatorEntityBase
    {
        public OperatorDto6(TEntity1 entity1, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4, TEntity5 entity5, TEntity6 entity6, TResult result)
        {
            Entity1 = entity1;
            Entity2 = entity2;
            Entity3 = entity3;
            Entity4 = entity4;
            Entity5 = entity5;
            Entity6 = entity6;
            Result = result;
        }

        public TEntity1 Entity1 { get; set; }
        public TEntity2 Entity2 { get; set; }
        public TEntity3 Entity3 { get; set; }
        public TEntity4 Entity4 { get; set; }
        public TEntity5 Entity5 { get; set; }
        public TEntity6 Entity6 { get; set; }

        public TResult Result { get; set; }
    }

    #endregion

    #region predicate

    private void TestPredicateQuery(
        int seed,
        ISetSource actualSetSource,
        Expression[] roots,
        Expression resultExpression)
    {
        // if we end up not using any sources
        // this can happen when we don't have any viable operations to perform for a given type
        // but we have gone to max depth so we can't go any deeper - we end up returning a constant
        // if that happens for every leaf, we end up with no sources
        if (roots.Length == 0)
        {
            return;
        }

        var methodName = roots.Length switch
        {
            1 => nameof(TestPredicateQueryWithOneSource),
            2 => nameof(TestPredicateQueryWithTwoSources),
            3 => nameof(TestPredicateQueryWithThreeSources),
            4 => nameof(TestPredicateQueryWithFourSources),
            5 => nameof(TestPredicateQueryWithFiveSources),
            6 => nameof(TestPredicateQueryWithSixSources),
            _ => throw new InvalidOperationException(),
        };

        var method = typeof(OperatorsQueryTestBase).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        var genericMethod = method.MakeGenericMethod(roots.Select(x => PropertyTypeToEntityMap[x.Type]).ToArray());

        var resultRewriter = new ResultExpressionPredicateRewriter(resultExpression, roots);

        genericMethod.Invoke(
            this,
            new object[]
            {
                seed,
                actualSetSource,
                resultRewriter
            });
    }

    private void TestPredicateQueryWithOneSource<TEntity1>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            orderby e1.Id
            where DummyTrue(e1)
            select new ValueTuple<TEntity1>(e1);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Item1.Id, a[i].Item1.Id);
            });
    }

    private void TestPredicateQueryWithTwoSources<TEntity1, TEntity2>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            orderby e1.Id, e2.Id
            where DummyTrue(e1, e2)
            select new ValueTuple<TEntity1, TEntity2>(e1, e2);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Item1.Id, a[i].Item1.Id);
                Assert.Equal(e[i].Item2.Id, a[i].Item2.Id);
            });
    }

    private void TestPredicateQueryWithThreeSources<TEntity1, TEntity2, TEntity3>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            orderby e1.Id, e2.Id, e3.Id
            where DummyTrue(e1, e2, e3)
            select new ValueTuple<TEntity1, TEntity2, TEntity3>(e1, e2, e3);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Item1.Id, a[i].Item1.Id);
                Assert.Equal(e[i].Item2.Id, a[i].Item2.Id);
                Assert.Equal(e[i].Item3.Id, a[i].Item3.Id);
            });
    }

    private void TestPredicateQueryWithFourSources<TEntity1, TEntity2, TEntity3, TEntity4>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id
            where DummyTrue(e1, e2, e3, e4)
            select new ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4>(e1, e2, e3, e4);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Item1.Id, a[i].Item1.Id);
                Assert.Equal(e[i].Item2.Id, a[i].Item2.Id);
                Assert.Equal(e[i].Item3.Id, a[i].Item3.Id);
                Assert.Equal(e[i].Item4.Id, a[i].Item4.Id);
            });
    }

    private void TestPredicateQueryWithFiveSources<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            from e5 in ss.Set<TEntity5>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id, e5.Id
            where DummyTrue(e1, e2, e3, e4, e5)
            select new ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(e1, e2, e3, e4, e5);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Item1.Id, a[i].Item1.Id);
                Assert.Equal(e[i].Item2.Id, a[i].Item2.Id);
                Assert.Equal(e[i].Item3.Id, a[i].Item3.Id);
                Assert.Equal(e[i].Item4.Id, a[i].Item4.Id);
                Assert.Equal(e[i].Item5.Id, a[i].Item5.Id);
            });
    }

    private void TestPredicateQueryWithSixSources<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(
        int seed,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter)
        where TEntity1 : OperatorEntityBase
        where TEntity2 : OperatorEntityBase
        where TEntity3 : OperatorEntityBase
        where TEntity4 : OperatorEntityBase
        where TEntity5 : OperatorEntityBase
        where TEntity6 : OperatorEntityBase
    {
        var setSourceTemplate = (ISetSource ss) =>
            from e1 in ss.Set<TEntity1>()
            from e2 in ss.Set<TEntity2>()
            from e3 in ss.Set<TEntity3>()
            from e4 in ss.Set<TEntity4>()
            from e5 in ss.Set<TEntity5>()
            from e6 in ss.Set<TEntity6>()
            orderby e1.Id, e2.Id, e3.Id, e4.Id, e5.Id, e6.Id
            where DummyTrue(e1, e2, e3, e4, e5, e6)
            select new ValueTuple<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(e1, e2, e3, e4, e5, e6);

        ExecuteQueryAndVerifyResults(
            seed,
            setSourceTemplate,
            actualSetSource,
            resultRewriter,
            resultVerifier: (e, a, i) =>
            {
                Assert.Equal(e[i].Item1.Id, a[i].Item1.Id);
                Assert.Equal(e[i].Item2.Id, a[i].Item2.Id);
                Assert.Equal(e[i].Item3.Id, a[i].Item3.Id);
                Assert.Equal(e[i].Item4.Id, a[i].Item4.Id);
                Assert.Equal(e[i].Item5.Id, a[i].Item5.Id);
                Assert.Equal(e[i].Item6.Id, a[i].Item6.Id);
            });
    }

    private static bool DummyTrue<TEntity1>(TEntity1 e1)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2>(
        TEntity1 e1, TEntity2 e2)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3, TEntity4>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3, TEntity4 e4)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3, TEntity4 e4, TEntity5 e5)
        => true;

    private static bool DummyTrue<TEntity1, TEntity2, TEntity3, TEntity4, TEntity5, TEntity6>(
        TEntity1 e1, TEntity2 e2, TEntity3 e3, TEntity4 e4, TEntity5 e5, TEntity6 e6)
        => true;

    private class ResultExpressionPredicateRewriter : ExpressionVisitor
    {
        private static readonly MethodInfo _likeMethodInfo
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like), new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private readonly Expression[] _roots;
        private readonly Expression _resultExpression;

        public ResultExpressionPredicateRewriter(Expression resultExpression, Expression[] roots)
        {
            _resultExpression = resultExpression;
            _roots = roots;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(DummyTrue))
            {
                // replace dummy with the actual predicate
                if (methodCallExpression.Arguments.Count == 1)
                {
                    var replaced = ReplacingExpressionVisitor.Replace(
                        _roots[0],
                        Expression.Property(methodCallExpression.Arguments[0], "Value"),
                        _resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 2)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 3)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 4)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                            Expression.Property(methodCallExpression.Arguments[3], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 5)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                            Expression.Property(methodCallExpression.Arguments[3], "Value"),
                            Expression.Property(methodCallExpression.Arguments[4], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }

                if (methodCallExpression.Arguments.Count == 6)
                {
                    var replaced = new ReplacingExpressionVisitor(
                        _roots,
                        new[]
                        {
                            Expression.Property(methodCallExpression.Arguments[0], "Value"),
                            Expression.Property(methodCallExpression.Arguments[1], "Value"),
                            Expression.Property(methodCallExpression.Arguments[2], "Value"),
                            Expression.Property(methodCallExpression.Arguments[3], "Value"),
                            Expression.Property(methodCallExpression.Arguments[4], "Value"),
                            Expression.Property(methodCallExpression.Arguments[5], "Value"),
                        }).Visit(_resultExpression);

                    return replaced;
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    #endregion

    #region common infra

    private class RootEntityExpressionInfo
    {
        public RootEntityExpressionInfo(Expression expression)
        {
            Expression = expression;
            Used = false;
        }

        public Expression Expression { get; }

        public bool Used { get; set; }
    }

    private class ActualSetSource : ISetSource
    {
        private readonly DbContext _context;

        public ActualSetSource(DbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();
    }

    protected class ExpectedQueryRewritingVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(
                nameof(string.StartsWith), new[] { typeof(string) })!;

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(
                nameof(string.EndsWith), new[] { typeof(string) })!;


        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method == _likeMethodInfo)
            {
                if (methodCallExpression.Arguments[2] is ConstantExpression { Value: "A%" })
                {
                    return Expression.Call(
                        methodCallExpression.Arguments[1],
                        _startsWithMethodInfo,
                        Expression.Constant("A"));
                }

                if (methodCallExpression.Arguments[2] is ConstantExpression { Value: "%B" })
                {
                    return Expression.Call(
                        methodCallExpression.Arguments[1],
                        _endsWithMethodInfo,
                        Expression.Constant("B"));
                }

                return Expression.Equal(methodCallExpression.Arguments[1], methodCallExpression.Arguments[2]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    private void ExecuteQueryAndVerifyResults<TResult>(
        int seed,
        Func<ISetSource, IQueryable<TResult>> setSourceTemplate,
        ISetSource actualSetSource,
        ExpressionVisitor resultRewriter,
        Action<List<TResult>, List<TResult>, int> resultVerifier)
    {
        var expectedQueryTemplate = setSourceTemplate(ExpectedData);
        var expectedRewritten = resultRewriter.Visit(expectedQueryTemplate.Expression);
        expectedRewritten = ExpectedQueryRewriter.Visit(expectedRewritten);
        var expectedQuery = expectedQueryTemplate.Provider.CreateQuery<TResult>(expectedRewritten);

        var actualQueryTemplate = setSourceTemplate(actualSetSource);
        var actualRewritten = resultRewriter.Visit(actualQueryTemplate.Expression);
        var actualQuery = actualQueryTemplate.Provider.CreateQuery<TResult>(actualRewritten);

        var expectedResults = new List<TResult>();
        var actualResults = new List<TResult>();
        var divideByZeroExpected = false;

        try
        {
            expectedResults = expectedQuery.ToList();
        }
        catch (DivideByZeroException)
        {
            divideByZeroExpected = true;
        }

        try
        {
            actualResults = actualQuery.ToList();
        }
        catch (Exception ex)
        {
            if (!divideByZeroExpected || !DivideByZeroException(ex))
            {
                throw new InvalidOperationException("Seed: " + seed, ex);
            }
        }

        // it's possible that expected tries to divide by zero, but actual doesn't
        // if this happens we can't validate the result
        if (!divideByZeroExpected)
        {
            try
            {
                Assert.Equal(expectedResults.Count, actualResults.Count);
                for (var i = 0; i < actualResults.Count; i++)
                {
                    resultVerifier(expectedResults, actualResults, i);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Seed: " + seed, ex);
            }
        }
    }

    protected virtual bool DivideByZeroException(Exception ex)
        => ex.Message.StartsWith(CoreStrings.ExpressionParameterizationExceptionSensitive("").Substring(0, 90))
            && ex.InnerException is DivideByZeroException;

    #endregion

    #region model

    protected class OperatorsContext : DbContext
    {
        public OperatorsContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperatorEntityString>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityInt>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityNullableInt>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityLong>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityBool>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityNullableBool>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OperatorEntityDateTimeOffset>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    protected class OperatorsData : ISetSource
    {
        public static readonly OperatorsData Instance = new();

        private readonly List<Expression<Func<string>>> _stringValues = new()
        {
            () => "A",
            () => "B",
            () => "AB",
        };

        private readonly List<Expression<Func<int>>> _intValues = new()
        {
            () => 1,
            () => 2,
            () => 8,
        };

        private readonly List<Expression<Func<int?>>> _nullableIntValues = new()
        {
            () => null,
            () => 2,
            () => 8,
        };

        private readonly List<Expression<Func<long>>> _longValues = new()
        {
            () => 1L,
            () => 2L,
            () => 8L,
        };

        private readonly List<Expression<Func<bool>>> _boolValues = new()
        {
            () => true,
            () => false,
        };

        private readonly List<Expression<Func<bool?>>> _nullableBoolValues = new()
        {
            () => null,
            () => true,
            () => false,
        };

        private readonly List<Expression<Func<DateTimeOffset>>> _dateTimeOffsetValues = new()
        {
            () => new DateTimeOffset(new DateTime(2000, 1, 1, 11, 0, 0), new TimeSpan(5, 10, 0)),
            () => new DateTimeOffset(new DateTime(2000, 1, 1, 10, 0, 0), new TimeSpan(-8, 0, 0)),
            () => new DateTimeOffset(new DateTime(2000, 1, 1, 9, 0, 0), new TimeSpan(13, 0, 0))
        };

        public IReadOnlyList<OperatorEntityString> OperatorEntitiesString { get; }
        public IReadOnlyList<OperatorEntityInt> OperatorEntitiesInt { get; }
        public IReadOnlyList<OperatorEntityNullableInt> OperatorEntitiesNullableInt { get; }
        public IReadOnlyList<OperatorEntityLong> OperatorEntitiesLong { get; }
        public IReadOnlyList<OperatorEntityBool> OperatorEntitiesBool { get; }
        public IReadOnlyList<OperatorEntityNullableBool> OperatorEntitiesNullableBool { get; }
        public IReadOnlyList<OperatorEntityDateTimeOffset> OperatorEntitiesDateTimeOffset { get; }
        public IDictionary<Type, List<Expression>> ConstantExpressionsPerType { get; }

        private OperatorsData()
        {
            OperatorEntitiesString = CreateStrings();
            OperatorEntitiesInt = CreateInts();
            OperatorEntitiesNullableInt = CreateNullableInts();
            OperatorEntitiesLong = CreateLongs();
            OperatorEntitiesBool = CreateBools();
            OperatorEntitiesNullableBool = CreateNullableBools();
            OperatorEntitiesDateTimeOffset = CreateDateTimeOffsets();

            ConstantExpressionsPerType = new Dictionary<Type, List<Expression>>()
            {
                { typeof(string), _stringValues.Select(x => x.Body).ToList() },
                { typeof(int), _intValues.Select(x => x.Body).ToList() },
                { typeof(int?), _nullableIntValues.Select(x => x.Body).ToList() },
                { typeof(long), _longValues.Select(x => x.Body).ToList() },
                { typeof(bool), _boolValues.Select(x => x.Body).ToList() },
                { typeof(bool?), _nullableBoolValues.Select(x => x.Body).ToList() },
                { typeof(DateTimeOffset), _dateTimeOffsetValues.Select(x => x.Body).ToList() },
            };
        }

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(OperatorEntityString))
            {
                return (IQueryable<TEntity>)OperatorEntitiesString.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityInt))
            {
                return (IQueryable<TEntity>)OperatorEntitiesInt.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityNullableInt))
            {
                return (IQueryable<TEntity>)OperatorEntitiesNullableInt.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityLong))
            {
                return (IQueryable<TEntity>)OperatorEntitiesLong.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityBool))
            {
                return (IQueryable<TEntity>)OperatorEntitiesBool.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityNullableBool))
            {
                return (IQueryable<TEntity>)OperatorEntitiesNullableBool.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OperatorEntityDateTimeOffset))
            {
                return (IQueryable<TEntity>)OperatorEntitiesDateTimeOffset.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public IReadOnlyList<OperatorEntityString> CreateStrings()
            => _stringValues.Select((x, i) => new OperatorEntityString { Id = i + 1, Value = _stringValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityInt> CreateInts()
            => _intValues.Select((x, i) => new OperatorEntityInt { Id = i + 1, Value = _intValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityNullableInt> CreateNullableInts()
            => _nullableIntValues.Select((x, i) => new OperatorEntityNullableInt { Id = i + 1, Value = _nullableIntValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityLong> CreateLongs()
            => _longValues.Select((x, i) => new OperatorEntityLong { Id = i + 1, Value = _longValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityBool> CreateBools()
            => _boolValues.Select((x, i) => new OperatorEntityBool { Id = i + 1, Value = _boolValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityNullableBool> CreateNullableBools()
            => _nullableBoolValues.Select((x, i) => new OperatorEntityNullableBool { Id = i + 1, Value = _nullableBoolValues[i].Compile()() }).ToList();

        public IReadOnlyList<OperatorEntityDateTimeOffset> CreateDateTimeOffsets()
            => _dateTimeOffsetValues.Select((x, i) => new OperatorEntityDateTimeOffset { Id = i + 1, Value = _dateTimeOffsetValues[i].Compile()() }).ToList();
    }

    public abstract class OperatorEntityBase
    {
        public int Id { get; set; }
    }

    public class OperatorEntityString : OperatorEntityBase
    {
        public string Value { get; set; }
    }

    public class OperatorEntityInt : OperatorEntityBase
    {
        public int Value { get; set; }
    }

    public class OperatorEntityNullableInt : OperatorEntityBase
    {
        public int? Value { get; set; }
    }

    public class OperatorEntityLong : OperatorEntityBase
    {
        public long Value { get; set; }
    }

    public class OperatorEntityBool : OperatorEntityBase
    {
        public bool Value { get; set; }
    }

    public class OperatorEntityNullableBool : OperatorEntityBase
    {
        public bool? Value { get; set; }
    }

    public class OperatorEntityDateTimeOffset : OperatorEntityBase
    {
        public DateTimeOffset Value { get; set; }
    }

    #endregion
}
