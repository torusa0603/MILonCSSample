using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS.Parameter
{
    class CCameraGeneral
    {
        public int Number { get; set; }         // カメラ個数
        public int BoardType { get; set; }      // ボード種類 
        public int HeartBeatTime { get; set; } // ハートビート時間(単位:s)
        public List<CCameraInfo> CameraInformation { get; private set; } = new List<CCameraInfo>(); // カメラの詳細情報
    }
}
