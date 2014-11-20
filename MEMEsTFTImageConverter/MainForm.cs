using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEMEsTFTImageConverter
{
    public partial class MainForm : Form
    {
        Bitmap resized;
        
        public MainForm()
        {
            InitializeComponent();
            convertBtn.Enabled = false;
            saveBtn.Enabled = false;
            widthBox.Enabled = false;
            heightBox.Enabled = false;
        }

        private void fileOpenButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            inputFilePathBox.Text = openFileDialog1.FileName;
            widthBox.Enabled = true;
            heightBox.Enabled = true;
            convertBtn.Enabled = true;
        }

        private void convertBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // サイズ変換してpictureBox1に表示
                Bitmap origin = new Bitmap(openFileDialog1.FileName);
                int w = int.Parse(widthBox.Text);
                int h = int.Parse(heightBox.Text);
                if (w <= 0 || w > 0xffff || h <= 0 || h > 0xffff)
                {
                    MessageBox.Show("幅と高さは1～65535の間で入力してください．");
                    return;
                }
                resized = ResizeImage(origin, w, h);

                pictureBox1.Image = resized;
                saveBtn.Enabled = true;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("画像ファイルが見つかりませんでした。");
            }
            catch (FormatException ex)
            {
                MessageBox.Show("幅と高さを正しい数値で入力してください。");
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            SaveTFTImageFile(saveFileDialog1.FileName);
        }

        /// <summary>
        /// TFTに表示できる形式でファイルを指定パスに保存します．
        /// </summary>
        /// <param name="path">保存ファイルパス</param>
        private void SaveTFTImageFile(string path)
        {
            byte[] buf = new byte[resized.Width * resized.Height * 4];

            BitmapData bd = resized.LockBits(
                new Rectangle(Point.Empty, resized.Size),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppRgb
                );
            Marshal.Copy(bd.Scan0, buf, 0, buf.Length);

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    // 画像サイズ情報の書き込み
                    // 幅，高さをそれぞれ16bitビッグエンディアンで出力
                    byte[] meta_buf = {
                                      (byte)((resized.Width >> 8) & 0xff),
                                      (byte)(resized.Width & 0xff),
                                      (byte)((resized.Height >> 8) & 0xff),
                                      (byte)(resized.Height & 0xff),
                                  };
                    fs.Write(meta_buf, 0, meta_buf.Length);

                    // 画素情報の書き込み
                    // 左上基準に右下方向へ順にデータを出力
                    for (int y = 0; y < resized.Height; y++)
                    {
                        for (int x = 0; x < resized.Width; x++)
                        {
                            // BitmapバイトデータからR,G,B値を取得
                            byte r = buf[(y * resized.Width + x) * 4 + 2];
                            byte g = buf[(y * resized.Width + x) * 4 + 1];
                            byte b = buf[(y * resized.Width + x) * 4];

                            // RGB=5:6:5の16bits形式に変換
                            ushort color = ConvertTFTColor(r, g, b);

                            byte[] cell = new byte[2];
                            // ビッグエンディアンで保存
                            cell[0] = (byte)((color >> 8) & 0x00ff);
                            cell[1] = (byte)(color & 0x00ff);

                            // ファイルストリームに書き込み
                            fs.Write(cell, 0, cell.Length);
                        }
                    }
                }
            }
            catch(System.IO.IOException ex)
            {
                MessageBox.Show("ファイルの保存に失敗しました。保存先のアクセス権やファイルがロックされていないか確認してください。");
            }

            resized.UnlockBits(bd);
        }

        /// <summary>
        /// RGB=5:6:5の形式に変換します
        /// </summary>
        /// <param name="R">赤の画素値</param>
        /// <param name="G">緑の画素値</param>
        /// <param name="B">青の画素値</param>
        /// <returns></returns>
        private static ushort ConvertTFTColor(byte R, byte G, byte B)
        {
            ushort col_short = (ushort)(((R & 0xF8) << 8) | ((G & 0xFC) << 3) | ((B & 0xF8) >> 3));
            return col_short;
        }

        /// <summary>
        /// Bitmapのリサイズを行います
        /// </summary>
        /// <param name="src">元のBitmap</param>
        /// <param name="width">リサイズ後の幅</param>
        /// <param name="height">リサイズ後の高さ</param>
        /// <returns></returns>
        private static Bitmap ResizeImage(Bitmap src, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(resized);

            // リサイズ時のアルゴリズム
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.DrawImage(src, 0, 0, width, height);

            return resized;
        }

    }
}
