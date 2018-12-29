// This code borrows heavily from DTXMania Xg Ver K (thanks kairera0467!)
// https://osdn.net/projects/dtxmaniaxg-verk/scm/git/dtxmaniaxg-verk-git/blobs/work-s/DTXMania%E3%83%97%E3%83%AD%E3%82%B8%E3%82%A7%E3%82%AF%E3%83%88/%E3%82%B3%E3%83%BC%E3%83%89/%E5%85%A8%E4%BD%93/CPrivateFont.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using LinearGradientBrush = System.Drawing.Drawing2D.LinearGradientBrush;
using Pen = System.Drawing.Pen;

namespace gitadora_textgen
{
    class Program
    {
        enum ImageType
        {
            Title,
            TitleSmall,
            Artist
        };

        enum TextAlignment
        {
            Left,
            Center,
            Right
        }

        static private ImageType _imageType;
        static private string _fontType;
        static private string _fontType2;

        static void Main(string[] args)
        {
            var gradationTopColor = Color.FromArgb(0x13, 0xD5, 0xBB);
            var gradationBottomColor = Color.FromArgb(0xe0, 0xe8, 0x2f);
            var fontColor = Color.White;
            var edgeColor = Color.White;

            if (args.Length < 3)
            {
                Console.WriteLine("usage: {0} [font] [song title] [artist name] [optional: output folder]", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("For [font], type \"!\" for default font: Helvetica Extra Compressed");
                Console.WriteLine("Please make sure the font is installed before running this program.");
                Environment.Exit(1);
            }

            _fontType = args[0];
            if (args[0] == "-")
            {
                _fontType = "Meiryo";
                _fontType2 = "Meiryo";
            }
            else if(args[0] == "!")
            {
                _fontType = "Helvetica Extra Compressed";//This font is most closely resemble Martines N10, which i cannot find online for free
                //_fontType2 = "Helvetica Ultra Compressed";
            }
            var songTitle = args[1];
            var artistName = args[2];
            var outputFoldername = args.Length == 4 ? args[3] : "";

            if (!String.IsNullOrEmpty(outputFoldername))
            {
                Directory.CreateDirectory(outputFoldername);
            }

            {
                // Song title = 20px
                _imageType = ImageType.Title;
                DrawImage(Path.Combine(outputFoldername, "title.png"), songTitle, 240, 30, 19, 
                    false, false, TextAlignment.Left, Color.White, Color.Transparent, Color.Transparent, Color.Transparent, FontStyle.Regular, 1);
                //DrawImage(Path.Combine(outputFoldername, "title.png"), songTitle, 240, 30, 22, false, Color.White);

                // Artist name = 24px
                _imageType = ImageType.Artist;
                DrawImage(Path.Combine(outputFoldername, "artist.png"), artistName, 240, 24, 17,
                    false, false, TextAlignment.Right, Color.White, Color.Transparent, Color.Transparent, Color.Transparent, FontStyle.Regular, 1);
                //DrawImage(Path.Combine(outputFoldername, "artist.png"), artistName, 240, 24, 16, false, Color.White);
            }            

            // Small song title = 13px-ish, bold
            //_imageType = ImageType.TitleSmall;
            //DrawImage(Path.Combine(outputFoldername, "title_small.png"), songTitle, 240, 30, 22, true, Color.White, FontStyle.Regular);
        }

        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, TextAlignment textAlignment, Color fontColor, FontStyle fontStyle)
        {
            var edgeColor = Color.Transparent;
            var gradationTopColor = Color.Transparent;
            var gradationBottomColor = Color.Transparent;
            DrawImage(output_filename, text, imageWidth, imageHeight, fontSize, false, false, textAlignment, fontColor, edgeColor, gradationTopColor, gradationBottomColor, fontStyle);
        }


        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, TextAlignment textAlignment, Color fontColor, Color edgeColor, FontStyle fontStyle)
        {
            var outlineSize = fontSize < 32 ? 4 : 5;

            var gradationTopColor = Color.Black;
            var gradationBottomColor = Color.Black;
            DrawImage(output_filename, text, imageWidth, imageHeight, fontSize, true, false, textAlignment, fontColor, edgeColor, gradationTopColor, gradationBottomColor, fontStyle, outlineSize);
        }

        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, TextAlignment textAlignment, Color fontColor, Color gradationTopColor, Color gradationBottomColor)
        {
            var edgeColor = Color.Black;
            DrawImage(output_filename, text, imageWidth, imageHeight, fontSize, true, true, textAlignment, fontColor, edgeColor, gradationTopColor, gradationBottomColor, FontStyle.Regular, 6);
        }

        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, bool drawOutline, bool drawOutlineGradient, TextAlignment textAlignment, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, FontStyle fontStyle, int outlineSize=1)
        {
            if (String.IsNullOrEmpty(text))
            {
                var emptyImage = new Bitmap(imageWidth, imageHeight);
                emptyImage.Save(output_filename);
                Console.WriteLine("Created {0}", output_filename);
                return;
            }

            var fontName = _fontType;
            var lcid = System.Globalization.CultureInfo.GetCultureInfo("en-us").LCID;
            FontFamily fontFamily = new InstalledFontCollection().Families.FirstOrDefault(ff => ff.GetName(lcid) == fontName);

            /*
            PrivateFontCollection privateFontCollection = new PrivateFontCollection();
            privateFontCollection.AddFontFile("dfgotp3.ttc");
            fontFamily = privateFontCollection.Families[0];
            */

            var stringFormat = StringFormat.GenericTypographic;
            stringFormat.LineAlignment = StringAlignment.Near;
            stringFormat.FormatFlags |= StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;

            int edgePt = (int)(fontSize / outlineSize);
            var font = new Font(new FontFamily(_fontType), fontSize, fontStyle);
            var stringSize = System.Windows.Forms.TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                System.Windows.Forms.TextFormatFlags.NoPrefix |
                System.Windows.Forms.TextFormatFlags.NoClipping |
                System.Windows.Forms.TextFormatFlags.SingleLine
            );
            
            int newImageWidth = stringSize.Width + edgePt * 2;
            int newImageHeight = stringSize.Height + edgePt * 2;

            System.Console.WriteLine("String Image Width: " + stringSize.Width);
            System.Console.WriteLine("String Image Height: " + stringSize.Height);

            newImageWidth = imageWidth > newImageWidth ? imageWidth : newImageWidth;
            newImageHeight = imageHeight > newImageHeight ? imageHeight : newImageHeight;

            Image image = new Bitmap(newImageWidth, newImageHeight);
            Image image2 = new Bitmap(newImageWidth, newImageHeight);
            Image image3 = new Bitmap(newImageWidth, newImageHeight);

            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Graphics g2 = Graphics.FromImage(image2);
            g2.SmoothingMode = SmoothingMode.AntiAlias;
            g2.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Graphics g3 = Graphics.FromImage(image3);
            g3.SmoothingMode = SmoothingMode.AntiAlias;
            g3.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            float emSize = g.DpiY * font.SizeInPoints / 72;
                
            Rectangle rect = new Rectangle(4, 0, newImageWidth + 4, newImageHeight);
                
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddString(text, fontFamily, (int)font.Style, emSize, rect, stringFormat);

            // Choose outline brush
            LinearGradientBrush brush = new LinearGradientBrush(rect, edgeColor, edgeColor, LinearGradientMode.Vertical);
            LinearGradientBrush brush2 = new LinearGradientBrush(rect, edgeColor, edgeColor, LinearGradientMode.Vertical);
            LinearGradientBrush brush3 = new LinearGradientBrush(rect, edgeColor, edgeColor, LinearGradientMode.Vertical);

            var box = new RectangleF(graphicsPath.GetBounds().X,
                graphicsPath.GetBounds().Y,
                newImageWidth,
                graphicsPath.GetBounds().Height - (graphicsPath.GetBounds().Height / 8));

            var box2 = new RectangleF(0,
                0,
                newImageWidth,
                graphicsPath.GetBounds().Y);

            var box3 = new RectangleF(0,
                graphicsPath.GetBounds().Y + graphicsPath.GetBounds().Height - (graphicsPath.GetBounds().Height / 8),
                newImageWidth,
                newImageHeight);

            var box4 = new RectangleF(graphicsPath.GetBounds().X - edgePt,
                graphicsPath.GetBounds().Y - (graphicsPath.GetBounds().Height / 8),
                graphicsPath.GetBounds().Width + 6 + edgePt,
                graphicsPath.GetBounds().Height + (graphicsPath.GetBounds().Height / 8) * 2);

            if (drawOutlineGradient)
            {
                brush = new LinearGradientBrush(box, gradationTopColor, gradationBottomColor, LinearGradientMode.Vertical);
                brush2 = new LinearGradientBrush(box2, gradationTopColor, gradationTopColor, LinearGradientMode.Vertical);
                brush3 = new LinearGradientBrush(box3, gradationBottomColor, gradationBottomColor, LinearGradientMode.Vertical);
            }

            // Draw outline
            Pen pen = new Pen(brush, edgePt) { LineJoin = LineJoin.Round };
            g.DrawPath(pen, graphicsPath);

            // This is just to make things look better (in my opinion)
            // Not really required
            Pen pen2 = new Pen(brush2, edgePt) { LineJoin = LineJoin.Round };
            g2.DrawPath(pen2, graphicsPath);

            Pen pen3 = new Pen(brush3, edgePt) { LineJoin = LineJoin.Round };
            g3.DrawPath(pen3, graphicsPath);

            g.DrawImage(image2, box2, box2, GraphicsUnit.Pixel);
            g.DrawImage(image3, box3, box3, GraphicsUnit.Pixel);

            // Draw inner color
            g.FillPath(new SolidBrush(fontColor), graphicsPath);
            
            newImageHeight = (int)box4.Height;
            double heightDiff = 1;
            if (newImageHeight > imageHeight)
            {
                heightDiff = (double)imageHeight / newImageHeight;
                newImageHeight = imageHeight;
            }

            newImageWidth = (int)box4.Width;
            if (newImageWidth > imageWidth)
            {
                newImageWidth = imageWidth;
            }

            newImageWidth = (int)(newImageWidth * heightDiff);

            var result = new Bitmap(imageWidth, imageHeight, image.PixelFormat);
            using (var g4 = Graphics.FromImage(result))
            {
                box4.X += Math.Abs(box4.X - box.X) / 2;
                var yoff = 8;

                foreach (var c in text)
                {
                    var descenderChars = new List<char>(){'g', 'j', 'p', 'q', 'y'};
                    if (descenderChars.Contains(c))
                    {
                        yoff = 8;
                        break;
                    }
                    else if(c > 0x7f)
                    {
                        yoff = 6;
                    }
                }

                yoff += (int)((result.Height - box4.Height) / 4) - 2;
                
                if (_imageType == ImageType.Artist)
                {
                    yoff /= 2;
                    yoff -= 2;
                }
                else if(_imageType == ImageType.Title)
                {
                    yoff /= 2;
                }

                float xoff = -8.0f;

                if(textAlignment == TextAlignment.Center)
                {
                    xoff = (result.Width - box4.Width) / 2 + 3;
                }
                else if(textAlignment == TextAlignment.Right)
                {
                    xoff = (result.Width - box4.Width) + 10;
                }

                g4.DrawImage(image,
                        new RectangleF(xoff, yoff, newImageWidth, newImageHeight - Math.Abs(box4.Y - box.Y) / 2), box4,
                        GraphicsUnit.Pixel);

                //if (centerText && box4.Width < imageWidth)
                //{
                //    g4.DrawImage(image,
                //        new RectangleF((result.Width - box4.Width) / 2 + 3, yoff, newImageWidth, newImageHeight - Math.Abs(box4.Y - box.Y) / 2), box4,
                //        GraphicsUnit.Pixel);
                //}
                //else
                
            }

            result.Save(output_filename);
            Console.WriteLine("Created {0}", output_filename);
        }
    }
}
