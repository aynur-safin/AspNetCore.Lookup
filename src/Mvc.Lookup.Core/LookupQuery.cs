using System;
using System.Linq;
using System.Linq.Expressions;

namespace NonFactors.Mvc.Lookup
{
    internal class LookupQuery : ExpressionVisitor
    {
        public Boolean Ordered { get; set; }

        public static Boolean IsOrdered(IQueryable models)
        {
            LookupQuery query = new LookupQuery();
            query.Visit(models.Expression);

            return query.Ordered;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && (node.Method.Name.StartsWith("OrderBy") || node.Method.Name.StartsWith("ThenBy")))
            {
                Ordered = true;

                return node;
            }

            return base.VisitMethodCall(node);
        }
    }
}
