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
                {Define.LogKey.m_cstrLogKeyMilError, new CLog(Define.EXE_FOLDER_PATH, "MILError.log")},    // MIL由来のエラー用
                {Define.LogKey.m_cstrLogKeyDllError, new CLog(Define.EXE_FOLDER_PATH, "DLLError.log")},    // DLL由来のエラー用
                {Define.LogKey.m_cstrLogKeyOperate,  new CLog(Define.EXE_FOLDER_PATH, "Operate.log")}};    // 操作履歴用

        /// <summary>
        /// ログを出力する
        /// </summary>
        /// <param name="nstrKeyName">指定ログキー(Define.LogKeyに定義)</param>
        /// <param name="nstrLogContents">出力内容</param>
        public static void Output(string nstrKeyName, string nstrLogContents) 
        {
            // キーに指定されている文字列以外を受けとった場合は返してしまう
            if ((nstrKeyName != Define.LogKey.m_cstrLogKeyMilError) && (nstrKeyName != Define.LogKey.m_cstrLogKeyDllError) && (nstrKeyName != Define.LogKey.m_cstrLogKeyOperate))
            {
                return;
            }
            // ログを出力する
            m_sdicLogInstance[nstrKeyName].OutputLog(nstrLogContents);
        }
    }
}
