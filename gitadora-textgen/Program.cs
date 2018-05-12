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

        static private ImageType _imageType;

        static void Main(string[] args)
        {
            var gradationTopColor = Color.FromArgb(0x13, 0xD5, 0xBB);
            var gradationBottomColor = Color.FromArgb(0xe0, 0xe8, 0x2f);
            var fontColor = Color.Black;
            var edgeColor = Color.White;

            if (args.Length < 3 || args[0].ToLower() != "solid" && args[0].ToLower() != "gradient")
            {
                Console.WriteLine("usage: {0} [solid/gradient] [song title] [artist name] [optional: output folder]", AppDomain.CurrentDomain.FriendlyName);
                Environment.Exit(1);
            }

            var style = args[0].ToLower();
            var songTitle = args[1];
            var artistName = args[2];
            var outputFoldername = args.Length == 4 ? args[3] : "";

            if (!String.IsNullOrEmpty(outputFoldername))
            {
                Directory.CreateDirectory(outputFoldername);
            }

            if (style == "solid")
            {
                // Song title = 40px
                _imageType = ImageType.Title;
                DrawImage(Path.Combine(outputFoldername, "title.png"), songTitle, 628, 64, 40, false, fontColor, edgeColor);

                // Artist name = 28px
                _imageType = ImageType.Artist;
                DrawImage(Path.Combine(outputFoldername, "artist.png"), artistName, 628, 48, 28, false, fontColor, edgeColor);
            }
            else if (style == "gradient")
            {
                // Song title = 40px
                _imageType = ImageType.Title;
                DrawImage(Path.Combine(outputFoldername, "title.png"), songTitle, 628, 64, 40, false, fontColor, gradationTopColor, gradationBottomColor);

                // Artist name = 28px
                _imageType = ImageType.Artist;
                DrawImage(Path.Combine(outputFoldername, "artist.png"), artistName, 628, 48, 28, false, fontColor, gradationTopColor, gradationBottomColor);
            }

            // Small song title = 13px-ish, bold
            _imageType = ImageType.TitleSmall;
            DrawImage(Path.Combine(outputFoldername, "title_small.png"), songTitle, 152, 20, 13, true, Color.White);
        }

        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, bool centerText, Color fontColor)
        {
            var edgeColor = Color.Transparent;
            var gradationTopColor = Color.Transparent;
            var gradationBottomColor = Color.Transparent;
            DrawImage(output_filename, text, imageWidth, imageHeight, fontSize, false, false, centerText, fontColor, edgeColor, gradationTopColor, gradationBottomColor);
        }


        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, bool centerText, Color fontColor, Color edgeColor)
        {
            var outlineSize = fontSize < 32 ? 4 : 5;

            var gradationTopColor = Color.Black;
            var gradationBottomColor = Color.Black;
            DrawImage(output_filename, text, imageWidth, imageHeight, fontSize, true, false, centerText, fontColor, edgeColor, gradationTopColor, gradationBottomColor, outlineSize);
        }

        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, bool centerText, Color fontColor, Color gradationTopColor, Color gradationBottomColor)
        {
            var edgeColor = Color.Black;
            DrawImage(output_filename, text, imageWidth, imageHeight, fontSize, true, true, centerText, fontColor, edgeColor, gradationTopColor, gradationBottomColor, 6);
        }

        static void DrawImage(string output_filename, string text, int imageWidth, int imageHeight, float fontSize, bool drawOutline, bool drawOutlineGradient, bool centerText, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int outlineSize=1)
        {
            if (String.IsNullOrEmpty(text))
            {
                var emptyImage = new Bitmap(imageWidth, imageHeight);
                emptyImage.Save(output_filename);
                Console.WriteLine("Created {0}", output_filename);
                return;
            }

            var fontName = "Meiryo";
            var lcid = System.Globalization.CultureInfo.GetCultureInfo("en-us").LCID;
            FontFamily fontFamily = new InstalledFontCollection().Families.FirstOrDefault(ff => ff.GetName(lcid) == fontName);

            /*
            PrivateFontCollection privateFontCollection = new PrivateFontCollection();
            privateFontCollection.AddFontFile("dfgotp3.ttc");
            fontFamily = privateFontCollection.Families[0];
            */

            var stringFormat = StringFormat.GenericTypographic;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.FormatFlags |= StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;

            int edgePt = (int)(fontSize / outlineSize);
            var font = new Font(new FontFamily(fontName), fontSize, _imageType == ImageType.TitleSmall ? FontStyle.Bold : FontStyle.Regular);
            var stringSize = System.Windows.Forms.TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                System.Windows.Forms.TextFormatFlags.NoPrefix |
                System.Windows.Forms.TextFormatFlags.NoClipping |
                System.Windows.Forms.TextFormatFlags.SingleLine
            );
            
            int newImageWidth = stringSize.Width + edgePt * 2;
            int newImageHeight = stringSize.Height + edgePt * 2;

            newImageWidth = imageWidth > newImageWidth ? imageWidth : newImageWidth;
            newImageHeight = imageHeight > newImageHeight ? imageHeight : newImageHeight;

            Image image = new Bitmap(newImageWidth, newImageHeight);
            Image image2 = new Bitmap(newImageWidth, newImageHeight);
            Image image3 = new Bitmap(newImageWidth, newImageHeight);

            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            Graphics g2 = Graphics.FromImage(image2);
            g2.SmoothingMode = SmoothingMode.HighQuality;
            g2.TextRenderingHint = TextRenderingHint.AntiAlias;

            Graphics g3 = Graphics.FromImage(image3);
            g3.SmoothingMode = SmoothingMode.HighQuality;
            g3.TextRenderingHint = TextRenderingHint.AntiAlias;

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
                
                if (_imageType == ImageType.TitleSmall)
                {
                    yoff /= 2;
                    yoff -= 1;
                }

                if (centerText && box4.Width < imageWidth)
                {
                    g4.DrawImage(image,
                        new RectangleF((result.Width - box4.Width) / 2 + 3, yoff, newImageWidth, newImageHeight - Math.Abs(box4.Y - box.Y) / 2), box4,
                        GraphicsUnit.Pixel);
                }
                else
                {
                    g4.DrawImage(image,
                        new RectangleF(2, yoff, newImageWidth, newImageHeight - Math.Abs(box4.Y - box.Y) / 2), box4,
                        GraphicsUnit.Pixel);
                }
            }

            result.Save(output_filename);
            Console.WriteLine("Created {0}", output_filename);
        }
    }
}
