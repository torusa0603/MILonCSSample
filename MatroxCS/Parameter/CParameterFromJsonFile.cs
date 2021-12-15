using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace MatroxCS.Parameter
{
    class CParameterFromJsonFile : CParameterBase
    {
        string m_strCommentCode = "###";                            // コメントコード
        string m_strNewLineCode = "\r\n";                           // 改行コード

        /// <summary>
        /// 設定ファイルの内容を設定用オブジェクトに格納
        /// </summary>
        /// <param name="nstrSettingFilePath">設定ファイルパス</param>
        /// <returns>0:正常終了、-1:設定ファイルパスの途中ディレクトリが存在しない、-2:設定ファイル作成・書き込みエラー、-3:設定ファイルなし(新規作成)、-4:設定ファイル構文エラー</returns>
        public override int ReadParameter(string nstrSettingFilePath)
        {

            int i_ret;
            if (!File.Exists(nstrSettingFilePath) || !(Path.GetExtension(nstrSettingFilePath) == ".json"))
            {
                i_ret = CreateSettingFile(nstrSettingFilePath);
                switch (i_ret)
                {
                    case -1:
                        // 設定ファイルの途中パスディレクトリが存在しない
                        return -1;
                    case -2:
                        // 設定ファイル作成・書き込みエラー
                        return -2;
                    default:
                        // 設定ファイルなし(新規作成)
                        return -3;
                }
            }
            try
            {
                // ファイルから文字列を丸ごと抜き出す
                string str_jsonfile_sentence = File.ReadAllText(nstrSettingFilePath);
                // 文章内のコメントコード～改行コード間にある文とコメントコードを削除する
                string str_jsonfile_sentence_commentout = RemoveComment(str_jsonfile_sentence);
                // コメントアウトの箇所を削除した文字列をデシリアライズする
                m_scCameraGeneral = JsonConvert.DeserializeObject<CCameraGeneral>(str_jsonfile_sentence_commentout);
                return 0;
            }
            catch
            {
                // -4:設定ファイル構文エラー
                return -4;
            }
        }

        /// <summary>
        /// "###"ー"改行コード(\r\n)"間の文字を排除する
        /// </summary>
        /// <param name="n_strJsonfileContents">Jsonファイルから読み込んだstring型データ</param>
        /// <returns>コメント削除結果</returns>
        private string RemoveComment(string nstrJsonfileContents)
        {
            string str_result = "";                     // 返答用のstring型データ
            string str_contents = nstrJsonfileContents; // 主となるstring型データ
            string str_front = "";                      // コメントコードより前の文章を格納するstring型データ
            string str_back = "";                       // コメントコードより後の文章を格納するstring型データ
            int i_num_comment_code;                     // コメントコードの位置を示すint型データ
            int i_num_enter;                            // 改行コードの位置を示すint型データ

            while (true)
            {
                // コメントコードの位置を探す
                i_num_comment_code = str_contents.IndexOf(m_strCommentCode);
                // コメントコードがこれ以上なければ終了
                if (i_num_comment_code == -1)
                {
                    break;
                }
                // コメントコードよりも前の文章を抽出
                str_front = str_contents.Substring(0, i_num_comment_code - 1);
                // コメントコードよりも後の文章を抽出
                str_back = str_contents.Substring(i_num_comment_code, str_contents.Length - i_num_comment_code);
                // コメントコード直後の改行コードを探す
                i_num_enter = str_back.IndexOf(m_strNewLineCode);
                // コメントコード直後の改行コードより後ろの文を抽出
                str_contents = str_back.Substring(i_num_enter, str_back.Length - i_num_enter);
                // コメントコードよりも前の文を返答用データに追加
                str_result += str_front;
            }
            // コメントコードを含まない後半データを返答用データに追加
            str_result += str_contents;
            // 返答する
            return str_result;
        }

        /// <summary>
        /// 設定ファイルを作成する
        /// </summary>
        /// <param name="nstrSettingFilePath">作成ファイルパス</param>
        /// <returns>0:正常終了、-1:設定ファイルパスの途中ディレクトリーが存在しない、-2:ファイル作成エラー</returns>
        private int CreateSettingFile(string nstrSettingFilePath)
        {
            // 作成ファイルパスのディレクトリの存在チェック
            string str_setting_file_folder = Path.GetDirectoryName(nstrSettingFilePath);
            if (!Directory.Exists(str_setting_file_folder))
            {
                return -1;
            }

            Encoding encod_encoding = Encoding.GetEncoding("utf-8");
            // デフォルトとなる情報を代入していく
            CCameraGeneral c_json_camera_general = new CCameraGeneral();
            CCameraInfo c_json_camera_info = new CCameraInfo();
            c_json_camera_general.Number = 1;
            c_json_camera_general.BoardType = 4;
            c_json_camera_general.HeartBeatTime = 5;
            c_json_camera_info.IdentifyName = "Camera1";
            c_json_camera_info.CameraType = 0;
            c_json_camera_info.CameraFile = " ";
            c_json_camera_info.Width = 0;
            c_json_camera_info.Height = 0;
            c_json_camera_info.Color = 0;
            c_json_camera_info.ImagePose = 0;
            c_json_camera_info.UseSerialComm = 0;
            c_json_camera_info.COMNo = 0;
            c_json_camera_info.IPAddress = " ";
            c_json_camera_general.CameraInformation.Add(c_json_camera_info);
            // パラメータをシリアライズする
            string str_json_contents = JsonConvert.SerializeObject(c_json_camera_general, Formatting.Indented);
            // パラメータ文字列にコメントを追加する
            AddDescriptionOfParameter(ref str_json_contents);

            try
            {
                // jsonファイルを作成する
                using (FileStream fs = File.Create(nstrSettingFilePath)) { }
                // jsonファイルにパラメータ文字列を書き込む
                using (var writer = new StreamWriter(nstrSettingFilePath, false, encod_encoding))
                {
                    writer.WriteLine(str_json_contents);
                }
                return 0;
            }
            catch
            {
                return -2;
            }
        }

        /// <summary>
        /// 作成される設定ファイルにコメントを加える
        /// </summary>
        /// <param name="nstrJsonContents">json型をシリアライズした文字列</param>
        private void AddDescriptionOfParameter(ref string nstrJsonContents)
        {
            // 取得するパラメータのアクセス修飾子を指定する
            BindingFlags b_access_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            // 一般設定項目クラスからプロパティを取得する
            PropertyInfo[] props_json_camera_general = typeof(CCameraGeneral).GetProperties(b_access_flag);
            string str_comment_contents = "";           // コメント内容
            int i_num_prop_name;                        // コメントコードの位置を示すint型データ
            int i_num_enter;                            // 改行コードの位置を示すint型データ
            foreach (PropertyInfo prop in props_json_camera_general)
            {
                // 各プロパティのコメントを設定する
                switch (prop.Name)
                {
                    case "Number":
                        str_comment_contents = "カメラ台数";
                        break;
                    case "BoardType":
                        str_comment_contents = "ボードタイプ";
                        break;
                    case "HeartBeatTime":
                        str_comment_contents = "最大カメラ取得待機時間(単位は秒、指定秒数以上カメラから画像が取得できないとエラーとする)";
                        break;
                    case "CameraInformation":
                        str_comment_contents = "カメラ毎の固有情報";
                        break;
                    default:
                        str_comment_contents = "";
                        break;
                }
                // 指定プロパティ名直後改行コードの直前にコメント追加する
                i_num_prop_name = nstrJsonContents.IndexOf(prop.Name);
                if (i_num_prop_name != -1)
                {
                    i_num_enter = nstrJsonContents.IndexOf(m_strNewLineCode, i_num_prop_name);
                    nstrJsonContents = $"{nstrJsonContents.Substring(0, i_num_enter)}          {m_strCommentCode} {str_comment_contents}{nstrJsonContents.Substring(i_num_enter, nstrJsonContents.Length - (i_num_enter))}";
                }

            }
            // 詳細設定項目クラスからプロパティを取得する
            PropertyInfo[] props_json_camera_info = typeof(CCameraInfo).GetProperties(b_access_flag);
            foreach (PropertyInfo prop in props_json_camera_info)
            {
                // 各プロパティのコメントを設定する
                switch (prop.Name)
                {
                    case "IdentifyName":
                        str_comment_contents = "カメラ固有ネーム";
                        break;
                    case "CameraType":
                        str_comment_contents = "現在未使用";
                        break;
                    case "CameraFile":
                        str_comment_contents = "dcfファイル名";
                        break;
                    case "Width":
                        str_comment_contents = "画像の幅";
                        break;
                    case "Height":
                        str_comment_contents = "画像の高さ";
                        break;
                    case "Color":
                        str_comment_contents = "現在未使用";
                        break;
                    case "ImagePose":
                        str_comment_contents = "現在未使用";
                        break;
                    case "UseSerialComm":
                        str_comment_contents = "現在未使用";
                        break;
                    case "COMNo":
                        str_comment_contents = "現在未使用";
                        break;
                    case "IPAddress":
                        str_comment_contents = "カメラのIPアドレス";
                        break;
                    default:
                        str_comment_contents = "";
                        break;
                }
                // 指定プロパティ名直後改行コードの直前にコメント追加する
                i_num_prop_name = nstrJsonContents.IndexOf(prop.Name);
                if (i_num_prop_name != -1)
                {
                    i_num_enter = nstrJsonContents.IndexOf(m_strNewLineCode, i_num_prop_name);
                    nstrJsonContents = $"{nstrJsonContents.Substring(0, i_num_enter)}          {m_strCommentCode} {str_comment_contents}{nstrJsonContents.Substring(i_num_enter, nstrJsonContents.Length - (i_num_enter))}";
                }
            }
        }
    }
}
