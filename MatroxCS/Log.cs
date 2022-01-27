using System;
using System.IO;
using System.Text;

namespace MatroxCS
{
    class CLog
    {
        string m_strExeFolderPath;  // アプリケーション実行パス
        string m_strFileName;       // 作成ログファイル名

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="nstrExeFolderPath">アプリケーション実行パス</param>
        /// <param name="nstrFileName">作成ログファイル名</param>
        public CLog(string nstrExeFolderPath, string nstrFileName)
        {
            m_strExeFolderPath = nstrExeFolderPath;
            m_strFileName = nstrFileName;
        }

        /// <summary>
        /// ログを書き出す
        /// </summary>
        /// <param name="nstrErrorLog">ログ内容</param>
        public void OutputLog(string nstrLogContents)
        {
            // ログの文字コードはShift-JISとする
            Encoding encod_shift_jis = Encoding.GetEncoding("Shift_JIS");
            string str_file_path = $"{SetFolderName(m_strExeFolderPath, CDefine.LOG_FOLDER)}{m_strFileName}";    // ログファイルパス
            string str_log_contents;                                                                // ログ内容
            DateTime time_now = System.DateTime.Now;                                                // 現在日時

            // ログ内容の作成
            str_log_contents = $"{time_now.ToString("yyyy/MM/dd")} {time_now.ToString("HH:mm:ss")},{nstrLogContents}";
            // ログの書き出し
            using (StreamWriter writer = new StreamWriter(str_file_path, true, encod_shift_jis))
            {
                writer.WriteLine(str_log_contents);
            }
        }

        /// <summary>
        /// フォルダーパスを作成する
        /// </summary>
        /// <param name="nstrExeFolderName">アプリケーション実行パス</param>
        /// <param name="nstrFolderName">指定フォルダーパス</param>
        /// <returns>作成絶対フォルダーパス(末尾は\とする)</returns>
        private string SetFolderName(string nstrExeFolderName, string nstrFolderName)
        {
            string str_folder_name; // フォルダー名
            // フォルダーパスを作成
            str_folder_name = $"{nstrExeFolderName}\\{nstrFolderName}";
            // 該当フォルダーパスが存在しない場合は作成する
            if (false == System.IO.File.Exists(str_folder_name))
            {
                System.IO.Directory.CreateDirectory(str_folder_name);
            }
            // フォルダーパスの末尾に\を付ける
            str_folder_name = $"{str_folder_name}\\";
            return str_folder_name;
        }
    }
}
