using System;
using System.Collections.Generic;

namespace MatroxCS.Parameter
{
    /// <summary>
    /// カメラに関する全般設定項目
    /// </summary>
    class CCameraGeneral: IParameter
    {
        /// <summary>
        /// カメラ個数
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// ボード種類 
        /// </summary>
        public int BoardType { get; set; }

        /// <summary>
        /// ハートビート時間(単位:s)
        /// </summary>
        public int HeartBeatTime { get; set; }

        /// <summary>
        /// カメラの詳細情報
        /// </summary>
        public List<CCameraInfo> CameraInformation { get; private set; } = new List<CCameraInfo>();

        /// <summary>
        /// パラメータの有効性を検査
        /// </summary>
        /// <returns>0:異常なし、-1:カメラ個数異常、-2:ハートビート時間異常、-3:取得画像幅異常、-4:取得画像高さ異常</returns>
        public int CheckVariableValidity()
        {
            int i_ret;
            // カメラ個数をチェック
            if (Number < 0 || Number > CDefine.CCameraGeneralLimit.MAX_CAMERA_NUMBER)
                return -1;

            // ハートビート時間をチェック
            if (HeartBeatTime < 0 || HeartBeatTime > CDefine.CCameraGeneralLimit.MAX_HEART_BEAT_TIME)
                return -2;

            // カメラ詳細情報をチェック
            foreach (CCameraInfo camera_info in CameraInformation)
            {
                i_ret = camera_info.CheckVariableValidity();
                // 戻り値(0:異常なし、-3:取得画像幅異常、-4:取得画像高さ異常)
                if (i_ret != 0)
                    return i_ret;
            }
            return 0;
        }

        public class Comment
        {
            public static string Number = "カメラ台数";

            public static string BoardType = "ボードタイプ(0: MORPHIS、1: SOLIOSXCL、2: SOLIOSXA、3: METEOR2MC、4: GIGE、100: HOST)";

            public static string HeartBeatTime = "最大カメラ取得待機時間(単位は秒、指定秒数以上カメラから画像が取得できないとエラーとする)";

            public static string CameraInformation = "カメラ毎の固有情報";
        }

        public void Dispose()
        {

        }
    }
}
