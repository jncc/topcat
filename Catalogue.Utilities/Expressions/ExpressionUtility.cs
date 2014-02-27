using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Utilities.Expressions
{
    public static class ExpressionUtility
    {
        public static Expression RemoveUnary(this Expression e)
        {
            if (e is UnaryExpression)
                return ((UnaryExpression)e).Operand;
            else
                return e;
        }

    }
}
