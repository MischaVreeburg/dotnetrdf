using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class TypedLiteralExpression<T> : LiteralExpression
    {
        protected internal TypedLiteralExpression(T literalValue)
            : base(literalValue.ToConstantTerm())
        {
        }

        protected TypedLiteralExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

        public static BooleanExpression operator >(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Gt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Lt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator >=(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Ge(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <=(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Le(left.Expression, right.Expression);
        }

        public static BooleanExpression operator ==(TypedLiteralExpression<T> left, T right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator !=(TypedLiteralExpression<T> left, T right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator ==(T left, TypedLiteralExpression<T> right)
        {
            return new BooleanExpression(new EqualsExpression(left.ToConstantTerm(), right.Expression));
        }

        public static BooleanExpression operator !=(T left, TypedLiteralExpression<T> right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.ToConstantTerm(), right.Expression));
        }

        public static BooleanExpression operator >(TypedLiteralExpression<T> left, T right)
        {
            return Gt(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator <(TypedLiteralExpression<T> left, T right)
        {
            return Lt(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(TypedLiteralExpression<T> left, T right)
        {
            return Ge(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(TypedLiteralExpression<T> left, T right)
        {
            return Le(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator >(T left, TypedLiteralExpression<T> right)
        {
            return Gt(left.ToConstantTerm(), right.Expression);
        }

        public static BooleanExpression operator <(T left, TypedLiteralExpression<T> right)
        {
            return Lt(left.ToConstantTerm(), right.Expression);
        }

        public static BooleanExpression operator >=(T left, TypedLiteralExpression<T> right)
        {
            return Ge(left.ToConstantTerm(), right.Expression);
        }

        public static BooleanExpression operator <=(T left, TypedLiteralExpression<T> right)
        {
            return Le(left.ToConstantTerm(), right.Expression);
        }
    }
}