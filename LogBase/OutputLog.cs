using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Log
{
    public class COutputLog
    {

        #region パラメータ変数・ローカル変数(LogBaseから引用部分)

        //プロパティ
        private CLogParameter m_cLogParameter = new CLogParameter();

        // 書き込みミューテックス
        private Mutex m_mtxWrite = new Mutex();
        #endregion

        readonly Dictionary<CDefine.UPDATE_TIME, string> m_dctUpdateTime = new Dictionary<CDefine.UPDATE_TIME, string>()
        {
            {CDefine.UPDATE_TIME.Add,"Add" },
            {CDefine.UPDATE_TIME.Day,"Day" },
            {CDefine.UPDATE_TIME.Month,"Month" }
        };

        readonly Dictionary<CDefine.DELETE_TIME, string> m_dctDeleteTime = new Dictionary<CDefine.DELETE_TIME, string>()
        {
            {CDefine.DELETE_TIME.None,"None" },
            {CDefine.DELETE_TIME.Month,"Month" },
            {CDefine.DELETE_TIME.Half,"Half" },
            {CDefine.DELETE_TIME.Year,"Year" }
        };

        public COutputLog(string nstrFileName, CDefine.UPDATE_TIME niUpdate, CDefine.DELETE_TIME niDelete)
        {
            if (nstrFileName.IndexOf(CDefine.EXTENSION) != -1)
            {
                // 拡張子有りの場合は消去
                nstrFileName = nstrFileName.Replace(CDefine.EXTENSION, "");
            }
            m_cLogParameter.FileName = nstrFileName;
            m_cLogParameter.UpdateTime = m_dctUpdateTime[niUpdate];
            m_cLogParameter.DeleteTime = m_dctDeleteTime[niDelete];

            //削除対象となる古いファイルがあるか確認し、ある場合は削除するか確認とる
            AskOldLogDelete();

            //ログファイルに書き込み
            WriteLog("OutputLog Open.");
        }

        /// <summary>
        /// デストラクタ。OutputLogが閉じることをログファイルに書き込み
        /// </summary>
        ~COutputLog()
        {
            WriteLog("OutputLog Close.");
        }

        #region パブリック関数

        /// <summary>
        /// ログ出力する
        /// </summary>
        /// <param name="nstrLog"></param>
        public void WriteLog(string nstrLog)
        {

            //ミューテックスで排他処理をかける
            m_mtxWrite.WaitOne();
            try
            {
                //書き込みのたびにファイル名を生成(運用時に日付が変わることを想定)
                string str_file_name = SetFileName();

                // 書き込み実行(常に追加処理を行う)
                using (var writer = new StreamWriter(str_file_name, true, CDefine.ENCODING))
                {
                    writer.WriteLine(GetLogString(nstrLog));
                }
            }
            finally
            {
                //ミューテックスを解除(排他処理の解除)
                m_mtxWrite.ReleaseMutex();
            }
        }

        #endregion

        #region ローカル関数


        /// <summary>
        /// 1行分のログ文字列生成
        /// </summary>
        /// <param name="strLog">ログ文字列</param>
        private string GetLogString(string nstrLog)
        {
            //年月日+時間,ログ本文 の順番で出力する
            return System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff") + "," + nstrLog;

        }

        /// <summary>
        /// ファイル名の設定
        /// </summary>
        private string SetFileName()
        {
            //logファイルを保存するフォルダがない場合
            if (false == System.IO.File.Exists(CDefine.FolderName))
            {
                //フォルダを作成する
                System.IO.Directory.CreateDirectory(CDefine.FolderName);
            }

            //フォルダ名 + ファイル名 + YYYYMMDD_ + 拡張子
            return CreateLogFileName();
        }

        /// <summary>
        /// 保存するlogのファイル名を返す
        /// </summary>
        /// <returns></returns>
        private string CreateLogFileName()
        {

            //Day:日ごとに新しいファイルにする (例) FileName_YYYYMMDD.log
            if (m_cLogParameter.UpdateTime.Equals("Day"))
            {
                return CDefine.FolderName + m_cLogParameter.FileName + "_" + System.DateTime.Now.ToString("yyyyMMdd") + CDefine.EXTENSION;
            }
            //Month:月ごとに新しいファイルにする (例) FileName_YYYYMM.log
            else if (m_cLogParameter.UpdateTime.Equals("Month"))
            {
                return CDefine.FolderName + m_cLogParameter.FileName + "_" + System.DateTime.Now.ToString("yyyyMM") + CDefine.EXTENSION;
            }
            //Add:常にひとつのファイルを上書き更新し続ける (例) FileName.log           
            return CDefine.FolderName + m_cLogParameter.FileName + CDefine.EXTENSION;
        }


        private void AskOldLogDelete()
        {
            //UpdateTimeがNoneでない && 削除対象のファイルが1つでも存在する
            if (!m_cLogParameter.UpdateTime.Equals("None") && CountOldLog() > 0)
            {
                int i_count_del_file = CountOldLog();
                DialogResult dialog_ask_del = MessageBox.Show(
                    "保存期間を過ぎたログファイルが" + i_count_del_file + "個存在します。削除しますか？",
                    "確認:古いログファイルを削除しますか",
                    MessageBoxButtons.YesNo,    // ボタンの設定
                    MessageBoxIcon.Question);   // アイコンの設定

                if (dialog_ask_del == DialogResult.Yes)
                {
                    RemoveOldLog();
                    MessageBox.Show(i_count_del_file + "個の古いログファイルを削除しました。");
                }

            }

        }

        /// <summary>
        /// ログファイル削除
        /// </summary>
        private void RemoveOldLog()
        {
            //Noneの場合はログファイルを削除しない
            if (!m_cLogParameter.UpdateTime.Equals("None"))
            {
                //ログファイル(拡張子が.log)名をリストアップする
                List<string> lstFileName = GetFileList();

                foreach (string str_del_file_name in lstFileName)
                {
                    //リストからファイルの作成日時と現在時刻を比較する
                    if (IsOldFile(str_del_file_name))
                    {
                        //古いファイルは削除
                        System.IO.File.Delete(str_del_file_name);
                    }
                }
            }
        }

        /// <summary>
        /// 削除対象のログファイルがあるかを確認し、あるなら正の値、ないなら0を返す
        /// </summary>
        private int CountOldLog()
        {
            int i_count_old_file = 0;

            //Noneの場合は確認しない
            if (!m_cLogParameter.UpdateTime.Equals("None"))
            {
                //ログファイル(拡張子が.log)名をリストアップする
                List<string> lstFileName = GetFileList();

                foreach (string str_del_file_name in lstFileName)
                {
                    //リストからファイルの作成日時と現在時刻を比較する
                    if (IsOldFile(str_del_file_name))
                    {
                        //古いファイルの数をカウント
                        i_count_old_file++;
                    }
                }

            }
            return i_count_old_file;
        }

        /// <summary>
        /// ファイルリスト取得
        /// </summary>
        /// <returns>ファイルリスト</returns>
        private List<string> GetFileList()
        {
            return new List<string>(System.IO.Directory.GetFiles(CDefine.FolderName, "*" + CDefine.EXTENSION));
        }

        /// <summary>
        /// 古いファイル(=削除対象)かどうかを判定しboolで返す。true:削除対象
        /// </summary>
        /// <param name="nstrFileName"></param>
        /// <returns></returns>
        private bool IsOldFile(string nstrFileName)
        {
            //Month:ファイル作成日が現在時刻より一ヵ月以上古い場合はtrue
            if (m_cLogParameter.DeleteTime.Equals("Month") && System.IO.File.GetCreationTime(nstrFileName) < System.DateTime.Now.AddMonths(-1))
            {
                return true;
            }
            //Half:ファイル作成日が現在時刻より六ヵ月以上古い場合はtrue
            else if (m_cLogParameter.DeleteTime.Equals("Half") && System.IO.File.GetCreationTime(nstrFileName) < System.DateTime.Now.AddMonths(-6))
            {
                return true;
            }
            //Year:ファイル作成日が現在時刻より一年以上古い場合はtrue
            else if (m_cLogParameter.DeleteTime.Equals("Year") && System.IO.File.GetCreationTime(nstrFileName) < System.DateTime.Now.AddYears(-1))
            {
                return true;
            }
            return false;


        }

        #endregion

    }


}
