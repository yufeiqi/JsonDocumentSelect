using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using JsonDocumentSelect.Filters;
using JsonDocumentSelect.Test.Asserts;
using Xunit;

namespace JsonDocumentSelect.Test
{
    public class JsonDocumentSelectParseTests
    {
        [Fact]
        public void BooleanQuery_TwoValues()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(1 > 2)]");
            Assert.Single(path.Filters);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.NotNull(booleanExpression);
            Assert.Equal(1, ((JsonElement)booleanExpression.Left).GetInt32());
            Debug.Assert(booleanExpression.Right != null, "booleanExpression.Right != null");
            Assert.Equal(2, ((JsonElement)booleanExpression.Right).GetInt32());
            Assert.Equal(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [Fact]
        public void BooleanQuery_TwoPaths()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.price > @.max_price)]");
            Assert.Single(path.Filters);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> leftPaths = (List<PathFilter>)booleanExpression.Left;
            List<PathFilter> rightPaths = (List<PathFilter>)booleanExpression.Right;

            Assert.Equal("price", ((FieldFilter)leftPaths[0]).Name);
            Debug.Assert(rightPaths != null, nameof(rightPaths) + " != null");
            Assert.Equal("max_price", ((FieldFilter)rightPaths[0]).Name);
            Assert.Equal(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [Fact]
        public void SingleProperty()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah");
            Assert.Single(path.Filters);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedProperty()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("['Blah']");
            Assert.Single(path.Filters);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[  'Blah'  ]");
            Assert.Single(path.Filters);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithDots()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("['Blah.Ha']");
            Assert.Single(path.Filters);
            Assert.Equal("Blah.Ha", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithBrackets()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("['[*]']");
            Assert.Single(path.Filters);
            Assert.Equal("[*]", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SinglePropertyWithRoot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$.Blah");
            Assert.Single(path.Filters);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SinglePropertyWithRootWithStartAndEndWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect(" $.Blah ");
            Assert.Single(path.Filters);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void RootWithBadWhitespace()
        {
            _ = ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("$ .Blah"); },
                @"Unexpected character while parsing path:  ");
        }

        [Fact]
        public void NoFieldNameAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("$.Blah."); }, @"Unexpected end while parsing path.");
        }

        [Fact]
        public void RootWithBadWhitespace2()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("$. Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [Fact]
        public void WildcardPropertyWithRoot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$.*");
            Assert.Single(path.Filters);
            Assert.Null(((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void WildcardArrayWithRoot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$.[*]");
            Assert.Single(path.Filters);
            Assert.Null(((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void RootArrayNoDot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$[1]");
            Assert.Single(path.Filters);
            Assert.Equal(1, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void WildcardArray()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[*]");
            Assert.Single(path.Filters);
            Assert.Null(((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void WildcardArrayWithProperty()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[ * ].derp");
            Assert.Equal(2, path.Filters.Count);
            Assert.Null(((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.Equal("derp", ((FieldFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void QuotedWildcardPropertyWithRoot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$.['*']");
            Assert.Single(path.Filters);
            Assert.Equal("*", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleScanWithRoot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$..Blah");
            Assert.Single(path.Filters);
            Assert.Equal("Blah", ((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void QueryTrue()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$.elements[?(true)]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("elements", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal(QueryOperator.Exists, ((QueryFilter)path.Filters[1]).Expression.Operator);
        }

        [Fact]
        public void ScanQuery()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$.elements..[?(@.id=='AAA')]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("elements", ((FieldFilter)path.Filters[0]).Name);

            BooleanQueryExpression expression = (BooleanQueryExpression)((QueryScanFilter)path.Filters[1]).Expression;

            List<PathFilter> paths = (List<PathFilter>)expression.Left;

            Assert.IsType<FieldFilter>(paths[0]);
        }

        [Fact]
        public void WildcardScanWithRoot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$..*");
            Assert.Single(path.Filters);
            Assert.Null(((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void WildcardScanWithRootWithWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("$..* ");
            Assert.Single(path.Filters);
            Assert.Null(((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void TwoProperties()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah.Two");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal("Two", ((FieldFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void OnePropertyOneScan()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah..Two");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal("Two", ((ScanFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void SinglePropertyAndIndexer()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[0]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal(0, ((ArrayIndexFilter)path.Filters[1]).Index);
        }

        [Fact]
        public void SinglePropertyAndExistsQuery()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[ ?( @..name ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Exists, expressions.Operator);
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.Equal("name", ((ScanFilter)paths[0]).Name);
            Assert.Single(paths);
        }

        [Fact]
        public void SinglePropertyAndFilterWithWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[ ?( @.name=='hi' ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal("hi", ((JsonElement)expressions.Right).GetString());
        }

        [Fact]
        public void SinglePropertyAndFilterWithEscapeQuote()
        {
            JsonDocumentSelect path = new JsonDocumentSelect(@"Blah[ ?( @.name=='h\'i' ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal("h'i", ((JsonElement)expressions.Right).GetString());
        }

        [Fact]
        public void SinglePropertyAndFilterWithDoubleEscape()
        {
            JsonDocumentSelect path = new JsonDocumentSelect(@"Blah[ ?( @.name=='h\\i' ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal("h\\i", ((JsonElement)expressions.Right).GetString());
        }

        [Fact]
        public void SinglePropertyAndFilterWithRegexAndOptions()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[ ?( @.name=~/hi/i ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.RegexEquals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal("/hi/i", ((JsonElement)expressions.Right).GetString());
        }

        [Fact]
        public void SinglePropertyAndFilterWithRegex()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[?(@.title =~ /^.*Sword.*$/)]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.RegexEquals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal("/^.*Sword.*$/", ((JsonElement)expressions.Right).GetString());
        }

        [Fact]
        public void SinglePropertyAndFilterWithEscapedRegex()
        {
            JsonDocumentSelect path = new JsonDocumentSelect(@"Blah[?(@.title =~ /[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g)]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.RegexEquals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal(@"/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g", ((JsonElement)expressions.Right).GetString());
        }

        [Fact]
        public void SinglePropertyAndFilterWithOpenRegex()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect(@"Blah[?(@.title =~ /[\"); }, "Path ended with an open regex.");
        }

        [Fact]
        public void SinglePropertyAndFilterWithUnknownEscape()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect(@"Blah[ ?( @.name=='h\i' ) ]"); }, @"Unknown escape character: \i");
        }

        [Fact]
        public void SinglePropertyAndFilterWithFalse()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[ ?( @.name==false ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.False(((JsonElement)expressions.Right).GetBoolean());
        }

        [Fact]
        public void SinglePropertyAndFilterWithTrue()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[ ?( @.name==true ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Assert.True(expressions.Right != null && ((JsonElement)expressions.Right).GetBoolean());
        }

        [Fact]
        public void SinglePropertyAndFilterWithNull()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[ ?( @.name==null ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Null(((JsonElement)expressions.Right).GetObjectValue());
        }

        [Fact]
        public void FilterWithScan()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@..name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.Equal("name", ((ScanFilter)paths[0]).Name);
        }

        [Fact]
        public void FilterWithNotEquals()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.NotEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithNotEquals2()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name!=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.NotEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithLessThan()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name<null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.LessThan, expressions.Operator);
        }

        [Fact]
        public void FilterWithLessThanOrEquals()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name<=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.LessThanOrEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithGreaterThan()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.GreaterThan, expressions.Operator);
        }

        [Fact]
        public void FilterWithGreaterThanOrEquals()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name>=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.GreaterThanOrEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithInteger()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name>=12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal(12, ((JsonElement)expressions.Right).GetInt32());
        }

        [Fact]
        public void FilterWithNegativeInteger()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name>=-12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal(-12, ((JsonElement)expressions.Right).GetInt32());
        }

        [Fact]
        public void FilterWithFloat()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal(12.1d, ((JsonElement)expressions.Right).GetDouble());
        }

        [Fact]
        public void FilterExistWithAnd()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name&&@.title)]");
            CompositeExpression expressions = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.And, expressions.Operator);
            Assert.Equal(2, expressions.Expressions.Count);

            var first = (BooleanQueryExpression)expressions.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.Equal("name", ((FieldFilter)firstPaths[0]).Name);
            Assert.Equal(QueryOperator.Exists, first.Operator);

            var second = (BooleanQueryExpression)expressions.Expressions[1];
            var secondPaths = (List<PathFilter>)second.Left;
            Assert.Equal("title", ((FieldFilter)secondPaths[0]).Name);
            Assert.Equal(QueryOperator.Exists, second.Operator);
        }

        [Fact]
        public void FilterExistWithAndOr()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name&&@.title||@.pie)]");
            CompositeExpression andExpression = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.And, andExpression.Operator);
            Assert.Equal(2, andExpression.Expressions.Count);

            var first = (BooleanQueryExpression)andExpression.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.Equal("name", ((FieldFilter)firstPaths[0]).Name);
            Assert.Equal(QueryOperator.Exists, first.Operator);

            CompositeExpression orExpression = (CompositeExpression)andExpression.Expressions[1];
            Assert.Equal(2, orExpression.Expressions.Count);

            var orFirst = (BooleanQueryExpression)orExpression.Expressions[0];
            var orFirstPaths = (List<PathFilter>)orFirst.Left;
            Assert.Equal("title", ((FieldFilter)orFirstPaths[0]).Name);
            Assert.Equal(QueryOperator.Exists, orFirst.Operator);

            var orSecond = (BooleanQueryExpression)orExpression.Expressions[1];
            var orSecondPaths = (List<PathFilter>)orSecond.Left;
            Assert.Equal("pie", ((FieldFilter)orSecondPaths[0]).Name);
            Assert.Equal(QueryOperator.Exists, orSecond.Operator);
        }

        [Fact]
        public void FilterWithRoot()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?($.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.Equal(2, paths.Count);
            Assert.IsType<RootFilter>(paths[0]);
            Assert.IsType<FieldFilter>(paths[1]);
        }

        [Fact]
        public void BadOr1()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name||)]"), "Unexpected character while parsing path query: )");
        }

        [Fact]
        public void BaddOr2()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name|)]"), "Unexpected character while parsing path query: |");
        }

        [Fact]
        public void BaddOr3()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name|"), "Unexpected character while parsing path query: |");
        }

        [Fact]
        public void BaddOr4()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name||"), "Path ended with open query.");
        }

        [Fact]
        public void NoAtAfterOr()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name||s"), "Unexpected character while parsing path query: s");
        }

        [Fact]
        public void NoPathAfterAt()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name||@"), @"Path ended with open query.");
        }

        [Fact]
        public void NoPathAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name||@."), @"Unexpected end while parsing path.");
        }

        [Fact]
        public void NoPathAfterDot2()
        {
            ExceptionAssert.Throws<JsonException>(() => _ = new JsonDocumentSelect("[?(@.name||@.)]"), @"Unexpected end while parsing path.");
        }

        [Fact]
        public void FilterWithFloatExp()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[?(@.name>=5.56789e+0)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Debug.Assert(expressions.Right != null, "expressions.Right != null");
            Assert.Equal(5.56789e+0, ((JsonElement)expressions.Right).GetDouble());
        }

        [Fact]
        public void MultiplePropertiesAndIndexers()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("Blah[0]..Two.Three[1].Four");
            Assert.Equal(6, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.Equal("Two", ((ScanFilter)path.Filters[2]).Name);
            Assert.Equal("Three", ((FieldFilter)path.Filters[3]).Name);
            Assert.Equal(1, ((ArrayIndexFilter)path.Filters[4]).Index);
            Assert.Equal("Four", ((FieldFilter)path.Filters[5]).Name);
        }

        [Fact]
        public void BadCharactersInIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("Blah[[0]].Two.Three[1].Four"); }, @"Unexpected character while parsing path indexer: [");
        }

        [Fact]
        public void UnclosedIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("Blah[0"); }, @"Path ended with open indexer.");
        }

        [Fact]
        public void IndexerOnly()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[111119990]");
            Assert.Single(path.Filters);
            Assert.Equal(111119990, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void IndexerOnlyWithWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[  10  ]");
            Assert.Single(path.Filters);
            Assert.Equal(10, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void MultipleIndexes()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[111119990,3]");
            Assert.Single(path.Filters);
            Assert.Equal(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.Equal(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.Equal(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Fact]
        public void MultipleIndexesWithWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[   111119990  ,   3   ]");
            Assert.Single(path.Filters);
            Assert.Equal(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.Equal(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.Equal(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Fact]
        public void MultipleQuotedIndexes()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("['111119990','3']");
            Assert.Single(path.Filters);
            Assert.Equal(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.Equal("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.Equal("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Fact]
        public void MultipleQuotedIndexesWithWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[ '111119990' , '3' ]");
            Assert.Single(path.Filters);
            Assert.Equal(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.Equal("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.Equal("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Fact]
        public void SlicingIndexAll()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[111119990:3:2]");
            Assert.Single(path.Filters);
            Assert.Equal(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndex()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[111119990:3]");
            Assert.Single(path.Filters);
            Assert.Equal(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Null(((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexNegative()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[-111119990:-3:-2]");
            Assert.Single(path.Filters);
            Assert.Equal(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexEmptyStop()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[  -3  :  ]");
            Assert.Single(path.Filters);
            Assert.Equal(-3, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Null(((ArraySliceFilter)path.Filters[0]).End);
            Assert.Null(((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexEmptyStart()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[ : 1 : ]");
            Assert.Single(path.Filters);
            Assert.Null(((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(1, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Null(((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexWhitespace()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[  -111119990  :  -3  :  -2  ]");
            Assert.Single(path.Filters);
            Assert.Equal(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void EmptyIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("[]"); }, "Array index expected.");
        }

        [Fact]
        public void IndexerCloseInProperty()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("]"); }, "Unexpected character while parsing path: ]");
        }

        [Fact]
        public void AdjacentIndexers()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("[1][0][0][" + int.MaxValue + "]");
            Assert.Equal(4, path.Filters.Count);
            Assert.Equal(1, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.Equal(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.Equal(0, ((ArrayIndexFilter)path.Filters[2]).Index);
            Assert.Equal(int.MaxValue, ((ArrayIndexFilter)path.Filters[3]).Index);
        }

        [Fact]
        public void MissingDotAfterIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { _ = new JsonDocumentSelect("[1]Blah"); }, "Unexpected character following indexer: B");
        }

        [Fact]
        public void PropertyFollowingEscapedPropertyName()
        {
            JsonDocumentSelect path = new JsonDocumentSelect("frameworks.dnxcore50.dependencies.['System.Xml.ReaderWriter'].source");
            Assert.Equal(5, path.Filters.Count);

            Assert.Equal("frameworks", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal("dnxcore50", ((FieldFilter)path.Filters[1]).Name);
            Assert.Equal("dependencies", ((FieldFilter)path.Filters[2]).Name);
            Assert.Equal("System.Xml.ReaderWriter", ((FieldFilter)path.Filters[3]).Name);
            Assert.Equal("source", ((FieldFilter)path.Filters[4]).Name);
        }
    }
}