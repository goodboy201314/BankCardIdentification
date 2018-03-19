using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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

namespace BankCardIdentification
{

   

    public partial class MainFrm : Form
    {
        #region 全局变量
        // 银行卡图片的路径
        String cardPath = "";
        // 银行卡上的数字个数
        int cardNums = 19;
        #endregion

        /// <summary>
        /// 图片位置信息
        /// </summary>

        // 统计0-9出现次数的数组  
        int[] TongJi = new int[10];
        const bool TEST = true;
        public MainFrm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///打开文件目录，选择图片  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImage_Click(object sender, EventArgs e)
        {
            // 创建文件打开窗口
            OpenFileDialog of = new OpenFileDialog();
            // 文件类型过滤
            of.Filter = "*.bmp;*.jpg;*.gif|*.bmp;*.jpg;*.gif;*.jpeg";
            of.ShowDialog();
             // 获取文件的路径
            string filePath = of.FileName;
            if (File.Exists(of.FileName))
            {
                this.cardPath = filePath;
                this.picCar.Image = Image.FromFile(filePath);
            }

            // 界面修改
            btnNumber.Enabled = true;
            pictureBox20.Image = Image.FromFile(@"skin/003.png");
            pictureBox24.Image = Image.FromFile(@"skin/004.png");
        }

        /// <summary>
        /// 抠出银行卡的卡号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNumber_Click(object sender, EventArgs e)
        {

            Bitmap bitmap = new Bitmap(cardPath);
            Image<Bgr, byte> img = new Image<Bgr, byte>(bitmap);
            Image<Gray, byte> gray = new Image<Gray, byte>(img.Width, img.Height);
            Image<Bgr, byte> resuImage = new Image<Bgr, byte>(img.Width, img.Height);
            Image<Gray, byte> dnc = new Image<Gray, byte>(img.Width, img.Height);
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgra2Gray);//灰度化
            CvInvoke.Threshold(gray, gray, 100, 255, ThresholdType.Binary| ThresholdType.ToZero);//二值化
            gray.ToBitmap().Save(@"C:\Users\Administrator\Desktop\1111111111.png");
            // 腐蚀
            var kernal = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(26,5),new Point(-1,-1));
            CvInvoke.Erode(gray, gray, kernal, new Point(0, 2), 1, BorderType.Default, new MCvScalar());
            gray.ToBitmap().Save(@"C:\Users\Administrator\Desktop\2222222222.png");

            //检测连通域，每一个连通域以一系列的点表示
            Bitmap  graybtm = gray.ToBitmap();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(gray, contours, dnc, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
            var color = new MCvScalar(0, 0, 255);

            //开始遍历
            // 存储轮廓
            Rectangle[] rects = new Rectangle[100];
      
            for (int i = 0; i < contours.Size; i++)
            {
                //得到这个连通区域的外接矩形
                var rect = CvInvoke.BoundingRectangle(contours[i]);

                //如果高度不足，或者长宽比太小，认为是无效数据，否则把矩形返回
                if (rect.Width > rect.Height *10 && rect.Height > 20)
                {
                    rects[i] = rect;
                    //CvInvoke.DrawContours(resuImage, contours, i, color);
                }
            }

            // 寻找宽度最大的轮廓
            int maxLenPos = 0;  // rects[0].Size.Width;
            for (int i = 1; i < rects.Length; i++)
            {
                if (rects[i].Size.Width > rects[maxLenPos].Size.Width) maxLenPos = i;
            }
            Bitmap newBitmap = SplitImg(bitmap, rects[maxLenPos]);
            this.picNum.Image = newBitmap;
            newBitmap.Save(Environment.CurrentDirectory + "//images//kouTu.jpg");

            // img.ConcateVertical(resuImage).ToBitmap().Save(@"C:\Users\Administrator\Desktop\2\00.png");

            #region 不好
            /*
           int X = 0;
           // int Y = (int)(132 * bitmap.Height / 235);
           int Y = (int)(130 * bitmap.Height / 235);
           int W = bitmap.Width;
           int H = (int)(25 * bitmap.Height / 235) + 1;

           Bitmap newBitmap = SplitImg(bitmap, new Rectangle(X, Y, W, H));
           this.picNum.Image = newBitmap;
           newBitmap.Save(Environment.CurrentDirectory + "//images//kouTu.jpg");
             */
            #endregion



            // 修改界面
            btnImageGray.Enabled = true;
            pictureBox21.Image = Image.FromFile(@"skin/004.png");
            pictureBox25.Image = Image.FromFile(@"skin/004.png");
        }

       

        /// <summary>
        ///  抠出的数字图像灰度化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImageGray_Click(object sender, EventArgs e)
        {
            // 获取图片
            Bitmap bitmap = (Bitmap)Image.FromFile(Environment.CurrentDirectory + "//images//kouTu.jpg");
            //定义锁定bitmap的rect的指定范围区域
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            //加锁区域像素
            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

            //位图的首地址
            var ptr = bitmapData.Scan0;

            //stride：扫描行
            int len = bitmapData.Stride * bitmap.Height;

            var bytes = new byte[len];

            //锁定区域的像素值copy到byte数组中
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, len);
            this.progressBar1.Maximum = bitmap.Width * bitmap.Height;
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width * 3; j = j + 3)
                {
                    var color = bytes[i * bitmapData.Stride + j + 2] * 0.299
                          + bytes[i * bitmapData.Stride + j + 1] * 0.597
                          + bytes[i * bitmapData.Stride + j] * 0.114;

                    bytes[i * bitmapData.Stride + j]
                         = bytes[i * bitmapData.Stride + j + 1]
                         = bytes[i * bitmapData.Stride + j + 2] = (byte)color;
                    this.progressBar1.Value++;
                }
            }

            //copy回位图
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, ptr, len);

            //解锁
            bitmap.UnlockBits(bitmapData);
            bitmap.Save(Environment.CurrentDirectory + "//images//gray.jpg");

            this.picGray.Image = bitmap;

            // 修改界面
            btnBitValue.Enabled = true;
            pictureBox22.Image = Image.FromFile(@"skin/004.png");
            pictureBox26.Image = Image.FromFile(@"skin/004.png");

        }



        #region 未优化的图片灰度化
        /***
        /// <summary>
        ///  抠出的数字图像灰度化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImageGray_Click(object sender, EventArgs e)
        {
            // 获取图片
            Bitmap bmp = (Bitmap)Image.FromFile(testPath);
            // 处理图片
            this.progressBar1.Maximum = bmp.Width * bmp.Height;

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色 
                    Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值 
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                    this.progressBar1.Value++;
                }
            }
            this.picGray.Image = bmp;

        }
        ****/
        #endregion

        /// <summary>
        /// 图像二值化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBitValue_Click(object sender, EventArgs e)
        {
            // 文件路径
            Bitmap curBitmap = (Bitmap)Image.FromFile(Environment.CurrentDirectory + "//images//gray.jpg");
            if (curBitmap != null)
            {
                Rectangle rect = new Rectangle(0, 0, curBitmap.Width, curBitmap.Height);
                System.Drawing.Imaging.BitmapData bmpData = curBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, curBitmap.PixelFormat);
                IntPtr ptr = bmpData.Scan0;
                int bytes = curBitmap.Width * curBitmap.Height * 3;
                byte[] grayValues = new byte[bytes];

                System.Runtime.InteropServices.Marshal.Copy(ptr, grayValues, 0, bytes);
                
                byte T = 0;
                byte[] neighb = new byte[bytes];
                byte temp = 0;
                byte maxGray = 0;
                byte minGray = 255;
                int[] countPixel = new int[256];

                // 进度条显示
                this.progressBar1.Minimum = 0;
                this.progressBar1.Maximum = grayValues.Length + bytes;
                this.progressBar1.Value = 0;

                for (int i = 0; i < grayValues.Length; i++)
                {
                    temp = grayValues[i];
                    countPixel[temp]++;
                    if (temp > maxGray)
                    {
                        maxGray = temp;
                    }
                    if (temp < minGray)
                    {
                        minGray = temp;
                    }
                    this.progressBar1.Value++;
                }
                double mu1, mu2;
                int numerator;
                double sigma;
                double tempMax = 0;

                // 大津法二值化图像
                double w1 = 0, w2 = 0;
                double sum = 0;
                numerator = 0;
                for (int i = minGray; i <= maxGray; i++)
                {
                    sum += i * countPixel[i];
                }
                for (int i = minGray; i < maxGray; i++)
                {
                    w1 += countPixel[i];
                    numerator += i * countPixel[i];
                    mu1 = numerator / w1;
                    w2 = grayValues.Length - w1;
                    mu2 = (sum - numerator) / w2;
                    sigma = w1 * w2 * (mu1 - mu2) * (mu1 - mu2);

                    if (sigma > tempMax)
                    {
                        tempMax = sigma;
                        T = Convert.ToByte(i);
                    }
                }

                for (int i = 0; i < bytes; i++)
                {
                    if (grayValues[i] < T)
                    {
                        grayValues[i] = 0;
                    }
                    else
                    {
                        grayValues[i] = 255;
                    }
                    this.progressBar1.Value++;
                }

                System.Runtime.InteropServices.Marshal.Copy(grayValues, 0, ptr, bytes);
                curBitmap.UnlockBits(bmpData);


                curBitmap.Save(Environment.CurrentDirectory + "//images//bValue.jpg");

                this.picBitValue.Image = curBitmap;
                
                // 修改界面
                btnCut.Enabled = true;
                pictureBox23.Image = Image.FromFile(@"skin/004.png");
                pictureBox27.Image = Image.FromFile(@"skin/004.png");
            }// end of if
        }

       
        /// <summary>
        /// 将图片上的文字分割出来
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCut_Click(object sender, EventArgs e)
        {
            // 打开图片
            Bitmap bitmap = new Bitmap(Environment.CurrentDirectory + "//images//bValue.jpg");
           
            // 中间值存储单元
            Bitmap graybtm;
            Rectangle[] rects = new Rectangle[100];
            int count;
            Bitmap bitImg = FindBundingBox(bitmap, out graybtm, rects, out count);
            

            this.pictureBox1.Image = bitImg;
            // 数字轮廓存储单元
            Rectangle[] rectNew = new Rectangle[100];
            bool[] flag = new bool[100]; // 标记是否是被包含的小矩形
            
            int k = 0; // 实际的数字轮廓个数
            for (int i = 0; i < count; i++)
            {
                flag[i] = false;
            }
            // 循环找出数字外面的轮廓
            for (int i = 0; i < count; i++)
            {
                if (flag[i]) continue;
                rectNew[k++] = rects[i];
                for (int j = i; j < count; j++)
                {
                    if (rects[i].Contains(rects[j]))
                    {
                        // rectNew.Add(rects[j]);
                        flag[j] = true;
                    }

                }
            }

            // 对数字的轮廓进行排序，使之对应正确的位置
            int pos;
            for (int i = 0; i < k - 1; i++)
            {
                pos = i;
                for (int j = i; j < k; j++)
                {
                    if (rectNew[j].X <= rectNew[pos].X) pos = j;
                }
                // 交换位置
                if (pos != i)
                {
                    Rectangle t = rectNew[i];
                    rectNew[i] = rectNew[pos];
                    rectNew[pos] = t;
                }
            }

            // 从原图中抠出每一个数字轮廓
            for (int i = 0; i < k; i++)
            {
                Bitmap img = SplitImg(new Bitmap(bitmap), rectNew[i]);
                // 获得Tag值为
                PictureBox p = null;
                // 获得对应的控件
                foreach (Control ctrl in this.panel2.Controls)
                {
                    p = (PictureBox)ctrl;
                    if (Convert.ToInt32(p.Tag) == i) break;
                }
                // 裁剪图片
                if (p != null)
                {
                    p.Image = img;
                    img.Save(Environment.CurrentDirectory + "//images//" + i + ".jpg");

                    // 图片归一化
                    Size size = new Size(16, 16);
                    Image<Bgr, byte> newImg = new Image<Bgr, byte>(img);
                    Image<Bgr, byte> newImg2 = new Image<Bgr, byte>(size);
                    CvInvoke.Resize(newImg, newImg2, size);
                    newImg2.ToBitmap().Save(Environment.CurrentDirectory + "//images//new//" + i + ".jpg");
                    
                }
                // this.progressBar1.Value++;
            }

            // 修改界面
            btnRecognize.Enabled = true;
            pictureBox28.Image = Image.FromFile(@"skin/004.png");
        }
        public static Bitmap FindBundingBox(Bitmap bitmap, out Bitmap graybtm, Rectangle[] rects, out int count)
        {
            Image<Bgr, byte> img = new Image<Bgr, byte>(bitmap);
            Image<Gray, byte> gray = new Image<Gray, byte>(img.Width, img.Height);
            Image<Bgr, byte> resuImage = new Image<Bgr, byte>(img.Width, img.Height);
            Image<Gray, byte> dnc = new Image<Gray, byte>(img.Width, img.Height);
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgra2Gray);//灰度化
            //做一下膨胀，x与y方向都做，但系数不同
            var kernal = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(4, 4), new Point(1, 1));
            CvInvoke.Erode(gray, gray, kernal, new Point(0, 2), 1, BorderType.Default, new MCvScalar());
            //CvInvoke.Canny(gray, gray, 100, 60);
            CvInvoke.Threshold(gray, gray, 100, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);//二值化
            //检测连通域，每一个连通域以一系列的点表示，FindContours方法只能得到第一个域
            graybtm = gray.ToBitmap();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(gray, contours, dnc, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
            var color = new MCvScalar(0, 0, 255);
            // Console.WriteLine(contours.Size);

            //开始遍历
            count = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                //得到这个连通区域的外接矩形
                var rect = CvInvoke.BoundingRectangle(contours[i]);
                //如果高度不足，或者长宽比太小，认为是无效数据，否则把矩形画到原图上
                if (rect.Height > bitmap.Height/12 && rect.Width > 10)
                {

                    // rects[count] = new Rectangle();
                    rects[count] = rect;
                    CvInvoke.DrawContours(resuImage, contours, i, color);
                    count++;
                    // Console.WriteLine(count);
                }
            }

            return img.ConcateVertical(resuImage).ToBitmap();
        }

        // 中间错误代码
        void test222()
        {
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = 19;
            this.progressBar1.Value = 0;


            Image image = Image.FromFile(Environment.CurrentDirectory + "//images//bValue.jpg");
            List<Rectangle> list = new List<Rectangle>();
            list = GetCharRect(image, 1, 600000);
            // 图像切割
            for (int i = 0; i < list.Count; i++)
            {
                Bitmap img = SplitImg(new Bitmap(image), list[i]);
                // 获得Tag值为
                PictureBox p = null;
                // 获得对应的控件
                foreach (Control ctrl in this.panel2.Controls)
                {
                    p = (PictureBox)ctrl;
                    if (Convert.ToInt32(p.Tag) == i) break;
                }
                // 裁剪图片
                if (p != null)
                {
                    img.Save("images//" + i + ".jpg");
                    p.Image = img;
                }
               
            }
        }

        #region 抠图,采用深度遍历的思想(不好，舍弃)
        private Point GetNextPoint(Point ptStart, int nArgb)
        {
            if (m_bmpData.Height > ptStart.Y + 1 && this.GetArgbFormByColor(ptStart.X, ptStart.Y + 1) == nArgb)
                return new Point(ptStart.X, ptStart.Y + 1);
            if (m_bmpData.Width > ptStart.X + 1 && this.GetArgbFormByColor(ptStart.X + 1, ptStart.Y) == nArgb)
                return new Point(ptStart.X + 1, ptStart.Y);
            if (0 <= ptStart.Y - 1 && this.GetArgbFormByColor(ptStart.X, ptStart.Y - 1) == nArgb)
                return new Point(ptStart.X, ptStart.Y - 1);
            if (0 <= ptStart.X - 1 && this.GetArgbFormByColor(ptStart.X - 1, ptStart.Y) == nArgb)
                return new Point(ptStart.X - 1, ptStart.Y);
            if (0 <= ptStart.X - 1 && m_bmpData.Height > ptStart.Y + 1 && this.GetArgbFormByColor(ptStart.X - 1, ptStart.Y + 1) == nArgb)
                return new Point(ptStart.X - 1, ptStart.Y + 1);
            if (m_bmpData.Width > ptStart.X + 1 && m_bmpData.Height > ptStart.Y + 1 && this.GetArgbFormByColor(ptStart.X + 1, ptStart.Y + 1) == nArgb)
                return new Point(ptStart.X + 1, ptStart.Y + 1);
            if (m_bmpData.Width > ptStart.X + 1 && 0 <= ptStart.Y - 1 && this.GetArgbFormByColor(ptStart.X + 1, ptStart.Y - 1) == nArgb)
                return new Point(ptStart.X + 1, ptStart.Y - 1);
            if (0 <= ptStart.X - 1 && 0 <= ptStart.Y - 1 && this.GetArgbFormByColor(ptStart.X - 1, ptStart.Y - 1) == nArgb)
                return new Point(ptStart.X - 1, ptStart.Y - 1);
            return Point.Empty;
        }

        private int GetArgbFormByColor(int x, int y)
        {
            return Color.FromArgb(255,
                 m_byColorInfo[y * m_bmpData.Stride + x * 3],
                 m_byColorInfo[y * m_bmpData.Stride + x * 3 + 1],
                 m_byColorInfo[y * m_bmpData.Stride + x * 3 + 2]).ToArgb();
        }

        private void SetColorFormByColorInfo(int x, int y, Color clr)
        {
            m_byColorInfo[y * m_bmpData.Stride + x * 3] = clr.B;
            m_byColorInfo[y * m_bmpData.Stride + x * 3 + 1] = clr.G;
            m_byColorInfo[y * m_bmpData.Stride + x * 3 + 2] = clr.R;
        }
        private Rectangle GetRectFromPoint(int nPixelMin, int nPixelMax, Point ptStart, Color clrSrc, Color clrSet)
        {
            int nCount = 0;
            int nArgb = clrSrc.ToArgb();
            Rectangle rect = new Rectangle(ptStart.X, ptStart.Y, 0, 0);
            List<Point> ptRegList = new List<Point>();
            List<Point> lstTemp = new List<Point>();
            ptRegList.Add(ptStart);
            lstTemp.Add(ptStart);
            this.SetColorFormByColorInfo(ptStart.X, ptStart.Y, clrSet);
            while (ptRegList.Count != 0)
            {
                Point ptTemp = this.GetNextPoint(ptRegList[ptRegList.Count - 1], nArgb);
                //Point ptTemp = ptRegList[ptRegList.Count - 1];
                if (ptTemp != Point.Empty)
                {
                    ptRegList.Add(ptTemp);
                    lstTemp.Add(ptTemp);
                    this.SetColorFormByColorInfo(ptTemp.X, ptTemp.Y, clrSet);
                    nCount++;
                    if (ptTemp.X < rect.Left) { rect.Width = rect.Right - ptTemp.X; rect.X = ptTemp.X; }
                    if (ptTemp.Y < rect.Top) { rect.Height = rect.Bottom - ptTemp.Y; rect.Y = ptTemp.Y; }
                    if (ptTemp.X > rect.Right) rect.Width = ptTemp.X - rect.Left;
                    if (ptTemp.Y > rect.Bottom) rect.Height = ptTemp.Y - rect.Top;
                }
                else
                    ptRegList.RemoveAt(ptRegList.Count - 1);
            }
            rect.Width += 1; rect.Height += 1;
            if (nCount < nPixelMin || nCount > nPixelMax)
            {
                foreach (var v in lstTemp)
                {
                    this.SetColorFormByColorInfo(v.X, v.Y, Color.White);
                }
                return Rectangle.Empty;
            }
            return rect;
        }


        private BitmapData m_bmpData;
        private byte[] m_byColorInfo;
        /// <summary>
        /// 获取验证码字符所在的区域
        /// </summary>
        /// <param name="imgDark">二值化后的图像</param>
        /// <param name="nPixelMin">连续的最小像素个数 小于此数将被忽略</param>
        /// <param name="nPixelMax">连续的最大像素个数 大于此数将被忽略</param>
        /// <returns></returns>
        public List<Rectangle> GetCharRect(Image imgDark, int nPixelMin, int nPixelMax)
        {
            Bitmap bmp = (Bitmap)imgDark;
            m_bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            m_byColorInfo = new byte[bmp.Height * m_bmpData.Stride];
            Marshal.Copy(m_bmpData.Scan0, m_byColorInfo, 0, m_byColorInfo.Length);
            List<Rectangle> lstRect = new List<Rectangle>();
            for (int y = 0, leny = bmp.Height; y < leny; y++)
            {
                for (int x = 0, lenx = bmp.Width; x < lenx; x++)
                {
                    if (this.GetArgbFormByColor(x, y) == Color.White.ToArgb())
                    {//【【【注意 你需要的是白色】】】
                        Rectangle rectTemp = this.GetRectFromPoint(nPixelMin, nPixelMax, new Point(x, y), Color.White, Color.Magenta);
                        if (rectTemp != Rectangle.Empty) lstRect.Add(rectTemp);
                    }
                }
            }
            Marshal.Copy(m_byColorInfo, 0, m_bmpData.Scan0, m_byColorInfo.Length);
            bmp.UnlockBits(m_bmpData);
            //将区域按照left属性排序
            for (int i = 0; i < lstRect.Count; i++)
            {
                for (int j = 1; j < lstRect.Count - i; j++)
                {
                    if (lstRect[j - 1].Left > lstRect[j].Left)
                    {
                        Rectangle rectTemp = lstRect[j];
                        lstRect[j] = lstRect[j - 1];
                        lstRect[j - 1] = rectTemp;
                    }
                }
            }
            return lstRect;
        }

        private static Bitmap SplitImg(Bitmap bitmap, Rectangle rect)
        {
            return bitmap.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);
        }

        #endregion

        private void btnRecognize_Click(object sender, EventArgs e)
        {
            string path = ""; // 数字轮廓路径
            string text = "银行卡卡号为："; // 存储最终的银行卡内容
            this.progressBar1.Maximum = cardNums;
            this.progressBar1.Minimum = 0;
            this.progressBar1.Value = 0;

            if(TEST)
            {
                for (int i = 0; i < TongJi.Length; i++)
                {
                    TongJi[i] = 0;
                }

            }
 
            // 模式匹配
            for (int i = 0; i < cardNums; i++)
            {
                // 获取路径
                path = Environment.CurrentDirectory + "//images//new//" + i + ".jpg";
                // 打开图片  
                Image<Gray, Byte> imageThreshold = new Image<Gray, byte>(path);
                Tesseract _ocr = new Tesseract(Environment.CurrentDirectory + @"\tessdata", "custom", OcrEngineMode.TesseractOnly);// chi_sim
                _ocr.Recognize(imageThreshold);
                string number = _ocr.GetText();
                text += number;

                if (TEST)
                {
                    try
                    {
                        int a = Convert.ToInt32(number.Trim());
                        TongJi[a]++;
                    }
                    catch (Exception)
                    {    }
                }
                
                this.progressBar1.Value++;
            }
            text = text.Replace("\r","").Replace("\n", "");
            this.lblText.Text = text;

            if(TEST)
            {
                RestFrm frm = new RestFrm(TongJi);
                frm.Show();
            }


        }
    }
}
