using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
namespace MatroxCS
{
    public class CBase
    {

        //  全てのMIL操作共通のものはCBaseで定義する
        static protected MIL_ID m_smilApplication = MIL.M_NULL; 
        static protected MIL_ID m_smilSystem = MIL.M_NULL;

        //  各インスタンスIDオフセット

        static protected int m_siIDMaxLength = 10000;     //  各IDのmax個数。
        //  カメラID
        static protected int m_siCameraOffsetID = 10000;      //カメラIDオフセット
        static protected int m_siNextCameraID   = 10000;      //10000～19999まで。これを循環して使用
        //  画面表示ID
        static protected int m_siDisplayOffsetID = 20000;     //画面表示IDオフセット
        static protected int m_siNextDisplayID  = 20000;     //20000～29999まで。これを循環して使用



        public int initial()
        {
            //色々初期化
            //  設定ファイル読んだり？
            return 0;
        }


    }
}
