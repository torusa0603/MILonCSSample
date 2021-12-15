using MatroxCS.Parameter;

namespace MatroxCS
{
    abstract class CParameterBase
    {
        public static CCameraGeneral m_scCameraGeneral = new CCameraGeneral();     // カメラ情報

        /// <summary>
        /// 設定ファイルの内容を設定用オブジェクトに格納
        /// </summary>
        /// <param name="nstrSettingFilePath">設定ファイルパス</param>
        public abstract int ReadParameter(string nstrSettingFilePath);

    }

    
}
