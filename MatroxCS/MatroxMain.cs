using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using Newtonsoft.Json;
using System.IO;

namespace MatroxCS
{

    //  dll使う人は基本全部ここを通るからこのファイル(クラス)長くなる。
    //  機能毎にファイル分けたほうがいいかも。

    public class CMatroxMain
    {
        #region メンバー変数

        List<CCamera> m_lstCamera = new List<CCamera>();      //  カメラオブジェクト
        List<CDisplayImage> m_lstDisplayImage = new List<CDisplayImage>();    //  ディスプレイオブジェクト
        CGraphic m_cGraphic = new CGraphic();   //  グラフィックオブジェクト

        CJsonCameraGeneral m_cJsonCameraGeneral = new CJsonCameraGeneral();

        //  パターンマッチング
        //  フィルター
        //  各種アルゴリズム(?)これはSPVIみたいにする？
        //  描画

        #endregion

        /// <summary>
        /// Matrox制御の初期化
        /// </summary>
        /// <returns></returns>
        public int initMatrox()
        {
            readParameter("");
            int i_camera_num = m_cJsonCameraGeneral.Number;   //  適当

            //  設定ファイル読む。この設定ファイルは人が書くので人が読み書きしやすい必要あり
            //  でも設定ファイルにはカメラ情報しかないからCCmaeraクラスでやればいいか?
            //  でもカメラ数は知らないとダメ

            //  カメラ初期化
            for(int i_loop = 0; i_loop < i_camera_num; i_loop++)
            {
                CCamera c_camera = new CCamera(m_cJsonCameraGeneral.CameraInformation[i_loop]);
                c_camera.OpenCamera(i_loop);
                m_lstCamera.Add(c_camera);
            }


            return 0;
        }

        /// <summary>
        /// カメラ数取得
        /// </summary>
        /// <returns></returns>
        public int GetCameraNum()
        {
            return m_lstCamera.Count();
        }

        /// <summary>
        /// カメラIDを取得
        /// </summary>
        /// <returns></returns>
        public int GetCameraID(int niCameraIndex)
        {
            return m_lstCamera[niCameraIndex].GetID();
        }

        /// <summary>
        /// スルーを実行
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <returns>-1:該当カメラID無し</returns>
        public int Through(int niCameraID)
        {
            //  このカメラIDのオブジェクトを探す
            //  探すのはカメラだけでなくディスプレイとかもあるので1行で済ませたい
            int i_camera_index = SearchCameraID(niCameraID);
            if (i_camera_index == -1)
            {
                return -1;
            }
            m_lstCamera[i_camera_index].Through();

            return 0;
        }

        
        /// <summary>
        /// ディスプレイオープン。
        /// </summary>
        /// <param name="nhHandle"></param>
        /// <returns>ディスプレイID</returns>
        public int OpenDisplay(IntPtr nhHandle)
        {
            int i_display_id;
            CDisplayImage c_display = new CDisplayImage();
            c_display.OpenDisplay(nhHandle);
            m_lstDisplayImage.Add(c_display);
            i_display_id = c_display.GetID();

            return i_display_id;
        }

        /// <summary>
        /// カメラ画像を写すディスプレイを選択する
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>-1:該当カメラID・該当ディスプレイID無し、-2:該当カメラID無し、-3:該当ディスプレイID無し</returns>
        public int SelectCameraImageDisplay(int niCameraID, int niDisplayID)
        {
            int i_camera_index = 0;
            int i_display_index = 0;

            //まずそれぞれのIDがあることを確認。なければエラー
            i_camera_index = SearchCameraID(niCameraID);
            i_display_index = SearchDisplayID(niCameraID);
            if (i_camera_index == -1 || i_display_index == -1)
            {
                if (i_camera_index == -1)
                {
                    if (i_display_index == -1)
                    {
                        return -1;
                    }
                    else
                    {
                        return -2;
                    }
                }
                else
                {
                    return -3;
                }
                
            }
            //  カメラの画像サイズ取得
            Size sz = m_lstCamera[i_camera_index].GetImageSize();
            //  このサイズでディスプレイの画像を作成する
            m_lstDisplayImage[i_display_index].CreateImage(sz);
            // 表示用画像バッファをカメラに渡す
            m_lstCamera[i_camera_index].SetShowImage(m_lstDisplayImage[i_display_index].GetShowImage());

            return 0;
        }

        /// <summary>
        /// 表示用ディスプレイを削除
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>-1:該当ディスプレイID無し</returns>
        public int DeleteDisplay(int niDisplayID)
        {
            int i_display_index = SearchDisplayID(niDisplayID); ;
            //  指定のIDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            //  メモリ解放
            m_lstDisplayImage[i_display_index].CloseDisplay();
            // Listから削除
            m_lstDisplayImage.RemoveAt(i_display_index);

            return 0;
        }

        /// <summary>
        /// 画像をロードする
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>-1:該当ディスプレイID無し</returns>
        public int LoadImage(string nstrImageFilePath, int niDisplayID)
        {
            int i_display_index = SearchDisplayID(niDisplayID); ;
            //  指定のIDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            m_lstDisplayImage[i_display_index].LoadImage(nstrImageFilePath);

            return 0;
        }

        /// <summary>
        /// グラフィック色の設定
        /// </summary>
        /// <param name="nGraphicColor"></param>
        /// <returns></returns>
        public int SetGraphicColor(Color nGraphicColor)
        {

            //  RGBの値に分割して設定
            m_cGraphic.SetColor(nGraphicColor.R, nGraphicColor.G, nGraphicColor.B);


            return 0;
        }

        /// <summary>
        /// 直線を描画
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <param name="nptStartPoint"></param>
        /// <param name="nptEndPoint"></param>
        /// <returns>-1:該当ディスプレイID無し</returns>
        public int DrawLine(int niDisplayID, Point nptStartPoint, Point nptEndPoint)
        {
            int i_display_index = SearchDisplayID(niDisplayID); ;
            //  指定のIDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            //  指定の画面のオーバーレイバッファを設定
            m_cGraphic.SetOverlay(m_lstDisplayImage[i_display_index].GetOverlay());
            //  ここに直線を描画
            

            return 0;
        }

        /// <summary>
        /// 画像を保存
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <param name="nstrExt"></param>
        /// <param name="nbIncludeGraphic"></param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>-1:該当ディスプレイID無し</returns>
        public int SaveImage(string nstrImageFilePath, string nstrExt, bool nbIncludeGraphic, int niDisplayID)
        {
            int i_display_index = SearchDisplayID(niDisplayID); ;
            //  指定のIDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            //  拡張子に応じたフォーマットで保存。グラフィックを含むか含まないかも設定出来るように

            return 0;

        }

        /// <summary>
        /// カメラIDに対応するインデックス番号を探す
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <returns>0以上の番号:該当インデックス番号、-1:該当カメラID無し</returns>
        private int SearchCameraID(int niCameraID)
        {
            int i_index = 0;
            foreach (CCamera camera in m_lstCamera)
            {
                if (camera.GetID() == niCameraID)
                {
                    break;
                }
                i_index++;
            }
            if (m_lstCamera.Count == i_index)
            {
                return -1;
            }
            return i_index;
        }

        /// <summary>
        /// ディスプレイIDに対応するインデックス番号を探す
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0以上の番号:該当インデックス番号、-1:該当カメラID無し</returns>
        private int SearchDisplayID(int niDisplayID)
        {
            int i_index = 0;
            foreach (CDisplayImage displayimage in m_lstDisplayImage)
            {
                if (displayimage.GetID() == niDisplayID)
                {
                    break;
                }
                i_index++;
            }
            if (m_lstDisplayImage.Count == i_index)
            {
                return -1;
            }
            return i_index;
        }

        protected int readParameter(string nstrSettingPath)
        {
            string str_jsonfile_contents = File.ReadAllText(nstrSettingPath);

            m_cJsonCameraGeneral = JsonConvert.DeserializeObject<CJsonCameraGeneral>(str_jsonfile_contents);

            return 0;
        }


    }

    class CJsonCameraGeneral
    {
        public int Number { get; set; }
        public List<CJsonCameraInfo> CameraInformation { get; private set; } = new List<CJsonCameraInfo>();
    }

    class CJsonCameraInfo
    {
        public string IdentifyName { get; set; }
        public int CameraType { get; set; }
        public string CameraFile { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Color { get; set; }
        public int ImagePose { get; set; }
        public int UseSerialComm { get; set; }
        public int COMNo { get; set; }
        public string IPAddress { get; set; }
    }
}
