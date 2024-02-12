using FluentAssertions;
using Sprache;
using UserProfileService.Queries.Language.ExpressionOperators;
using UserProfileService.Queries.Language.Grammar;
using UserProfileService.Queries.Language.Tests.Extensions;
using UserProfileService.Queries.Language.TreeDefinition;
using Xunit;

namespace UserProfileService.Queries.Language.Tests.FilterQuery;

public class FilterQueryTests
{
    [Fact]
    public void FilterStringObjectRootTest()
    {
        const string queryString = "Date ge datetime'2022-01-01T00:00:00'";
        TreeNode rootNode = new FilterQueryGrammar(queryString).RootNodeParsed.Parse(queryString);
        const string queryResult = "Date ge datetime'2022-01-01T00:00:00'";

        var rootNodeToCompare = new RootNode(FilterOption.DollarFilter, queryResult);

        var expressionNode = new ExpressionNode(
            "Date",
            "2022-01-01T00:00:00",
            OperatorType.GreaterEqualsOperator);

        rootNodeToCompare.LeftChild = expressionNode;

        rootNode.Should()
                .BeEquivalentTo(
                    rootNodeToCompare,
                    opt => opt.Excluding(o => o.Id).Excluding(o => o.LeftChild.Id).RespectingRuntimeTypes());
    }

    [Theory]
    [InlineData(ExpressionOperators.ExpressionOperators.LessEqualsOperator)]
    [InlineData(ExpressionOperators.ExpressionOperators.LessThenOperator)]
    [InlineData(ExpressionOperators.ExpressionOperators.EqualsOperator)]
    [InlineData(ExpressionOperators.ExpressionOperators.NotEqualsOperator)]
    [InlineData(ExpressionOperators.ExpressionOperators.GreaterThenOperator)]
    [InlineData(ExpressionOperators.ExpressionOperators.GreaterEqualsOperator)]
    public void FilterQueryCreateEqualsExpressionTests(string @operator)
    {
        var queryString = $"Date {@operator} datetime′2022-01-01T00:00:00′";
        var filterQuery = new FilterQueryGrammar(queryString);
        TreeNode rootNode = filterQuery.ExpressionNode.Parse(queryString);
        var rootNodeToCompare = new ExpressionNode("Date", "2022-01-01T00:00:00", @operator.GetOperatorType());
        rootNode.Should().BeEquivalentTo(rootNodeToCompare, opt => opt.RespectingRuntimeTypes().Excluding(o => o.Id));
    }

    [Theory]
    [InlineData("Date eq datetime′2022-01-01T00:00:00′", "2022-01-01T00:00:00")]
    [InlineData("Id eq 123", "123")]
    public void FilterQueryParseInnerValue(string query, string rightHandSide)
    {
        var filterQuery = new FilterQueryGrammar(query);
        TreeNode parser = filterQuery.ExpressionNode.Parse(query);
        ((ExpressionNode)parser).RightSideExpression.Should().BeEquivalentTo(rightHandSide);
    }
    

    [Theory]
    [InlineData("Customer/Bill/Id", "Customer.Bill.Id")]
    [InlineData("Customer/Bill/Amount/Dollar/Prize", "Customer.Bill.Amount.Dollar.Prize")]
    [InlineData("Customer/Bill", "Customer.Bill")]
    public void LeftHandSideNestedClause(string query, string result)
    {
        var queryParser = new FilterQueryGrammar(query);

        string resultQuery = queryParser.LeftHandSideNested.Parse(query);

        resultQuery.Should().BeEquivalentTo(result);
    }
}
