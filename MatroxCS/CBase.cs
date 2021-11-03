using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;
using System.IO;
//using System.Text.Json;
//using System.Text.Json.Serialization;


namespace MatroxCS
{
    public enum MTX_TYPE
    {
        MTX_MORPHIS = 0,
        MTX_SOLIOSXCL,
        MTX_SOLIOSXA,
        MTX_METEOR2MC,
        MTX_GIGE,
        MTX_HOST = 100
    };
    class CBase
    {

        //  全てのMIL操作共通のものはCBaseで定義する
        static protected MIL_ID m_smilApplication = MIL.M_NULL;
        static protected MIL_ID m_smilSystem = MIL.M_NULL;
        static protected int m_iBoardType;            //	使用ボードタイプ

        //  各インスタンスIDオフセット

        static protected int m_siIDMaxLength = 10000;     //  各IDのmax個数。
        //  カメラID
        static protected int m_siCameraOffsetID = 10000;      //カメラIDオフセット
        static protected int m_siNextCameraID = 10000;      //10000～19999まで。これを循環して使用
        //  画面表示ID
        static protected int m_siDisplayOffsetID = 20000;     //画面表示IDオフセット
        static protected int m_siNextDisplayID = 20000;     //20000～29999まで。これを循環して使用

        private string m_strExePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

        protected GCHandle hUserData_Error;
        protected MIL_APP_HOOK_FUNCTION_PTR ProcessingFunctionPtr_Error;
        protected bool m_bFatalErrorOccured;   //	致命的なエラー発生(ソフト再起動必須)




        public int initial(string nstrSettingPath)
        {
            ////	パラメータファイルを読み込む
            //readParameter(nstrSettingPath);

            //色々初期化

            //	アプリケーションID取得
            MIL.MappAlloc(MIL.M_DEFAULT, ref m_smilApplication);
            if (m_smilApplication == MIL.M_NULL)
            {
                return -1;
            }
            //　エラーメッセージを出さないようにする
            MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_DISABLE);
            //	エラーフック関数登録
            // 本クラスのポインター
            hUserData_Error = GCHandle.Alloc(this);
            // フック関数のポインタ
            ProcessingFunctionPtr_Error = new MIL_APP_HOOK_FUNCTION_PTR(hookErrorHandler);
            // 設定
            MIL.MappHookFunction(MIL.M_ERROR_CURRENT, ProcessingFunctionPtr_Error, GCHandle.ToIntPtr(hUserData_Error));

            //	システムID取得
            switch (m_iBoardType)
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

                    //MIL.MsysAlloc(MIL.M_SYSTEM_METEOR_II, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                    break;
                case (int)MTX_TYPE.MTX_GIGE:
                    MIL.MsysAlloc(MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                    break;
                case (int)MTX_TYPE.MTX_HOST:
                    MIL.MsysAlloc(MIL.M_SYSTEM_HOST, MIL.M_DEV0, MIL.M_DEFAULT, ref m_smilSystem);
                    break;
                default:
                    return -1;
            }
            if (m_smilSystem == MIL.M_NULL)
            {
                return -1;
            }

            //  設定ファイル読んだり？
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nlHookType"></param>
        /// <param name="nEventId"></param>
        /// <param name="npUserDataPtr"></param>
        /// <returns></returns>
        protected MIL_INT hookErrorHandler(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            StringBuilder ErrorMessageFunction = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorMessage = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorSubMessage1 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorSubMessage2 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorSubMessage3 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            string str_error;
            string str_function;
            long NbSubCode = 0;


            // get the handle to the DigHookUserData object back from the IntPtr
            GCHandle hUserData = GCHandle.FromIntPtr(npUserDataPtr);
            // get a reference to the DigHookUserData object
            CBase p_matrox_common = hUserData.Target as CBase;

            try
            {
                //	エラー文字列を取得する

                //	エラー発生関数
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT_FCT, ErrorMessageFunction);
                //	エラー内容
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT, ErrorMessage);
                //	エラー内容詳細の文字列数
                MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_NB, ref NbSubCode);

                //　エラー内容の詳細文字列を取得する
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

                //	まずエラー発生関数
                str_error = "Function:(";
                str_error += ErrorMessageFunction;
                str_error += ") ";
                //	次にエラー内容
                str_error += ErrorMessage;
                str_error += " ";
                //	次に詳細内容
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
                //	エラーをログ出力する
                p_matrox_common.outputErrorLog(str_error);

                //	致命的なエラーかどうか判断する
                //	MdigProcess、xxxAllocで発生するエラーは全て致命的とする
                str_function = ErrorMessageFunction.ToString();
                if (str_function.IndexOf("Alloc") != -1)
                {
                    p_matrox_common.m_bFatalErrorOccured = true;
                }

                return (MIL.M_NULL);
            }
            catch
            {
                //	エラーフックの例外エラー
                str_error = "Unknown Error";
                //	エラーをログ出力する
                p_matrox_common.outputErrorLog(str_error);

                return (MIL.M_NULL);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nstrErrorLog"></param>
        private void outputErrorLog(string nstrErrorLog)
        {
            Encoding m_Encoding = Encoding.GetEncoding("Shift_JIS");
            string str_file_name = "MILErrorLog.log";       //	ログファイルパス
            string str_file_path = $"{setFolderName(m_strExePath, "Log")}{str_file_name}";
            string str_log_data;
            DateTime time_now = System.DateTime.Now;
            str_log_data = $"{time_now.ToString("yyyyMMdd")}{time_now.ToString("HHmm")}{time_now.ToString("ssfff")}_error_{nstrErrorLog}";
            var writer = new StreamWriter(str_file_path, true, m_Encoding);
            writer.WriteLine(str_log_data);
            writer.Close();
        }

        /// <summary>
        /// フォルダーパスを作成する
        /// </summary>
        /// <param name="nstrExecFolderName"></param>
        /// <param name="nstrFolderName"></param>
        /// <returns></returns>
        private string setFolderName(string nstrExecFolderName, string nstrFolderName)
        {
            string str_folder_name;
            str_folder_name = $"{nstrExecFolderName}\\{nstrFolderName}";
            if (false == System.IO.File.Exists(str_folder_name))
            {
                System.IO.Directory.CreateDirectory(str_folder_name);
            }
            str_folder_name = $"{str_folder_name}\\";
            return str_folder_name;
        }
    }

    
}
