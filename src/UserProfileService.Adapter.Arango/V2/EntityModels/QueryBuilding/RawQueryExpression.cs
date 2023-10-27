using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class RawQueryExpression : Expression
{
    public MemberInfo MemberInformation { get; private init; }

    /// <inheritdoc />
    public override ExpressionType NodeType => ExpressionType.Constant;

    public string RawQueryText { get; private init; }

    public Func<string, string> RawStringBuilder { get; private init; }

    /// <inheritdoc />
    public override Type Type { get; } = typeof(RawQueryExpression);

    private RawQueryExpression()
    {
    }

    public static RawQueryExpression CreateInstance<TEntity, TProp>(
        string rawQueryText,
        Expression<Func<TEntity, TProp>> selector)
    {
        if (string.IsNullOrWhiteSpace(rawQueryText))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rawQueryText));
        }

        return new RawQueryExpression
        {
            RawQueryText = rawQueryText,
            MemberInformation = (selector.Body as MemberExpression)?.Member
                ?? throw new ArgumentException("Could not extract property info.", nameof(selector))
        };
    }

    public static RawQueryExpression CreateInstance<TEntity, TProp>(
        Func<string, string> rawStringBuilder,
        Expression<Func<TEntity, TProp>> selector)
    {
        if (rawStringBuilder == null)
        {
            throw new ArgumentNullException(nameof(rawStringBuilder));
        }

        return new RawQueryExpression
        {
            RawStringBuilder = rawStringBuilder,
            MemberInformation = (selector.Body as MemberExpression)?.Member
                ?? throw new ArgumentException("Could not extract property info.", nameof(selector))
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{MemberInformation?.Name} => {RawQueryText ?? RawStringBuilder.Invoke("[ARG]")}";
    }
}
