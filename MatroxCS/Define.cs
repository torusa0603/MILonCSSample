using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS
{
    class Define
    {
        /// <summary>
        /// 固有エラー番号
        /// </summary>
        public struct SpecificErrorCode
        {
            /// <summary>
            /// MILの処理中に発生した致命的エラー
            /// </summary>
            public const int FATAL_ERROR_OCCURED = -100;                                                            
            /// <summary>
            /// 初期化完了前に処理を実行した時のエラー
            /// </summary>
            public const int UNCOMPLETED_OPENING_ERROR = -200;                                                      
            /// <summary>
            /// try-catchで捉えた予期せぬエラー
            /// </summary>
            public const int EXCEPTION_ERROR = -999;                                                                
        }

        /// <summary>
        /// アプリケーション実行パス
        /// </summary>
        public readonly static string EXE_FOLDER_PATH = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

        /// <summary>
        /// 辞書型ログインスタンスのキー値
        /// </summary>
        public struct LogKey{
            public const string m_cstrLogKeyMilError = "MILError";  // 辞書型ログインスタンスのMILエラー用オブジェクトに対応するキー名
            public const string m_cstrLogKeyDllError = "DLLError";  // 辞書型ログインスタンスのDLL由来エラー用オブジェクトに対応するキー名                                                                                                                                  
            public const string m_cstrLogKeyOperate  = "Operate";   // 辞書型ログインスタンスの操作履歴用オブジェクトに対応するキー名 
        }

        /// <summary>
        /// ボードの種類毎に割り振られた整数値
        /// </summary>
        public enum MTX_TYPE
        {
            MTX_MORPHIS = 0,
            MTX_SOLIOSXCL,
            MTX_SOLIOSXA,
            MTX_METEOR2MC,
            MTX_GIGE,   
            MTX_HOST = 100
        }
    }
}
