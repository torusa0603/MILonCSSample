using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS.Algorithm
{
    public interface IAlgorithm
    {
        //  検査を実行
        List<object> Execute(List<object> noValue = null);
    }
}
