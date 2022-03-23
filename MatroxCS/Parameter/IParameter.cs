using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS.Parameter
{
    public interface IParameter : IDisposable
    {
        /// <summary>
        /// 異常値判定
        /// </summary>
        /// <returns>0:異常値なし、-1:異常値あり</returns>
        int CheckVariableValidity();
    }
}
