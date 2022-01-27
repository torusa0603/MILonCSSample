namespace Log
{
    class CLogParameter
    {
        // ファイルを新規作成するタイミング デフォルトは"Day"。
        //Add:常に1つのファイルを上書き更新 Day,Month:日付/月が変わったら新規ファイルを作成する
        public string UpdateTime { get; set; } = "Day";

        // 古いファイルを削除するタイミング デフォルトは"None"。
        //None:削除しない Month,Half,Year:1月・半年・1年前のファイルは削除する
        public string DeleteTime { get; set; } = "None";

        // ファイル名(Error,Oparate等)
        public string FileName { get; set; } = "";
    }
}