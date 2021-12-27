using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS.Algorithm
{
    interface IAlgorithm : IDisposable
    {
        //  検査を実行
        List<object> Execute(CRequiredParameterForAlgorithm ncRequiredParameterForAlgorithm, List<object> noValue = null);
    }
}
