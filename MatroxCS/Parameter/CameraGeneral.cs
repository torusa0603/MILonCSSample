using System;
using System.Collections.Generic;

namespace MatroxCS.Parameter
{
    /// <summary>
    /// カメラに関する全般設定項目
    /// </summary>
    class CCameraGeneral
    {
        public int Number { get; set; }         // カメラ個数
        public int BoardType { get; set; }      // ボード種類 
        public int HeartBeatTime { get; set; } // ハートビート時間(単位:s)
        public List<CCameraInfo> CameraInformation { get; private set; } = new List<CCameraInfo>(); // カメラの詳細情報
    }

    class CCameraGeneralLimit
    {
        public static int Number_Limit = 10;           // カメラ個数の上限
        public static int HeartBeatTime_Limit = 10;    // ハートビート時間の上限
    }
}
