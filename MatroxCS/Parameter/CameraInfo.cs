namespace MatroxCS.Parameter
{
    /// <summary>
    /// 個々のカメラに対する設定項目
    /// </summary>
    class CCameraInfo: IParameter
    {
        /// <summary>
        /// 識別ネーム
        /// </summary>
        public string IdentifyName { get; set; }

        /// <summary>
        /// 現在未使用
        /// </summary>
        public int CameraType { get; set; }

        /// <summary>
        /// DCFファイルパス
        /// </summary>
        public string CameraFile { get; set; }

        /// <summary>
        /// 取得画像幅
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 取得画像高さ
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 現在未使用
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// 現在未使用
        /// </summary>
        public int ImagePose { get; set; }

        /// <summary>
        /// 現在未使用
        /// </summary>
        public int UseSerialComm { get; set; }

        /// <summary>
        /// 現在未使用
        /// </summary>
        public int COMNo { get; set; }

        /// <summary>
        /// gigeカメラのIPアドレス
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// パラメータの有効性を検査
        /// </summary>
        /// <returns>0:異常なし、-3:取得画像幅異常、-4:取得画像高さ異常</returns>
        public int CheckVariableValidity()
        {
            // 取得画像幅をチェック
            if (Width < 0 || Width > CDefine.CCameraInfoLimit.WIDTH)
                return -3;

            // 取得画像高さをチェック
            if (Height < 0 || Height > CDefine.CCameraInfoLimit.HEIGHT)
                return -4;
            return 0;
        }

        public void Dispose()
        {

        }
    }
}
