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

        #endregion

        #region メンバ関数

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="niBoardType">ボードの種類</param>
        /// <param name="nstrExeFolderPath">アプリケーション実行パス</param>
        /// <returns>0:正常終了、-1:アプリケーションID取得失敗、-2:指定ボード種類に該当なし、-3:システムID取得失敗、-999:異常終了</returns>
        public int Initial(int niBoardType)
        {
            m_siBoardType = niBoardType;

            //// ログオブジェクトを作成
            //m_sdicLogInstance = new Dictionary<string, CLog>();
            //m_sdicLogInstance.Add(m_sstrLogKeyMilError, new CLog(nstrExeFolderPath, "MILError.log"));
            //m_sdicLogInstance.Add(m_sstrLogKeyDllError, new CLog(nstrExeFolderPath, "DLLError.log"));

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
                // エラーフック関数登録
                // 本クラスのポインターを設定
                hUserData_Error = GCHandle.Alloc(this);
                // フック関数のポインタを設定
                ProcessingFunctionPtr_Error = new MIL_APP_HOOK_FUNCTION_PTR(HookErrorHandler);
                // MILのフック関数に設定
                MIL.MappHookFunction(MIL.M_ERROR_CURRENT, ProcessingFunctionPtr_Error, GCHandle.ToIntPtr(hUserData_Error));

                // システムID取得(ボードの種類毎に異なる)
                switch (m_siBoardType)
                {
                    case (int)CDefine.MTX_TYPE.MTX_MORPHIS:
                        MIL.MsysAlloc(MIL.M_SYSTEM_MORPHIS, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                        break;
                    case (int)CDefine.MTX_TYPE.MTX_SOLIOSXCL:
                        break;
                    case (int)CDefine.MTX_TYPE.MTX_SOLIOSXA:
                        MIL.MsysAlloc(MIL.M_SYSTEM_SOLIOS, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                        break;
                    case (int)CDefine.MTX_TYPE.MTX_METEOR2MC:
                        return -1;
                    case (int)CDefine.MTX_TYPE.MTX_GIGE:
                        MIL.MsysAlloc(MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                        break;
                    case (int)CDefine.MTX_TYPE.MTX_HOST:
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
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
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
            return 0;
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
            StringBuilder str_buid_error_message_function = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);     // エラー発生関数名バッファ
            StringBuilder strbuid_error_main_message = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);         // エラー内容バッファ
            StringBuilder strbuid_error_sub_message1 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);         // エラー内容詳細文字列1バッファ
            StringBuilder strbuid_error_sub_message2 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);         // エラー内容詳細文字列2バッファ
            StringBuilder strbuid_error_sub_message3 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);         // エラー内容詳細文字列3バッファ
            string str_error_log_contents;                                                                  // エラーログ内容
            string str_function;                                                                            // エラー発生関数名
            long long_count_sub_comment = 0;                                                                // エラー内容詳細の文字列数


            //　送られてきたポインタをベースクラスポインタにキャスティングする
            GCHandle h_user_data = GCHandle.FromIntPtr(npUserDataPtr);
            CBase p_c_base = h_user_data.Target as CBase;

            try
            {
                //	エラー文字列を取得する

                //	エラー発生関数を取得
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT_FCT, str_buid_error_message_function);
                //	エラー内容を取得
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT, strbuid_error_main_message);
                //	エラー内容詳細の文字列数を取得
                MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_NB, ref long_count_sub_comment);

                //　エラー内容詳細文字列を項目数に合わせて受け取る
                if (long_count_sub_comment > 2)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_3 + MIL.M_MESSAGE, strbuid_error_sub_message3);
                }
                if (long_count_sub_comment > 1)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_2 + MIL.M_MESSAGE, strbuid_error_sub_message2);
                }
                if (long_count_sub_comment > 0)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_1 + MIL.M_MESSAGE, strbuid_error_sub_message1);
                }

                //	ログに出力するエラー内容を作成する

                //	まずエラー発生関数をエラーログ内容に追加する
                str_error_log_contents = "Function:(";
                str_error_log_contents += str_buid_error_message_function;
                str_error_log_contents += ") ";
                //	次にエラー内容をエラーログ内容に追加する
                str_error_log_contents += strbuid_error_main_message;
                str_error_log_contents += " ";
                //	次に詳細内容を順次エラーログ内容に追加する
                if (long_count_sub_comment > 2)
                {
                    str_error_log_contents += strbuid_error_sub_message3;
                    str_error_log_contents += " ";
                }
                if (long_count_sub_comment > 1)
                {
                    str_error_log_contents += strbuid_error_sub_message2;
                    str_error_log_contents += " ";
                }
                if (long_count_sub_comment > 0)
                {
                    str_error_log_contents += strbuid_error_sub_message1;
                    str_error_log_contents += " ";
                }
                //	エラーログ内容を出力する
                CLogMatroxCS.Output(CDefine.LogKey.MIL_ERROR, str_error_log_contents);
                //	致命的なエラーかどうか判断する
                //	MdigProcess、MbufAllocで発生するエラーは全て致命的とする

                str_function = str_buid_error_message_function.ToString();
                if (str_function.IndexOf("MbufAlloc") != -1)
                {
                    if (m_sevFatalErrorOccured != null)
                    {
                        // 致命的エラーの発生をイベントで知らせる
                        m_sevFatalErrorOccured();
                    }
                    // 致命的エラーの発生を示すフラグを立てる
                    m_sbFatalErrorOccured = true;
                }

                return (MIL.M_NULL);
            }
            catch
            {
                //	エラーフックの例外エラー
                str_error_log_contents = "Unknown Error";
                //	エラーをログ出力する
                CLogMatroxCS.Output(CDefine.LogKey.MIL_ERROR, str_error_log_contents);
                return (MIL.M_NULL);
            }
        }

        #endregion
    }
}
