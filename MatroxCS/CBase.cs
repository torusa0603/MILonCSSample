using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;


namespace MatroxCS
{
    public enum MTX_TYPE
    {
        // ボードの種類毎に割り振られた整数値
        MTX_MORPHIS = 0,
        MTX_SOLIOSXCL,
        MTX_SOLIOSXA,
        MTX_METEOR2MC,
        MTX_GIGE,
        MTX_HOST = 100
    };
    class CBase
    {
        #region メンバ変数

        public static Action m_sevFatalErrorOccured;                                            // 致命的なエラー発生時に起動するイベント

        #endregion

        #region ローカル変数

        static protected MIL_ID m_smilApplication = MIL.M_NULL;                                 // アプリケーションID
        static protected MIL_ID m_smilSystem = MIL.M_NULL;                                      // システムID
        static protected int m_siBoardType;                                                     // 使用ボードタイプ
        static protected object m_slockObject = new object();                                   // 排他制御に使用
        static protected Dictionary<string, CLog> m_sdicLogInstance;                            // ログインスタンスを格納
        static protected string m_sstrExePath;                                                  // アプリケーション実行パス 
        static protected readonly MIL_INT m_smilintTransparentColor = MIL.M_RGB888(1, 1, 1);    // 透過色
        static protected bool m_sbFatalErrorOccured = false;                                    // 致命的なエラー発生を示すフラグ

        protected GCHandle hUserData_Error;                                                     // 本クラスのポインター
        protected MIL_APP_HOOK_FUNCTION_PTR ProcessingFunctionPtr_Error;                        // フック関数のポインター

        # region 各インスタンスID

        static protected int m_siIDMaxLength = 10000;                                           // 各IDのmax個数。

        static protected int m_siCameraOffsetID = 10000;                                        // カメラIDオフセット
        static protected int m_siNextCameraID = 10000;                                          // 10000～19999まで。これを循環して使用

        static protected int m_siDisplayOffsetID = 20000;                                       // 画面表示IDオフセット
        static protected int m_siNextDisplayID = 20000;                                         // 20000～29999まで。これを循環して使用

        #endregion

        #region 固有エラー番号

        protected const int EXCPTIOERROR = -999;                                                // try-catchで捉えたエラー

        #endregion

        #endregion

        #region メンバ関数

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="niBoardType">ボードの種類</param>
        /// <param name="nstrExePath">アプリケーション実行パス</param>
        /// <returns>0:正常終了、-1:アプリケーションID取得失敗、-2:指定ボード種類に該当なし、-3:システムID取得失敗、-999:異常終了</returns>
        public int Initial(int niBoardType, string nstrExePath)
        {
            m_siBoardType = niBoardType;
            m_sstrExePath = nstrExePath;

            // ログオブジェクトを作成
            m_sdicLogInstance = new Dictionary<string, CLog>();
            m_sdicLogInstance.Add("MILError", new CLog(nstrExePath, "MILErrorLog.log"));
            m_sdicLogInstance.Add("DLLError", new CLog(nstrExePath, "DLLErrorLog.log"));
            m_sdicLogInstance.Add("Operate", new CLog(nstrExePath, "OperateLog.log"));

            try
            {
                // アプリケーションID取得
                MIL.MappAlloc(MIL.M_DEFAULT, ref m_smilApplication);
                if (m_smilApplication == MIL.M_NULL)
                {
                    return -1;
                }
                //　エラーメッセージを出さないようにする
                MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_DISABLE);
                //	エラーフック関数登録
                // 本クラスのポインターを設定
                hUserData_Error = GCHandle.Alloc(this);
                // フック関数のポインタを設定
                ProcessingFunctionPtr_Error = new MIL_APP_HOOK_FUNCTION_PTR(HookErrorHandler);
                // MILのフック関数に設定
                MIL.MappHookFunction(MIL.M_ERROR_CURRENT, ProcessingFunctionPtr_Error, GCHandle.ToIntPtr(hUserData_Error));

                // システムID取得(ボードの種類毎に異なる)
                switch (m_siBoardType)
                {
                    case (int)MTX_TYPE.MTX_MORPHIS:
                        MIL.MsysAlloc(MIL.M_SYSTEM_MORPHIS, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                        break;
                    case (int)MTX_TYPE.MTX_SOLIOSXCL:
                        break;
                    case (int)MTX_TYPE.MTX_SOLIOSXA:
                        MIL.MsysAlloc(MIL.M_SYSTEM_SOLIOS, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                        break;
                    case (int)MTX_TYPE.MTX_METEOR2MC:
                        return -1;
                    case (int)MTX_TYPE.MTX_GIGE:
                        MIL.MsysAlloc(MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                        break;
                    case (int)MTX_TYPE.MTX_HOST:
                        MIL.MsysAlloc(MIL.M_SYSTEM_HOST, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                        break;
                    default:
                        return -2;
                }
                if (m_smilSystem == MIL.M_NULL)
                {
                    // システムIDが取得できていない場合はエラー
                    return -3;
                }

                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance["DLLError"].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return EXCPTIOERROR;
            }
        }

        /// <summary>
        /// 致命的なエラーの有無を取得
        /// </summary>
        /// <returns>true:あり、false:なし</returns>
        public bool GetFatalErrorOccured()
        {
            return m_sbFatalErrorOccured;
        }

        /// <summary>
        /// ベースクラスの終了処理
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int End()
        {
            try
            {
                // システムID取得済みなら
                if (m_smilSystem != MIL.M_NULL)
                {
                    // システムIDの解放
                    MIL.MsysFree(m_smilSystem);
                    m_smilSystem = MIL.M_NULL;
                }

                // アプリケーションID取得済みなら
                if (m_smilApplication != MIL.M_NULL)
                {
                    // アプリケーションIDの解放
                    MIL.MappFree(m_smilApplication);
                    m_smilApplication = MIL.M_NULL;
                }
                // 致命的エラーを示すフラグをオフにする
                m_sbFatalErrorOccured = false;
                // ログオブジェクト全てを破棄する
                m_sdicLogInstance.Clear();
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance["DLLError"].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return EXCPTIOERROR;
            }
        }

        #endregion

        #region ローカル関数

        /// <summary>
        /// エラーフック関数
        /// </summary>
        /// <param name="nlHookType"></param>
        /// <param name="nEventId"></param>
        /// <param name="npUserDataPtr"></param>
        /// <returns>MIL.M_NULL</returns>
        protected MIL_INT HookErrorHandler(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            StringBuilder ErrorMessageFunction = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);   // エラー発生関数名バッファ
            StringBuilder ErrorMessage = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);           // エラー内容バッファ
            StringBuilder ErrorSubMessage1 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);       // エラー内容詳細文字列1バッファ
            StringBuilder ErrorSubMessage2 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);       // エラー内容詳細文字列2バッファ
            StringBuilder ErrorSubMessage3 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);       // エラー内容詳細文字列3バッファ
            string str_error;                                                                   // エラーログ内容
            string str_function;                                                                // エラー発生関数名
            long NbSubCode = 0;                                                                 // エラー内容詳細の文字列数


            //　送られてきたポインタをベースクラスポインタにキャスティングする
            GCHandle hUserData = GCHandle.FromIntPtr(npUserDataPtr);
            CBase p_matrox_common = hUserData.Target as CBase;

            try
            {
                //	エラー文字列を取得する

                //	エラー発生関数を取得
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT_FCT, ErrorMessageFunction);
                //	エラー内容を取得
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT, ErrorMessage);
                //	エラー内容詳細の文字列数を取得
                MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_NB, ref NbSubCode);

                //　エラー内容詳細文字列を項目数に合わせて受け取る
                if (NbSubCode > 2)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_3 + MIL.M_MESSAGE, ErrorSubMessage3);
                }
                if (NbSubCode > 1)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_2 + MIL.M_MESSAGE, ErrorSubMessage2);
                }
                if (NbSubCode > 0)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_1 + MIL.M_MESSAGE, ErrorSubMessage1);
                }

                //	ログに出力するエラー内容を作成する

                //	まずエラー発生関数をエラーログ内容に追加する
                str_error = "Function:(";
                str_error += ErrorMessageFunction;
                str_error += ") ";
                //	次にエラー内容をエラーログ内容に追加する
                str_error += ErrorMessage;
                str_error += " ";
                //	次に詳細内容を順次エラーログ内容に追加する
                if (NbSubCode > 2)
                {
                    str_error += ErrorSubMessage3;
                    str_error += " ";
                }
                if (NbSubCode > 1)
                {
                    str_error += ErrorSubMessage2;
                    str_error += " ";
                }
                if (NbSubCode > 0)
                {
                    str_error += ErrorSubMessage1;
                    str_error += " ";
                }
                //	エラーログ内容を出力する
                m_sdicLogInstance["MILError"].OutputLog(str_error);

                //	致命的なエラーかどうか判断する
                //	MdigProcess、xxxAllocで発生するエラーは全て致命的とする

                str_function = ErrorMessageFunction.ToString();
                if (str_function.IndexOf("Alloc") != -1)
                {
                    // 致命的エラーの発生をイベントで知らせる
                    m_sevFatalErrorOccured();
                    // 致命的エラーの発生を示すフラグを立てる
                    m_sbFatalErrorOccured = true;
                }

                return (MIL.M_NULL);
            }
            catch
            {
                //	エラーフックの例外エラー
                str_error = "Unknown Error";
                //	エラーをログ出力する
                m_sdicLogInstance["MILError"].OutputLog(str_error);

                return (MIL.M_NULL);
            }
        }

        #endregion
    }
}
