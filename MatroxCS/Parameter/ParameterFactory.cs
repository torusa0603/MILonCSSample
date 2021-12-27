using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Reflection;
using MatroxCS.Parameter;

namespace MatroxCS.Parameter
{

    public class CJsonParameterFactory<T> where T : IParameter
    {
        const string m_strCommentCode = "###";    // コメントコード
        const string m_strNewLineCode = "\r\n";   // 改行コード

        /// <summary>
        /// Json形式ファイルからパラメータ値を抜き出し、クラス変数に代入する
        /// </summary>
        /// <param name="nstrFilePath"></param>
        /// <param name="ncParameter"></param>
        /// <returns>-1:ファイルからの読み込み失敗、-2:json構文エラー、-3:異常値が代入された</returns>
        public static int Load(string nstrFilePath, ref T ncParameter)
        {
            string str_jsonfile_sentence;
            int i_ret;
            try
            {
                // ファイルから文字列を丸ごと抜き出す
                str_jsonfile_sentence = File.ReadAllText(nstrFilePath);
            }
            catch
            {
                // ファイルからの読み込み失敗
                return -1;
            }
            // 文章内のコメントコード～改行コード間にある文とコメントコードを削除する
            string str_jsonfile_sentence_commentout = RemoveComment(str_jsonfile_sentence);
            try
            {
                // コメントアウトの箇所を削除した文字列をデシリアライズする
                ncParameter = JsonConvert.DeserializeObject<T>(str_jsonfile_sentence_commentout);
            }
            catch
            {
                // json構文エラー
                return -2;
            }
            i_ret = ncParameter.CheckVariableValidity();
            if (i_ret != 0)
            {
                // 異常値が代入された

                return -3;
            }
            return 0;
        }

        public static int Save(string nstrFilePath, T ncParameter)
        {
            Encoding encd_encoding = Encoding.GetEncoding("utf-8");
            // パラメータをシリアライズする
            string str_json_contents = JsonConvert.SerializeObject(ncParameter, Formatting.Indented);
            // パラメータ文字列にコメントを追加する
            AddDescriptionOfParameter(ref str_json_contents);

            try
            {
                // jsonファイルを作成する
                using (FileStream fs = File.Create(nstrFilePath)) { }
                // jsonファイルにパラメータ文字列を書き込む
                using (StreamWriter writer = new StreamWriter(nstrFilePath, false, encd_encoding))
                {
                    writer.WriteLine(str_json_contents);
                }
                return 0;
            }
            catch
            {
                // ファイル作成・書き込みエラー
                return -1;
            }
        }

        /// <summary>
        /// "###"ー"改行コード(\r\n)"間の文字を排除する
        /// </summary>
        /// <param name="n_strJsonfileContents">Jsonファイルから読み込んだstring型データ</param>
        /// <returns>コメント削除結果</returns>
        private static string RemoveComment(string nstrJsonfileContents)
        {
            string str_result = "";                     // 返答用のstring型データ
            string str_contents = nstrJsonfileContents; // 主となるstring型データ
            string str_front = "";                      // コメントコードより前の文章を格納するstring型データ
            string str_back = "";                       // コメントコードより後の文章を格納するstring型データ
            int i_num_comment_code;                     // コメントコードの位置を示すint型データ
            int i_num_newline_code;                     // 改行コードの位置を示すint型データ

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
                i_num_newline_code = str_back.IndexOf(m_strNewLineCode);
                // コメントコード直後の改行コードより後ろの文を抽出
                str_contents = str_back.Substring(i_num_newline_code, str_back.Length - i_num_newline_code);
                // コメントコードよりも前の文を返答用データに追加
                str_result += str_front;
            }
            // コメントコードを含まない後半データを返答用データに追加
            str_result += str_contents;
            // 返答する
            return str_result;
        }

        /// <summary>
        /// 作成される設定ファイルにコメントを加える
        /// </summary>
        /// <param name="nstrParameterContents">json型をシリアライズした文字列</param>
        private static void  AddDescriptionOfParameter(ref string nstrParameterContents)
        {
            // 取得するパラメータのアクセス修飾子を指定する
            BindingFlags b_access_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            // 一般設定項目クラスからプロパティを取得する
            List<PropertyInfo> list_props_json_camera_general = new List<PropertyInfo>(typeof(T).GetProperties(b_access_flag));
            List<MemberInfo> list_fields_json_camera_general = new List<MemberInfo>(typeof(T).GetMembers(b_access_flag));
            var c_comment = list_fields_json_camera_general.Where(member => member.Name == "Comment");
            string str_comment_contents = "";           // コメント内容
            int i_num_prop_name;                        // コメントコードの位置を示すint型データ
            int i_num_enter;                            // 改行コードの位置を示すint型データ
            foreach (PropertyInfo prop in list_props_json_camera_general)
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
                i_num_prop_name = nstrParameterContents.IndexOf(prop.Name);
                if (i_num_prop_name != -1)
                {
                    i_num_enter = nstrParameterContents.IndexOf(m_strNewLineCode, i_num_prop_name);
                    nstrParameterContents = $"{nstrParameterContents.Substring(0, i_num_enter)}          {m_strCommentCode} {str_comment_contents}{nstrParameterContents.Substring(i_num_enter, nstrParameterContents.Length - (i_num_enter))}";
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
                i_num_prop_name = nstrParameterContents.IndexOf(prop.Name);
                if (i_num_prop_name != -1)
                {
                    i_num_enter = nstrParameterContents.IndexOf(m_strNewLineCode, i_num_prop_name);
                    nstrParameterContents = $"{nstrParameterContents.Substring(0, i_num_enter)}          {m_strCommentCode} {str_comment_contents}{nstrParameterContents.Substring(i_num_enter, nstrParameterContents.Length - (i_num_enter))}";
                }
            }
        }
        }
}

