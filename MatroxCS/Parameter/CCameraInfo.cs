using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS.Parameter
{
    class CCameraInfo
    {
        public string IdentifyName { get; set; }    // 識別ネーム
        public int CameraType { get; set; }         // 現在未使用
        public string CameraFile { get; set; }      // DCFファイルパス
        public int Width { get; set; }              // 取得画像幅
        public int Height { get; set; }             // 取得画像高さ
        public int Color { get; set; }              // 現在未使用
        public int ImagePose { get; set; }          // 現在未使用
        public int UseSerialComm { get; set; }      // 現在未使用
        public int COMNo { get; set; }              // 現在未使用
        public string IPAddress { get; set; }       // gigeカメラのIPアドレス
    }
}
