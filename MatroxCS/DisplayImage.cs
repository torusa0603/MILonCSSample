using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Reflection;

namespace MatroxCS
{
    class CDisplayImage : CBase
    {
        #region ローカル変数

        MIL_ID m_milDisplay = MIL.M_NULL;       // ディスプレイID
        MIL_ID m_milOverlay = MIL.M_NULL;       // グラフィックを描画するためのオーバーレイバッファ
        MIL_ID m_milDisplayImage = MIL.M_NULL;  // 画面に表示するときの画像バッファ。常にカラーバッファ
        Size m_szImageSize;                     // 画面に表示させる画像のサイズ
        IntPtr m_hDisplayHandle = IntPtr.Zero;  // ウインドウハンドル
        int m_iDisplayID;                       // ディスプレイインスタンスID
        int? m_iConnectCameraID;                // 現在接続しているカメラID

        bool m_bOpenDone = false;               // オープンされたか否か

        double m_dMagRate = 1.0;                // 現在の画像拡大の倍率(100%=1.0)
        const double m_cdMinMagRate = 0.1;      // 最小倍率(10%)
        const double m_cdMaxMagRate = 8.0;      // 最大倍率(800%)

        #endregion

        #region メンバ関数

        /// <summary>
        /// ディスプレイのオープン
        /// </summary>
        /// <param name="nhDisplayHandle">表示用ハンドル</param>
        /// <param name="nDisplaySize">ディスプレイサイズ</param>
        /// <returns>0:正常終了、-1:ディスプレイID取得失敗、-2:画像バッファ取得失敗、-3:既にオープンしている、-999:異常終了</returns>
        public int OpenDisplay(IntPtr nhDisplayHandle, Size nDisplaySize)
        {
            //  既にオープンされたインスタンスならエラー
            if (m_bOpenDone == true)
            {
                return -3;
            }
            try
            {
                // サイズを渡す
                m_szImageSize = nDisplaySize;
                //  ディスプレイオープン
                MIL.MdispAlloc(m_smilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref m_milDisplay);
                if (m_milDisplay == MIL.M_NULL)
                {
                    return -1;
                }

                //  ディスプレイの各種設定
                MIL.MdispControl(m_milDisplay, MIL.M_INTERPOLATION_MODE, MIL.M_NEAREST_NEIGHBOR);
                // オーバーレイバッファ使用可能に設定
                MIL.MdispControl(m_milDisplay, MIL.M_OVERLAY, MIL.M_ENABLE);
                // オーバーレイバッファ表示可能に設定
                MIL.MdispControl(m_milDisplay, MIL.M_OVERLAY_SHOW, MIL.M_ENABLE);
                // 透過色の設定
                MIL.MdispControl(m_milDisplay, (long)MIL.M_TRANSPARENT_COLOR, m_smilintTransparentColor);

                // 表示バッファを確保
                MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDisplayImage);
                if (m_milDisplayImage == MIL.M_NULL)
                {
                    return -2;
                }
                MIL.MbufClear(m_milDisplayImage, 0);
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
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// ディスプレイID取得
        /// </summary>
        /// <returns>ディスプレイID</returns>
        public int GetID()
        {
            return m_iDisplayID;
        }

        /// <summary>
        /// ディスプレイハンドル取得
        /// </summary>
        /// <returns>ディスプレイハンドル</returns>
        public IntPtr GetHandle()
        {
            return m_hDisplayHandle;
        }

        /// <summary>
        /// 指定の画像サイズの画像バッファを作成する
        /// </summary>
        /// <param name="niImageSize">指定画像サイズ</param>
        /// <returns>0:正常終了、-1:オーバレイバッファ取得失敗、-999:異常終了</returns>
        public int CreateImage(Size niImageSize)
        {
            try
            {
                //  そもそも同じサイズで確定していれば何もしない
                if (m_szImageSize != niImageSize)
                {
                    //  違ければ今の画像バッファをクリアしてこの大きさで再確保
                    MIL.MbufFree(m_milDisplayImage);
                    MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDisplayImage);
                    // 画像バッファサイズを更新
                    m_szImageSize = niImageSize;
                }
                //  ディスプレイIDにハンドルを渡す
                MIL.MdispSelectWindow(m_milDisplay, m_milDisplayImage, m_hDisplayHandle);
                //  オーバーレイバッファを確保
                MIL.MdispInquire(m_milDisplay, MIL.M_OVERLAY_ID, ref m_milOverlay);
                if (m_milOverlay == MIL.M_NULL)
                {
                    return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 画面表示用バッファを取得
        /// </summary>
        /// <returns>表示バッファMIL_ID</returns>
        public MIL_ID GetShowImage(int niConnectCameraID)
        {
            // 接続先のカメラクラスIDを取得
            m_iConnectCameraID = niConnectCameraID;
            // 表示バッファを返す
            return m_milDisplayImage;
        }

        /// <summary>
        /// 接続中のカメラID取得
        /// </summary>
        /// <returns>接続中の場合はカメラID、非接続の場合はnull</returns>
        public int? GetConnectCameraID()
        {
            // 非接続の場合はnull、接続済みなら接続カメラIDを返す
            return m_iConnectCameraID;
        }

        /// <summary>
        /// 画像サイズを取得
        /// </summary>
        /// <returns>画像サイズ</returns>
        public Size GetImageSize()
        {
            return m_szImageSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nmilShowImage">接続カメラの表示用バッファ、未接続ならばM_NULL</param>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int CloseDisplay()
        {
            try
            {
                if (m_milDisplayImage != MIL.M_NULL)
                {
                    // 表示用バッファを解放
                    MIL.MbufFree(m_milDisplayImage);
                    m_milDisplayImage = MIL.M_NULL;
                }
                if (m_milDisplay != MIL.M_NULL)
                {
                    // ディスプレイIDを解放
                    MIL.MdispFree(m_milDisplay);
                    m_milDisplay = MIL.M_NULL;
                }
                // オーバーレイバッファについてはディスプレイIDに紐づくために解放は不要
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 画像ファイルをロードする
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <returns>0:正常終了、-1:画像バッファ取得失敗、-2:オーバレイバッファ取得失敗、-3:画像拡張子無し、-999:異常終了</returns>
        public int LoadImage(string nstrImageFilePath)
        {
            try
            {
                string str_ext;
                bool b_exist_ext = ExtractExtention(nstrImageFilePath, out str_ext);
                if (!b_exist_ext)
                {
                    // 画像拡張子(bmp,jpg,jpeg,png)がついていない
                    return -3;
                }
                //  MIL関数でロード
                if (m_milDisplay != MIL.M_NULL)
                {
                    //  もし既に画像バッファを確保していたら、一度開放してから再度確保する
                    MIL.MbufFree(m_milDisplayImage);
                    //  画像サイズも更新
                    MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDisplayImage);
                    if (m_milDisplayImage == MIL.M_NULL)
                    {
                        return -1;
                    }
                }
                //  ディスプレイIDにハンドルを渡す
                MIL.MdispSelectWindow(m_milDisplay, m_milDisplayImage, m_hDisplayHandle);
                // 画像をインポートする
                MIL.MbufImport(nstrImageFilePath, MIL.M_DEFAULT, MIL.M_LOAD, m_smilSystem, ref m_milDisplayImage);
                // オーバーレイバッファを確保
                MIL.MdispInquire(m_milDisplay, MIL.M_OVERLAY_ID, ref m_milOverlay);
                if (m_milOverlay == MIL.M_NULL)
                {
                    return -2;
                }
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 倍率切替
        /// </summary>
        /// <param name="ndMagRate">倍率値</param>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int SetMagRate(double ndMagRate)
        {
            try
            {
                // 設定最小値よりも小さい値を指定された場合は、設定最小値を使用する
                if (ndMagRate < m_cdMinMagRate)
                {
                    ndMagRate = m_cdMinMagRate;
                }
                // 設定最大値よりも大きい値を指定された場合は、設定最大値を使用する
                if (ndMagRate > m_cdMaxMagRate)
                {
                    ndMagRate = m_cdMaxMagRate;
                }
                //  倍率を切り替える
                MIL.MdispZoom(m_milDisplay, ndMagRate, ndMagRate);
                m_dMagRate = ndMagRate;
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 倍率を取得する
        /// </summary>
        /// <returns>倍率値</returns>
        public double GetMagRate()
        {
            return m_dMagRate;
        }

        /// <summary>
        /// オーバーレイバッファを取得する
        /// </summary>
        /// <returns>オーバーレイバッファID、-1:オーバーレイバッファ取得失敗、-999:異常終了</returns>
        public MIL_ID GetOverlay()
        {
            try
            {
                //  ディスプレイIDにハンドルを渡す
                MIL.MdispSelectWindow(m_milDisplay, m_milDisplayImage, m_hDisplayHandle);
                //  オーバーレイバッファを確保
                MIL.MdispInquire(m_milDisplay, MIL.M_OVERLAY_ID, ref m_milOverlay);
                if (m_milOverlay == MIL.M_NULL)
                {
                    return -1;
                }
                return m_milOverlay;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// グラフィックをクリアする
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int ClearGraphic()
        {
            try
            {
                // グラフィックを透過色でクリアする
                MIL.MdispControl(m_milDisplay, MIL.M_OVERLAY_CLEAR, MIL.M_TRANSPARENT_COLOR);
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 画像を保存
        /// </summary>
        /// <param name="nstrImageFilePath">保存先ファイルパス</param>
        /// <param name="nbIncludeGraphic">保存画像にグラフィックを含めるか否か</param>
        /// <returns>0:正常終了、-1:拡張子エラー、-2:画像バッファ取得失敗、-3:パス内にファイル名無し、-999:異常終了</returns>
        public int SaveImage(string nstrImageFilePath, bool nbIncludeGraphic)
        {
            try
            {
                MIL_ID mil_overlay_temp = MIL.M_NULL;   // オーバーレイバッファを一時的に保存するバッファ
                MIL_ID mil_result_temp = MIL.M_NULL;    // 画像を一時的に保存するバッファ
                string str_ext;                         // 拡張子

                // 一時的保存バッファを確保
                MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref mil_result_temp);
                if (m_milDisplayImage == MIL.M_NULL)
                {
                    return -2;
                }
                // 表示画像を一時的保存バッファにコピー
                MIL.MbufCopy(m_milDisplayImage, mil_result_temp);
                if (nbIncludeGraphic)
                {
                    //	一時的オーバーレイバッファを確保
                    MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref mil_overlay_temp);
                    if (m_milDisplayImage == MIL.M_NULL)
                    {
                        return -2;
                    }
                    //	一時的オーバーレイバッファにオーバーレイバッファをコピー
                    MIL.MbufCopy(m_milOverlay, mil_overlay_temp);
                    //	オーバーレイを一時的保存バッファ上に適応
                    MIL.MbufTransfer(mil_overlay_temp, mil_result_temp, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_COMPOSITION, MIL.M_DEFAULT, m_smilintTransparentColor, MIL.M_NULL);
                    //	メモリ開放
                    MIL.MbufFree(mil_overlay_temp);
                }

                bool b_exist_ext = ExtractExtention(nstrImageFilePath, out str_ext);
                if (!b_exist_ext)
                {
                    if (str_ext == "")
                    {
                        // 画像ファイル用の拡張子ではない
                        MIL.MbufFree(mil_result_temp);
                        return -1;
                    }
                    else
                    {
                        // 拡張子がない為、bmp拡張子を追加する
                        nstrImageFilePath += str_ext;
                        str_ext = str_ext.Replace(".", "");
                    }
                }
                if (System.IO.Path.GetFileNameWithoutExtension(nstrImageFilePath) == "")
                {
                    // 拡張子無しのファイル名を取得し、空ならエラー
                    MIL.MbufFree(mil_result_temp);
                    return -3;
                }
                switch (str_ext)
                {
                    case "jpg":
                        // jpgで保存する
                        MIL.MbufExport(nstrImageFilePath, MIL.M_JPEG_LOSSY, mil_result_temp);
                        break;
                    case "png":
                        // pngで保存する
                        MIL.MbufExport(nstrImageFilePath, MIL.M_PNG, mil_result_temp);
                        break;
                    case "bmp":
                        // bmpで保存する
                        MIL.MbufExport(nstrImageFilePath, MIL.M_BMP, mil_result_temp);
                        break;
                    case "tiff":
                        // tiffで保存する
                        MIL.MbufExport(nstrImageFilePath, MIL.M_TIFF, mil_result_temp);
                        break;
                }
                //	メモリ開放
                MIL.MbufFree(mil_result_temp);

                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                m_sdicLogInstance[m_sstrLogKeyDllError].OutputLog($"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// ファイルパスから拡張子を抽出する
        /// </summary>
        /// <param name="nstrImageFilePath">ファイルパス</param>
        /// <param name="nstrExt">拡張子を返す</param>
        /// <returns>画像拡張子(bmp,jpg,jpeg,png,tiff)の有無</returns>
        private bool ExtractExtention(string nstrImageFilePath, out string nstrExt)
        {
            int i_index_ext;                        // パス内の拡張子の位置
            string str_ext;                         // 拡張子
            //	拡張子の位置を探す
            i_index_ext = nstrImageFilePath.IndexOf(".");
            //	拡張子がない場合は仕方ないのでビットマップの拡張子を返す
            if (i_index_ext < 0)
            {
                nstrExt = ".bmp";
                return false;
            }
            else
            {
                //	ファイル名の最後の文字が「.」だった場合もビットマップにしてしまう
                if (i_index_ext + 1 == nstrImageFilePath.Length)
                {
                    nstrExt = "bmp";
                    return false;
                }
                else
                {
                    // 拡張子を抽出
                    str_ext = nstrImageFilePath.Substring(i_index_ext + 1);
                }
            }
            //	拡張子がjpgの場合
            if (string.Compare(str_ext, "jpg") == 0 || string.Compare(str_ext, "JPG") == 0 || string.Compare(str_ext, "jpeg") == 0 || string.Compare(str_ext, "JPEG") == 0)
            {
                nstrExt = "jpg";
                return true;
            }
            //	拡張子がpngの場合
            else if (string.Compare(str_ext, "png") == 0 || string.Compare(str_ext, "PNG") == 0)
            {
                nstrExt = "png";
                return true;
            }
            //	拡張子がbmpの場合
            else if (string.Compare(str_ext, "bmp") == 0 || string.Compare(str_ext, "BMP") == 0)
            {
                nstrExt = "bmp";
                return true;
            }
            // 拡張子がtiffの場合
            else if (string.Compare(str_ext, "tiff") == 0 || string.Compare(str_ext, "TIFF") == 0)
            {
                nstrExt = "tiff";
                return true;
            }
            // 該当拡張子がない場合
            else
            {
                nstrExt = "";
                return false;
            }
        }

        #endregion
    }
}
