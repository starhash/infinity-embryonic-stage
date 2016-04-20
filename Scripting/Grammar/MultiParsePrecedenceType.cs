using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Scripting.Grammar
{
    //1...n, Highest to Lowest
    //n...1, Lowest to Highest
    //No precedence
    //Custom precedence
    public enum MultiParsePrecedenceType
    {
        Ascending,
        Descending,
        Normal, 
        Custom
    }
}
