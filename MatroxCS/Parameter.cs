using MatroxCS.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace MatroxCS
{
    class CParameter
    {
        string m_strCommentCode = "###";    // コメントコード
        string m_strNewLineCode = "\r\n";   // 改行コード

        /// <summary>
        /// 設定ファイルの内容を設定用オブジェクトに格納
        /// </summary>
        /// <param name="nstrSettingFilePath">設定ファイルパス</param>
        /// <returns>0:正常終了、-1:設定ファイルパスの途中ディレクトリが存在しない、-2:設定ファイル作成・書き込みエラー、-3:設定ファイルなし(新規作成)<br />
        /// -4:設定ファイル構文エラー、-5:設定値エラー</returns>
        public int ReadParameter(string nstrSettingFilePath, ref CCameraGeneral ncCameraGeneral)
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
            i_ret = CJsonParameterFactory<CCameraGeneral>.Load(nstrSettingFilePath, ref ncCameraGeneral);
            switch (i_ret)
            {
                case -1:
                    // ファイルへのアクセス失敗
                    return -4;
                case -2:
                    // 設定ファイル構文エラー
                    return -5;
                case -3:
                    // 異常値が代入された
                    ncCameraGeneral = null;
                    return -6;
                default:
                    break;
            }
            return 0;
        }

        /// <summary>
        /// 設定ファイルを作成する
        /// </summary>
        /// <param name="nstrSettingFilePath">作成ファイルパス</param>
        /// <returns>0:正常終了、-1:設定ファイルパスの途中ディレクトリーが存在しない、-2:ファイル作成・書き込みエラー</returns>
        private int CreateSettingFile(string nstrSettingFilePath)
        {
            // 作成ファイルパスのディレクトリの存在チェック
            string str_setting_file_folder = Path.GetDirectoryName(nstrSettingFilePath);
            if (!Directory.Exists(str_setting_file_folder))
            {
                return -1;
            }

            Encoding encd_encoding = Encoding.GetEncoding("utf-8");
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
            
            // int i_ret = CJsonParameterFactory<CCameraGeneral>.Save(nstrSettingFilePath, c_json_camera_general);  ←後に実装する
            // パラメータをシリアライズする
            string str_json_contents = JsonConvert.SerializeObject(c_json_camera_general, Formatting.Indented);
            // パラメータ文字列にコメントを追加する
            AddDescriptionOfParameter(ref str_json_contents);

            try
            {
                // jsonファイルを作成する
                using (FileStream fs = File.Create(nstrSettingFilePath)) { }
                // jsonファイルにパラメータ文字列を書き込む
                using (StreamWriter writer = new StreamWriter(nstrSettingFilePath, false, encd_encoding))
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
                        str_comment_contents = "ボードタイプ(0: MORPHIS、1: SOLIOSXCL、2: SOLIOSXA、3: METEOR2MC、4: GIGE、100: HOST)";
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
