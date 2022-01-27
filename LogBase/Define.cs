using System.Text;

namespace Log
{
    public static class CDefine
    {
        public const string FolderName = "Log\\";                                         // フォルダ名
        public const string EXTENSION = ".log";                                         //logファイルの拡張子
        public static readonly Encoding ENCODING = Encoding.GetEncoding("Shift_JIS");   // 文字列エンコーダ

        public enum UPDATE_TIME
        {
            Add,
            Day,
            Month,
        }

        public enum DELETE_TIME
        {
            None,
            Month,
            Half,
            Year,
        }
    }
}
