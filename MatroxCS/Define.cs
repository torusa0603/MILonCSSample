using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS
{
    class CDefine
    {
        /// <summary>
        /// 固有エラー番号
        /// </summary>
        public static class SpecificErrorCode
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
        public static class LogKey
        {
            /// <summary>
            /// MILエラー用オブジェクトに対応するキー名
            /// </summary>
            public const string MIL_ERROR = "MILError";
            /// <summary>
            /// DLL由来エラー用オブジェクトに対応するキー名
            /// </summary>
            public const string DLL_ERROR = "DLLError";
            /// <summary>
            /// 操作履歴用オブジェクトに対応するキー名
            /// </summary>
            public const string OPERATE = "Operate";
        }

        /// <summary>
        /// カメラに関する全般設定項目の限界値
        /// </summary>
        public static class CCameraGeneralLimit
        {
            /// <summary>
            /// カメラ個数の上限
            /// </summary>
            public static int NUMBER = 10;
            /// <summary>
            /// ハートビート時間の上限
            /// </summary>
            public static int HEART_BEAT_TIME = 10;
        }

        /// <summary>
        /// 個々のカメラに対する設定項目の限界値
        /// </summary>
        public static class CCameraInfoLimit
        {
            /// <summary>
            /// 取得画像幅の上限
            /// </summary>
            public const int WIDTH = 10000;
            /// <summary>
            /// 取得画像高さの上限
            /// </summary>
            public const int HEIGHT = 10000;
        }



        /// <summary>
        /// ボードの種類毎に割り振られた整数値(0: MORPHIS、1: SOLIOSXCL、2: SOLIOSXA、3: METEOR2MC、4: GIGE、100: HOST)
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
