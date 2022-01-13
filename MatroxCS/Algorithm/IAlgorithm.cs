using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS.Algorithm
{
    abstract class IAlgorithm : CBase
    {
        //  検査を実行
        public abstract List<object> Execute(CRequiredParameterForAlgorithm ncRequiredParameterForAlgorithm, List<object> noValue = null);
    }
}
