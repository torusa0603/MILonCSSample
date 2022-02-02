using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;

namespace MatroxCS
{
    class CFileIO : CBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nstrFilePath"></param>
        /// <returns>0:正常終了、-1:拡張子エラー</returns>
        public static int Load(MIL_ID nmilLoadImage ,string nstrImageFilePath)
        {

            string str_ext; // 拡張子
            // 拡張子をチェック
            bool b_exist_ext = ExtractExtention(nstrImageFilePath, out str_ext);
            if (!b_exist_ext)
            {
                // 画像拡張子(bmp,jpg,jpeg,png)がついていない
                return -1;
            }

            // 画像をインポートする
            MIL.MbufImport(nstrImageFilePath, MIL.M_DEFAULT, MIL.M_LOAD, m_smilSystem, ref nmilLoadImage);


            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nstrFilePath"></param>
        /// <returns>0:正常終了、-1:拡張子エラー、-2:パス内にファイル名無し</returns>
        public static int Save(MIL_ID nmilSaveImage, string nstrImageFilePath)
        {
            string str_ext; // 拡張子
            // 拡張子をチェック
            bool b_exist_ext = ExtractExtention(nstrImageFilePath, out str_ext);
            if (!b_exist_ext)
            {
                if (str_ext == "")
                {
                    // 画像ファイル用の拡張子ではない
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
                return -2;
            }
            switch (str_ext)
            {
                case "jpg":
                    // jpgで保存する
                    MIL.MbufExport(nstrImageFilePath, MIL.M_JPEG_LOSSY, nmilSaveImage);
                    break;
                case "png":
                    // pngで保存する
                    MIL.MbufExport(nstrImageFilePath, MIL.M_PNG, nmilSaveImage);
                    break;
                case "bmp":
                    // bmpで保存する
                    MIL.MbufExport(nstrImageFilePath, MIL.M_BMP, nmilSaveImage);
                    break;
                case "tiff":
                    // tiffで保存する
                    MIL.MbufExport(nstrImageFilePath, MIL.M_TIFF, nmilSaveImage);
                    break;
            }
            //	メモリ開放
            MIL.MbufFree(nmilSaveImage);

            return 0;
        }

        /// <summary>
        /// ファイルパスから拡張子を抽出する
        /// </summary>
        /// <param name="nstrImageFilePath">ファイルパス</param>
        /// <param name="nstrExt">拡張子を返す</param>
        /// <returns>画像拡張子(bmp,jpg,jpeg,png,tiff)の有無</returns>
        private static bool ExtractExtention(string nstrImageFilePath, out string nstrExt)
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


    }
}
