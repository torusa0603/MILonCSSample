using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;

namespace MatroxCS
{
    class CDisplayImage : CBase
    {
        #region メンバー変数
        //  ディスプレイID
        private MIL_ID m_milDisplay = MIL.M_NULL;
        //  グラフィックを描画するためのオーバーレイバッファ
        private MIL_ID m_milOverlay = MIL.M_NULL;
        //  画面に表示するときの画像バッファ。常にカラーバッファ
        private MIL_ID m_milDisplayImage = MIL.M_NULL;
        //  画面に表示させる画像のサイズ
        private Size m_szImageSize;
        //  ウインドウハンドル
        private IntPtr m_hDisplayHandle = IntPtr.Zero;
        //  ディスプレイインスタンスID
        private int m_iDisplayID;

        private bool m_bOpenDone = false;   //  オープンされたか否か

        private double m_dMagRate = 1.0;    //  現在の画像拡大の倍率(100%=1.0)

        private int? m_iConnectCameraID;

        private const double m_cdMinMagRate = 0.1;//    最小倍率(10%)
        private const double m_cdMaxMagRate = 8.0;//    最大倍率(800%)
        

        #endregion


        #region パブリック関数

        /// <summary>
        /// ディスプレイのオープン
        /// </summary>
        /// <param name="nhDisplayHandle"></param>
        /// <returns></returns>
        public int OpenDisplay(IntPtr nhDisplayHandle, Size nDisplaySize)
        {
            //  既にオープンされたインスタンスならエラー
            if (m_bOpenDone == true)
            {
                return -999;
            }

            m_szImageSize = nDisplaySize;

            //  ディスプレイオープン
            MIL.MdispAlloc(m_smilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref m_milDisplay);
            if (m_milDisplay == MIL.M_NULL)
            {
                return -1;
            }

            //  ディスプレイの各種設定。C++のImageMatrox.dllとか見ればわかる
            MIL.MdispControl(m_milDisplay, MIL.M_INTERPOLATION_MODE, MIL.M_NEAREST_NEIGHBOR);
            MIL.MdispControl(m_milDisplay, MIL.M_OVERLAY, MIL.M_ENABLE);
            MIL.MdispControl(m_milDisplay, MIL.M_OVERLAY_SHOW, MIL.M_ENABLE);
            MIL.MdispControl(m_milDisplay, (long)MIL.M_TRANSPARENT_COLOR, m_milintTransparentColor);

            
            MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDisplayImage);
            MIL.MbufClear(m_milDisplayImage, 0);
            //MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milOverlay);
            //MIL.MbufClear(m_milOverlay, 0);
            
            //  ハンドルをコピー
            m_hDisplayHandle = nhDisplayHandle;

            //  ディスプレイが無事オープン出来たらIDを割り当てる
            m_iDisplayID = m_siNextDisplayID;
            //  次のディスプレイで被らないようにインクリメントしておく
            m_siNextDisplayID++;
            //  IDが最大値まで行ったらリセットする
            if (m_siNextDisplayID >= m_siDisplayOffsetID + m_siIDMaxLength)
            {
                m_siNextDisplayID = m_siDisplayOffsetID;
            }

            

            //  オープンフラグを立てる
            m_bOpenDone = true;

            return 0;
        }

        /// <summary>
        /// ディスプレイID取得
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return m_iDisplayID;
        }

        /// <summary>
        /// 指定の画像サイズの画像バッファを作成する
        /// </summary>
        /// <param name="niImageSize"></param>
        /// <returns></returns>
        public int CreateImage(Size niImageSize)
        {
            //  そもそも同じサイズで確定していれば何もしない
            if (m_szImageSize != niImageSize)
            {
                //  違ければ今の画像バッファをクリアしてこの大きさで再確保
                MIL.MbufFree(m_milDisplayImage);
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDisplayImage);
            }
            //  ここでMdispSelectWindow( m_milDisp, m_milDisplayImage, nhDispHandle );
            MIL.MdispSelectWindow(m_milDisplay, m_milDisplayImage, m_hDisplayHandle);
            //  MdispSelectWindowのあとはオーバーレイバッファを確保
            MIL.MdispInquire(m_milDisplay, MIL.M_OVERLAY_ID, ref m_milOverlay);

            m_szImageSize = niImageSize;
            

            return 0;
        }

        /// <summary>
        /// 画面表示用バッファを取得
        /// </summary>
        /// <returns></returns>
        public MIL_ID GetShowImage(int niConnectCameraID)
        {
            m_iConnectCameraID = niConnectCameraID;
            return m_milDisplayImage;
        }

        public int? GetConnectCameraID()
        {
            if (m_iConnectCameraID == null)
            {
                return null;
            }
            else
            {
                return m_iConnectCameraID;
            }
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        /// <returns></returns>
        public Size GetImageSize()
        {
            return m_szImageSize;
        }

        /// <summary>
        /// ディスプレイ削除
        /// </summary>
        public int CloseDisplay()
        {
            //  ディスプレイID、オーバーレイバッファ、表示用画像バッファ等を開放。nullに
            if (m_milDisplayImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milDisplayImage);
                m_milDisplayImage = MIL.M_NULL;
            }
            if (m_milDisplay != MIL.M_NULL)
            {
                MIL.MdispFree(m_milDisplay);
                m_milDisplay = MIL.M_NULL;
            }
            //if (m_milOverlay != MIL.M_NULL)
            //{
            //    MIL.MbufFree(m_milOverlay);
            //    m_milOverlay = MIL.M_NULL;
            //}
            return 0;
        }

        /// <summary>
        /// 画像ファイルをロードする
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <returns></returns>
        public int LoadImage(string nstrImageFilePath)
        {

            //  MIL関数でロード
            if (m_milDisplay != MIL.M_NULL)
            {
                //  もし既に画像バッファを確保していたら、一度開放してから再度確保する
                MIL.MbufFree(m_milDisplayImage);
                //  画像サイズも更新
                MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDisplayImage);
            }
            //  ここでMdispSelectWindow( m_milDisp, m_milDisplayImage, nhDispHandle );
            MIL.MdispSelectWindow(m_milDisplay, m_milDisplayImage, m_hDisplayHandle);
            MIL.MbufImport(nstrImageFilePath, MIL.M_DEFAULT, MIL.M_LOAD, m_smilSystem, ref m_milDisplayImage);
            //  ↑毎回やって大丈夫かな？
            //  MdispSelectWindowのあとはオーバーレイバッファを確保
            MIL.MdispInquire(m_milDisplay, MIL.M_OVERLAY_ID, ref m_milOverlay);

            return 0;
        }

        /// <summary>
        /// 倍率切替
        /// </summary>
        /// <param name="ndMagRate"></param>
        /// <returns></returns>
        public int SetMagRate(double ndMagRate)
        {
            //  最大最小で丸める!
            if (ndMagRate < m_cdMinMagRate)
            {
                ndMagRate = m_cdMinMagRate;
            }
            if (ndMagRate > m_cdMaxMagRate)
            {
                ndMagRate = m_cdMaxMagRate;
            }
            //  倍率を切り替える
            MIL.MdispZoom(m_milDisplay, ndMagRate, ndMagRate);
            m_dMagRate = ndMagRate;
            return 0;
        }

        /// <summary>
        /// 倍率を取得する
        /// </summary>
        /// <returns></returns>
        public double GetMagRate()
        {
            return m_dMagRate;
        }

        /// <summary>
        /// オーバーレイバッファを取得する
        /// </summary>
        /// <returns></returns>
        public MIL_ID GetOverlay()
        {
            MIL.MdispSelectWindow(m_milDisplay, m_milDisplayImage, m_hDisplayHandle);
            MIL.MdispInquire(m_milDisplay, MIL.M_OVERLAY_ID, ref m_milOverlay);
            return m_milOverlay;
        }

        /// <summary>
        /// グラフィックをクリアする
        /// </summary>
        /// <returns></returns>
        public int ClearGraphic()
        {
            MIL.MdispControl( m_milDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_TRANSPARENT_COLOR );
            return 0;
        }

        /// <summary>
        /// 画像を保存
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <param name="nstrExt"></param>
        /// <param name="nbIncludeGraphic"></param>
        /// <returns>-1:拡張子エラー</returns>
        public int SaveImage(string nstrImageFilePath, bool nbIncludeGraphic)
        {
            MIL_ID mil_temp = MIL.M_NULL;
            MIL_ID mil_result_temp = MIL.M_NULL;
            int i_index_ext;
            string str_ext;

            MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref mil_result_temp);
            MIL.MbufCopy(m_milDisplayImage, mil_result_temp);
            if (nbIncludeGraphic)
            {
                //	オーバーレイバッファと検査結果画像バッファの一時バッファを用意
                MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref mil_temp);
                //	一時バッファに画像をコピー
                MIL.MbufCopy(m_milOverlay, mil_temp);
                //	オーバーレイを検査結果画像上にコピー
                MIL.MbufTransfer(mil_temp, mil_result_temp, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_COMPOSITION, MIL.M_DEFAULT, m_milintTransparentColor, MIL.M_NULL);
                //	メモリ開放
                MIL.MbufFree(mil_temp);
            }
            else
            {
                
            }

            //	拡張子を抽出
            i_index_ext = nstrImageFilePath.IndexOf(".");
            //	拡張子がない場合は仕方ないのでビットマップの拡張子をつけてビットマップで保存する
            if (i_index_ext < 0)
            {
                nstrImageFilePath += ".bmp";
                str_ext = "bmp";
            }
            else
            {
                //	ファイル名の最後の文字が「.」だった場合もビットマップにしてしまう
                if (i_index_ext + 1 == nstrImageFilePath.Length)
                {
                    nstrImageFilePath += "bmp";
                    str_ext = "bmp";
                }
                else
                {
                    str_ext = nstrImageFilePath.Substring(i_index_ext + 1);
                }
            }
            //	jpg
            if (string.Compare(str_ext, "jpg") == 0 || string.Compare(str_ext, "JPG") == 0)
            {
                MIL.MbufExport(nstrImageFilePath, MIL.M_JPEG_LOSSY, mil_result_temp);
            }
            //	png
            else if (string.Compare(str_ext, "png") == 0 || string.Compare(str_ext, "PNG") == 0)
            {
                MIL.MbufExport(nstrImageFilePath, MIL.M_PNG, mil_result_temp);
            }
            //	bmp
            else if (string.Compare(str_ext, "bmp") == 0 || string.Compare(str_ext, "BMP") == 0)
            {
                MIL.MbufExport(nstrImageFilePath, MIL.M_BMP, mil_result_temp);
            }
            else
            {
                MIL.MbufFree(mil_result_temp);
                return -1;
            }

            //	メモリ開放
            MIL.MbufFree(mil_result_temp);

            return 0;
        }


        #endregion
    }
}
