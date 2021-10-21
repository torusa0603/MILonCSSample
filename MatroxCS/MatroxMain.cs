﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;

namespace MatroxCS
{

    //  dll使う人は基本全部ここを通るからこのファイル(クラス)長くなる。
    //  機能毎にファイル分けたほうがいいかも。

    class CMatroxMain
    {
        #region メンバー変数

        List<CCamera> m_lstCamera = new List<CCamera>();      //  カメラオブジェクト
        List<CDisplayImage> m_lstDisplayImage = new List<CDisplayImage>();    //  ディスプレイオブジェクト
        CGraphic m_cGraphic = new CGraphic();   //  グラフィックオブジェクト

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
            int i_camera_num = 2;   //  適当

            //  設定ファイル読む。この設定ファイルは人が書くので人が読み書きしやすい必要あり
            //  でも設定ファイルにはカメラ情報しかないからCCmaeraクラスでやればいいか?
            //  でもカメラ数は知らないとダメ

            //  カメラ初期化
            for(int i_loop = 0; i_loop < i_camera_num; i_loop++)
            {
                CCamera c_camera = new CCamera();
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
        /// <param name="niCameraIndex"></param>
        /// <returns></returns>
        public int Through(int niCameraID)
        {
            //  このカメラIDのオブジェクトを探す
            //  探すのはカメラだけでなくディスプレイとかもあるので1行で済ませたい
            int i_index = 0;
            foreach(CCamera camera in m_lstCamera)
            {
                if (camera.GetID() == niCameraID)
                {
                    break;
                }
                i_index++;
            }
            m_lstCamera[i_index].Through();

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
        /// <param name="niCameraID"></param>
        /// <param name="niDisplayID"></param>
        /// <returns></returns>
        public int SelectCameraImageDisplay(int niCameraID, int niDisplayID)
        {
            int i_camera_index = 0;
            int i_display_index = 0;

            //まずそれぞれのIDがあることを確認。なければエラー

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
        /// <returns></returns>
        public int DeleteDisplay(int niDisplayID)
        {
            int i_display_index = 0;
            //  指定のIDのオブジェクトがなければエラー

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
        /// <returns></returns>
        public int LoadImage(string nstrImageFilePath, int niDisplayID)
        {
            int i_display_index = 0;
            //  指定のIDのオブジェクトがなければエラー

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
        /// <param name="niDisplayID"></param>
        /// <param name="nptStartPoint"></param>
        /// <param name="nptEndPoint"></param>
        /// <returns></returns>
        public int DrawLine(int niDisplayID, Point nptStartPoint, Point nptEndPoint)
        {
            int i_display_index = 0;
            //  指定のIDのオブジェクトがなければエラー

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
        /// <param name="niDisplayID"></param>
        /// <returns></returns>
        public int SaveImage(string nstrImageFilePath, string nstrExt, bool nbIncludeGraphic, int niDisplayID)
        {
            int i_display_index = 0;
            //  指定のIDのオブジェクトがなければエラー

            //  拡張子に応じたフォーマットで保存。グラフィックを含むか含まないかも設定出来るように

            return 0;

        }



    }
}
