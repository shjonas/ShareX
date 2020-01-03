﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2019 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Encoder = System.Drawing.Imaging.Encoder;

namespace ShareX.HelpersLib
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (T item in source)
            {
                action(item);
            }
        }

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            using (IEnumerator<TFirst> e1 = first.GetEnumerator())
            using (IEnumerator<TSecond> e2 = second.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    yield return resultSelector(e1.Current, e2.Current);
                }
            }
        }

        public static byte[] GetBytes(this Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                return ms.ToArray();
            }
        }

        public static Stream GetStream(this Image img)
        {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, img.RawFormat);
            return ms;
        }

        public static ImageCodecInfo GetCodecInfo(this ImageFormat format)
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(info => info.FormatID.Equals(format.Guid));
        }

        public static string GetMimeType(this ImageFormat format)
        {
            ImageCodecInfo codec = format.GetCodecInfo();

            if (codec != null) return codec.MimeType;

            return "image/unknown";
        }

        public static bool IsValidIndex<T>(this T[] array, int index)
        {
            return array != null && index >= 0 && index < array.Length;
        }

        public static bool IsValidIndex<T>(this List<T> list, int index)
        {
            return list != null && index >= 0 && index < list.Count;
        }

        public static T ReturnIfValidIndex<T>(this T[] array, int index)
        {
            if (array.IsValidIndex(index)) return array[index];
            return default(T);
        }

        public static T ReturnIfValidIndex<T>(this List<T> list, int index)
        {
            if (list.IsValidIndex(index)) return list[index];
            return default(T);
        }

        public static T Last<T>(this T[] array, int index = 0)
        {
            if (array.Length > index) return array[array.Length - index - 1];
            return default(T);
        }

        public static T Last<T>(this List<T> list, int index = 0)
        {
            if (list.Count > index) return list[list.Count - index - 1];
            return default(T);
        }

        public static double ToDouble(this Version value)
        {
            return (Math.Max(value.Major, 0) * Math.Pow(10, 12)) +
                (Math.Max(value.Minor, 0) * Math.Pow(10, 9)) +
                (Math.Max(value.Build, 0) * Math.Pow(10, 6)) +
                Math.Max(value.Revision, 0);
        }

        public static bool IsValid(this Rectangle rect)
        {
            return rect.Width > 0 && rect.Height > 0;
        }

        public static Point Add(this Point point, int offset)
        {
            return point.Add(offset, offset);
        }

        public static Point Add(this Point point, int offsetX, int offsetY)
        {
            return new Point(point.X + offsetX, point.Y + offsetY);
        }

        public static Point Add(this Point point, Point offset)
        {
            return new Point(point.X + offset.X, point.Y + offset.Y);
        }

        public static Size Offset(this Size size, int offset)
        {
            return size.Offset(offset, offset);
        }

        public static Size Offset(this Size size, int width, int height)
        {
            return new Size(size.Width + width, size.Height + height);
        }

        public static Rectangle Offset(this Rectangle rect, int offset)
        {
            return new Rectangle(rect.X - offset, rect.Y - offset, rect.Width + (offset * 2), rect.Height + (offset * 2));
        }

        public static Rectangle LocationOffset(this Rectangle rect, int x, int y)
        {
            return new Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height);
        }

        public static Rectangle LocationOffset(this Rectangle rect, Point offset)
        {
            return rect.LocationOffset(offset.X, offset.Y);
        }

        public static Rectangle LocationOffset(this Rectangle rect, int offset)
        {
            return rect.LocationOffset(offset, offset);
        }

        public static Rectangle SizeOffset(this Rectangle rect, int width, int height)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width + width, rect.Height + height);
        }

        public static Rectangle SizeOffset(this Rectangle rect, int offset)
        {
            return rect.SizeOffset(offset, offset);
        }

        public static string Join<T>(this T[] array, string separator = " ")
        {
            StringBuilder sb = new StringBuilder();

            if (array != null)
            {
                foreach (T t in array)
                {
                    if (sb.Length > 0 && !string.IsNullOrEmpty(separator)) sb.Append(separator);
                    sb.Append(t);
                }
            }

            return sb.ToString();
        }

        public static void BeginUpdate(this RichTextBox rtb)
        {
            NativeMethods.SendMessage(rtb.Handle, (int)WindowsMessages.SETREDRAW, 0, 0);
        }

        public static void EndUpdate(this RichTextBox rtb)
        {
            NativeMethods.SendMessage(rtb.Handle, (int)WindowsMessages.SETREDRAW, 1, 0);
            rtb.Invalidate();
        }

        public static void AddContextMenu(this RichTextBox rtb)
        {
            if (rtb.ContextMenuStrip == null)
            {
                ContextMenuStrip cms = new ContextMenuStrip()
                {
                    ShowImageMargin = false
                };

                ToolStripMenuItem tsmiUndo = new ToolStripMenuItem(Resources.Extensions_AddContextMenu_Undo);
                tsmiUndo.Click += (sender, e) => rtb.Undo();
                cms.Items.Add(tsmiUndo);

                ToolStripMenuItem tsmiRedo = new ToolStripMenuItem(Resources.Extensions_AddContextMenu_Redo);
                tsmiRedo.Click += (sender, e) => rtb.Redo();
                cms.Items.Add(tsmiRedo);

                cms.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem tsmiCut = new ToolStripMenuItem(Resources.Extensions_AddContextMenu_Cut);
                tsmiCut.Click += (sender, e) => rtb.Cut();
                cms.Items.Add(tsmiCut);

                ToolStripMenuItem tsmiCopy = new ToolStripMenuItem(Resources.Extensions_AddContextMenu_Copy);
                tsmiCopy.Click += (sender, e) => rtb.Copy();
                cms.Items.Add(tsmiCopy);

                ToolStripMenuItem tsmiPaste = new ToolStripMenuItem(Resources.Extensions_AddContextMenu_Paste);
                tsmiPaste.Click += (sender, e) => rtb.Paste();
                cms.Items.Add(tsmiPaste);

                ToolStripMenuItem tsmiDelete = new ToolStripMenuItem(Resources.Extensions_AddContextMenu_Delete);
                tsmiDelete.Click += (sender, e) => rtb.SelectedText = "";
                cms.Items.Add(tsmiDelete);

                cms.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem tsmiSelectAll = new ToolStripMenuItem(Resources.Extensions_AddContextMenu_SelectAll);
                tsmiSelectAll.Click += (sender, e) => rtb.SelectAll();
                cms.Items.Add(tsmiSelectAll);

                cms.Opening += (sender, e) =>
                {
                    tsmiUndo.Enabled = !rtb.ReadOnly && rtb.CanUndo;
                    tsmiRedo.Enabled = !rtb.ReadOnly && rtb.CanRedo;
                    tsmiCut.Enabled = !rtb.ReadOnly && rtb.SelectionLength > 0;
                    tsmiCopy.Enabled = rtb.SelectionLength > 0;
                    tsmiPaste.Enabled = !rtb.ReadOnly && Clipboard.ContainsText();
                    tsmiDelete.Enabled = !rtb.ReadOnly && rtb.SelectionLength > 0;
                    tsmiSelectAll.Enabled = rtb.TextLength > 0 && rtb.SelectionLength < rtb.TextLength;
                };

                rtb.ContextMenuStrip = cms;
            }
        }

        public static void SupportSelectAll(this TextBox tb)
        {
            tb.KeyDown += (sender, e) =>
            {
                if (e.Control && e.KeyCode == Keys.A)
                {
                    tb.SelectAll();
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            };
        }

        public static void SaveJPG(this Image img, Stream stream, int quality)
        {
            quality = quality.Clamp(0, 100);
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            img.Save(stream, ImageFormat.Jpeg.GetCodecInfo(), encoderParameters);
        }

        public static void SaveJPG(this Image img, string filepath, int quality)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                SaveJPG(img, fs, quality);
            }
        }

        public static void SaveGIF(this Image img, Stream stream, GIFQuality quality)
        {
            if (quality == GIFQuality.Default)
            {
                img.Save(stream, ImageFormat.Gif);
            }
            else
            {
                Quantizer quantizer;
                switch (quality)
                {
                    case GIFQuality.Grayscale:
                        quantizer = new GrayscaleQuantizer();
                        break;
                    case GIFQuality.Bit4:
                        quantizer = new OctreeQuantizer(15, 4);
                        break;
                    default:
                    case GIFQuality.Bit8:
                        quantizer = new OctreeQuantizer(255, 4);
                        break;
                }

                using (Bitmap quantized = quantizer.Quantize(img))
                {
                    quantized.Save(stream, ImageFormat.Gif);
                }
            }
        }

        public static long ToUnix(this DateTime dateTime)
        {
            return Helpers.DateTimeToUnix(dateTime);
        }

        public static void AppendTextToSelection(this TextBoxBase tb, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                int start = tb.SelectionStart;
                tb.Text = tb.Text.Insert(start, text);
                tb.SelectionStart = start + text.Length;
            }
        }

        public static void RadioCheck(this ToolStripMenuItem tsmi)
        {
            ToolStripDropDownItem tsddiParent = tsmi.OwnerItem as ToolStripDropDownItem;

            foreach (ToolStripMenuItem tsmiChild in tsddiParent.DropDownItems.OfType<ToolStripMenuItem>())
            {
                tsmiChild.Checked = tsmiChild == tsmi;
            }
        }

        public static void RadioCheck(this ToolStripButton tsb)
        {
            ToolStrip parent = tsb.GetCurrentParent();

            foreach (ToolStripButton tsbParent in parent.Items.OfType<ToolStripButton>())
            {
                if (tsbParent != tsb)
                {
                    tsbParent.Checked = false;
                }
            }

            tsb.Checked = true;
        }

        public static void UpdateCheckedAll(this ToolStripMenuItem tsmi, bool check)
        {
            foreach (ToolStripMenuItem tsmiChild in tsmi.DropDownItems.OfType<ToolStripMenuItem>())
            {
                tsmiChild.Checked = check;
            }
        }

        public static void InvokeSafe(this Control control, Action action)
        {
            if (control != null && !control.IsDisposed)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action);
                }
                else
                {
                    action();
                }
            }
        }

        public static void ForceActivate(this Form form)
        {
            if (!form.IsDisposed)
            {
                if (!form.Visible)
                {
                    form.Show();
                }

                if (form.WindowState == FormWindowState.Minimized)
                {
                    form.WindowState = FormWindowState.Normal;
                }

                form.BringToFront();
                form.Activate();
            }
        }

        public static int WeekOfYear(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        public static Icon ToIcon(this Bitmap bmp)
        {
            IntPtr handle = bmp.GetHicon();
            return Icon.FromHandle(handle);
        }

        public static void DisposeHandle(this Icon icon)
        {
            if (icon.Handle != IntPtr.Zero)
            {
                NativeMethods.DestroyIcon(icon.Handle);
            }
        }

        public static void ApplyDefaultPropertyValues(this object self)
        {
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(self))
            {
                DefaultValueAttribute attr = prop.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
                if (attr != null) prop.SetValue(self, attr.Value);
            }
        }

        public static void MoveUp(this ListViewItem lvi)
        {
            ListView lv = lvi.ListView;

            if (lv.Items.Count > 1)
            {
                int index = lvi.Index;

                if (index == 0)
                {
                    index = lv.Items.Count - 1;
                }
                else
                {
                    index--;
                }

                lv.Items.Remove(lvi);
                lv.Items.Insert(index, lvi);
            }

            lv.Focus();
            lvi.EnsureVisible();
            lvi.Selected = true;
        }

        public static void MoveDown(this ListViewItem lvi)
        {
            ListView lv = lvi.ListView;

            if (lv.Items.Count > 1)
            {
                int index = lvi.Index;

                if (index == lv.Items.Count - 1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }

                lv.Items.Remove(lvi);
                lv.Items.Insert(index, lvi);
            }

            lv.Focus();
            lvi.EnsureVisible();
            lvi.Selected = true;
        }

        public static Bitmap CreateEmptyBitmap(this Image img, int widthOffset = 0, int heightOffset = 0, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
        {
            Bitmap bmp = new Bitmap(img.Width + widthOffset, img.Height + heightOffset, pixelFormat);
            bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);
            return bmp;
        }

        public static Bitmap CreateEmptyBitmap(this Image img, PixelFormat pixelFormat)
        {
            return img.CreateEmptyBitmap(0, 0, pixelFormat);
        }

        public static string GetDescription(this Type type)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : type.Name;
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            return source.Reverse().Take(count).Reverse();
        }

        public static void Check(this ToolStripMenuItem tsmi)
        {
            if (tsmi != null)
            {
                foreach (ToolStripItem item in tsmi.GetCurrentParent().Items)
                {
                    if (item != null && item is ToolStripMenuItem tsmiItem && tsmiItem.Tag.Equals(tsmi.Tag))
                    {
                        tsmiItem.Checked = tsmiItem == tsmi;
                    }
                }
            }
        }

        public static Version Normalize(this Version version)
        {
            return new Version(Math.Max(version.Major, 0), Math.Max(version.Minor, 0), Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
        }

        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            T obj = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, obj);
        }

        public static void SetWatermark(this TextBox textBox, string watermarkText, bool showCueWhenFocus = false)
        {
            if (textBox != null && textBox.IsHandleCreated && watermarkText != null)
            {
                NativeMethods.SendMessage(textBox.Handle, (int)NativeConstants.EM_SETCUEBANNER, showCueWhenFocus ? 1 : 0, watermarkText);
            }
        }

        public static void HideImageMargin(this ToolStripDropDownItem tsddi)
        {
            ((ToolStripDropDownMenu)tsddi.DropDown).ShowImageMargin = false;
        }

        public static void DisableMenuCloseOnClick(this ToolStripDropDownItem tsddi)
        {
            tsddi.DropDown.Closing += (sender, e) => e.Cancel = e.CloseReason == ToolStripDropDownCloseReason.ItemClicked;
        }

        public static void SetValue(this NumericUpDown nud, decimal number)
        {
            nud.Value = number.Clamp(nud.Minimum, nud.Maximum);
        }

        public static void SetValue(this TrackBar tb, int number)
        {
            tb.Value = number.Clamp(tb.Minimum, tb.Maximum);
        }

        public static bool IsValidImage(this PictureBox pb)
        {
            return pb.Image != null && pb.Image != pb.InitialImage && pb.Image != pb.ErrorImage;
        }

        public static void IgnoreSeparatorClick(this ContextMenuStrip cms)
        {
            bool cancelClose = false;

            cms.ItemClicked += (sender, e) =>
            {
                cancelClose = e.ClickedItem is ToolStripSeparator;
            };

            cms.Closing += (sender, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked && cancelClose)
                {
                    e.Cancel = true;
                }
            };
        }

        public static Rectangle Combine(this IEnumerable<Rectangle> rects)
        {
            Rectangle result = Rectangle.Empty;

            foreach (Rectangle rect in rects)
            {
                if (result.IsEmpty)
                {
                    result = rect;
                }
                else
                {
                    result = Rectangle.Union(result, rect);
                }
            }

            return result;
        }

        public static Rectangle AddPoint(this Rectangle rect, Point point)
        {
            return Rectangle.Union(rect, new Rectangle(point, new Size(1, 1)));
        }

        public static Rectangle CreateRectangle(this IEnumerable<Point> points)
        {
            Rectangle result = Rectangle.Empty;

            foreach (Point point in points)
            {
                if (result.IsEmpty)
                {
                    result = new Rectangle(point, new Size(1, 1));
                }
                else
                {
                    result = result.AddPoint(point);
                }
            }

            return result;
        }

        public static Point Center(this Rectangle rect)
        {
            return new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
        }

        public static Point Restrict(this Point point, Rectangle rect)
        {
            point.X = Math.Max(point.X, rect.X);
            point.Y = Math.Max(point.Y, rect.Y);
            point.X = Math.Min(point.X, rect.X + rect.Width - 1);
            point.Y = Math.Min(point.Y, rect.Y + rect.Height - 1);
            return point;
        }

        public static void RefreshItems(this ComboBox cb)
        {
            typeof(ComboBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, cb, new object[] { });
        }

        public static void RefreshItem(this ListBox lb, int index)
        {
            typeof(ListBox).InvokeMember("RefreshItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, lb, new object[] { index });
        }

        public static void RefreshSelectedItem(this ListBox lb)
        {
            if (lb.SelectedIndex > -1)
            {
                lb.RefreshItem(lb.SelectedIndex);
            }
        }

        public static void RefreshItems(this ListBox lb)
        {
            typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, lb, new object[] { });
        }

        public static void ShowError(this Exception e, bool fullError = true)
        {
            string error = fullError ? e.ToString() : e.Message;
            MessageBox.Show(error, "ShareX - " + Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static Task ContinueInCurrentContext(this Task task, Action action)
        {
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return task.ContinueWith(t => action(), scheduler);
        }

        public static void DoubleBuffered(this DataGridView dgv, bool value)
        {
            PropertyInfo pi = dgv.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, value, null);
        }

        public static void AppendLine(this RichTextBox rtb, string value = "")
        {
            rtb.AppendText(value + Environment.NewLine);
        }

        public static void SetFontRegular(this RichTextBox rtb)
        {
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);
        }

        public static void SetFontBold(this RichTextBox rtb)
        {
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
        }

        public static void SupportDarkTheme(this ListView lv)
        {
            if (!lv.OwnerDraw)
            {
                lv.OwnerDraw = true;

                lv.DrawItem += (sender, e) =>
                {
                    e.DrawDefault = true;
                };

                lv.DrawSubItem += (sender, e) =>
                {
                    e.DrawDefault = true;
                };

                lv.DrawColumnHeader += (sender, e) =>
                {
                    if (ShareXResources.UseDarkTheme)
                    {
                        using (Brush brush = new SolidBrush(ShareXResources.Theme.BackgroundColor))
                        {
                            e.Graphics.FillRectangle(brush, e.Bounds);
                        }

                        TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds.LocationOffset(2, 0).SizeOffset(-4, 0), ShareXResources.Theme.TextColor,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                        if (e.Bounds.Right < lv.ClientRectangle.Right)
                        {
                            using (Pen pen = new Pen(ShareXResources.Theme.SeparatorDarkColor))
                            using (Pen pen2 = new Pen(ShareXResources.Theme.SeparatorLightColor))
                            {
                                e.Graphics.DrawLine(pen, e.Bounds.Right - 2, e.Bounds.Top, e.Bounds.Right - 2, e.Bounds.Bottom - 1);
                                e.Graphics.DrawLine(pen2, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom - 1);
                            }
                        }
                    }
                    else
                    {
                        e.DrawDefault = true;
                    }
                };
            }
        }

        public static List<T> Range<T>(this List<T> source, int start, int end)
        {
            List<T> list = new List<T>();

            if (start > end)
            {
                for (int i = start; i >= end; i--)
                {
                    list.Add(source[i]);
                }
            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    list.Add(source[i]);
                }
            }

            return list;
        }

        public static List<T> Range<T>(this List<T> source, T start, T end)
        {
            int startIndex = source.IndexOf(start);
            if (startIndex == -1) return new List<T>();

            int endIndex = source.IndexOf(end);
            if (endIndex == -1) return new List<T>();

            return Range(source, startIndex, endIndex);
        }

        public static Image CloneSafe(this Image img)
        {
            try
            {
                if (img != null)
                {
                    return (Image)img.Clone();
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return null;
        }
    }
}