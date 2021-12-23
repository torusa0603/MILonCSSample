using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS
{
    static class CLogMatroxCS
    {
        // 辞書型ログインスタンスを作成
        private static Dictionary<string, CLog> m_sdicLogInstance 
            = new Dictionary<string, CLog>() { 
                {CDefine.LogKey.MIL_ERROR, new CLog(CDefine.EXE_FOLDER_PATH, "MILError.log")},    // MIL由来のエラー用
                {CDefine.LogKey.DLL_ERROR, new CLog(CDefine.EXE_FOLDER_PATH, "DLLError.log")},    // DLL由来のエラー用
                {CDefine.LogKey.OPERATE,  new CLog(CDefine.EXE_FOLDER_PATH, "Operate.log")}};    // 操作履歴用

        /// <summary>
        /// ログを出力する
        /// </summary>
        /// <param name="nstrKeyName">指定ログキー(Define.LogKeyに定義)</param>
        /// <param name="nstrLogContents">出力内容</param>
        public static void Output(string nstrKeyName, string nstrLogContents) 
        {
            // キーに指定されている文字列以外を受けとった場合は返してしまう
            if ((nstrKeyName != CDefine.LogKey.MIL_ERROR) && (nstrKeyName != CDefine.LogKey.DLL_ERROR) && (nstrKeyName != CDefine.LogKey.OPERATE))
            {
                return;
            }
            // ログを出力する
            m_sdicLogInstance[nstrKeyName].OutputLog(nstrLogContents);
        }
    }
}
