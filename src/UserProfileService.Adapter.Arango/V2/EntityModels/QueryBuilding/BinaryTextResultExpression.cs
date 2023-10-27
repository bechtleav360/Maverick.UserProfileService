using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class BinaryTextResultExpression : Expression, IAqlExpression
{
    public string LeftSideResult { get; }
    public override ExpressionType NodeType => ExpressionType.Constant;
    public string OperatorString { get; }
    public string RightSideResult { get; }
    public override Type Type { get; }

    public BinaryTextResultExpression(
        string leftSideResult,
        string operatorString,
        string rightSideResult)
    {
        Type = typeof(BinaryTextResultExpression);
        OperatorString = operatorString ?? throw new ArgumentNullException(nameof(operatorString));
        LeftSideResult = leftSideResult ?? throw new ArgumentNullException(nameof(leftSideResult));
        RightSideResult = rightSideResult ?? throw new ArgumentNullException(nameof(rightSideResult));
    }

    public string GetAqlString()
    {
        return $"({LeftSideResult}{OperatorString}{RightSideResult})";
    }
}
