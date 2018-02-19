using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VersOne.Epub;
using VersOne.Epub.Schema;
using HtmlAgilityPack;
using System.Threading;
using System.Xaml;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public EpubBook epubBook;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Epub Files (*.epub)|*.epub"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                // Opens a book and reads all of its content into memory
                epubBook = EpubReader.ReadBook(openFileDialog.FileName);

                // COMMON PROPERTIES

                // Book's title
                string title = epubBook.Title;

                // Book's authors (comma separated list)
                string author = epubBook.Author;

                // Book's authors (list of authors names)
                List<string> authors = epubBook.AuthorList;

                // Book's cover image (null if there is no cover)
                byte[] coverImageContent = epubBook.CoverImage;
                if (coverImageContent != null)
                {
                    using (MemoryStream coverImageStream = new MemoryStream(coverImageContent.ToArray()))
                    {
                        // Assign the Source property of your image
                        try
                        {
                            File.Delete(openFileDialog.FileName + ".jpg");
                            File.WriteAllBytes(openFileDialog.FileName + ".jpg", coverImageStream.ToArray());
                        }
                        catch
                        {
                            File.WriteAllBytes(openFileDialog.FileName + ".jpg", coverImageStream.ToArray());
                        }
                        BitmapImage imageSource = new BitmapImage(new Uri(@"C:/Users/shish/Downloads/1.jpg", UriKind.Absolute));
                        Image1.Source = imageSource;
                    }
                }
                Info.Text = "Title: " + title + "\n" + "Author: " + author + "\n";
                Info.FontWeight = FontWeights.Bold;

                // CHAPTERS

                // Enumerating chapters
                foreach (EpubChapter chapter in epubBook.Chapters)
                {
                    // Title of chapter
                    string chapterTitle = chapter.Title;

                    // HTML content of current chapter
                    string chapterHtmlContent = chapter.HtmlContent;

                    // Nested chapters
                    List<EpubChapter> subChapters = chapter.SubChapters;

                    //PrintChapter(chapter);

                    //Chapters.Inlines.Add(new Run(chapterTitle + "\n"));

                    Chapters.Items.Add(chapterTitle);
                }
                // CONTENT

                // Book's content (HTML files, stlylesheets, images, fonts, etc.)
                EpubContent bookContent = epubBook.Content;


                // IMAGES

                // All images in the book (file name is the key)
                Dictionary<string, EpubByteContentFile> images = bookContent.Images;

                EpubByteContentFile firstImage = images.Values.First();

                // Content type (e.g. EpubContentType.IMAGE_JPEG, EpubContentType.IMAGE_PNG)
                EpubContentType contentType = firstImage.ContentType;

                // MIME type (e.g. "image/jpeg", "image/png")
                string mimeContentType = firstImage.ContentMimeType;

                // Creating Image class instance from the content
                using (MemoryStream imageStream = new MemoryStream(firstImage.Content))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(imageStream);
                }


                // HTML & CSS

                // All XHTML files in the book (file name is the key)
                Dictionary<string, EpubTextContentFile> htmlFiles = bookContent.Html;

                // All CSS files in the book (file name is the key)
                Dictionary<string, EpubTextContentFile> cssFiles = bookContent.Css;

                // Entire HTML content of the book
                foreach (EpubTextContentFile htmlFile in htmlFiles.Values)
                {
                    string htmlContent = htmlFile.Content;
                }

                // All CSS content in the book
                foreach (EpubTextContentFile cssFile in cssFiles.Values)
                {
                    string cssContent = cssFile.Content;
                }


                // OTHER CONTENT

                // All fonts in the book (file name is the key)
                Dictionary<string, EpubByteContentFile> fonts = bookContent.Fonts;

                // All files in the book (including HTML, CSS, images, fonts, and other types of files)
                Dictionary<string, EpubContentFile> allFiles = bookContent.AllFiles;


                // ACCESSING RAW SCHEMA INFORMATION

                // EPUB OPF data
                EpubPackage package = epubBook.Schema.Package;

                // Enumerating book's contributors
                foreach (EpubMetadataContributor contributor in package.Metadata.Contributors)
                {
                    string contributorName = contributor.Contributor;
                    string contributorRole = contributor.Role;
                }

                // EPUB NCX data
                EpubNavigation navigation = epubBook.Schema.Navigation;

                // Enumerating NCX metadata
                foreach (EpubNavigationHeadMeta meta in navigation.Head)
                {
                    string metadataItemName = meta.Name;
                    string metadataItemContent = meta.Content;
                }
            }

            
        }


        private async void PrintChapter(EpubChapter chapter)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(chapter.HtmlContent);
            StringBuilder sb = new StringBuilder();
            foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//text()"))
            {
                sb.AppendLine(node.InnerText.Trim());
            }
            string chapterTitle = chapter.Title;
            string chapterText = sb.ToString();
            //Console.WriteLine("------------ ", chapterTitle, "------------ ");
            //Console.WriteLine(chapterText);
            //Console.WriteLine();
            
            @ChapterContent.Inlines.Add(chapterText);
            @ChapterContent.FontWeight = FontWeights.Bold;
            
            foreach (EpubChapter subChapter in chapter.SubChapters)
            {
                PrintChapter(subChapter);
            }
        }

        private void Chapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = Chapters.SelectedIndex;
            //MessageBox.Show(index.ToString());
            ChapterContent.Inlines.Clear();
            EpubChapter chapter = epubBook.Chapters[index];
            PrintChapter(chapter);
            TextSize.Text = ChapterContent.FontSize.ToString();
        }

        private void TextSize_plus_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontSize += 2;
            TextSize.Text = ChapterContent.FontSize.ToString();
        }

        private void TextSize_minus_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontSize -= 2;
            TextSize.Text = ChapterContent.FontSize.ToString();
        }

        private void Text_Bold_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => {
                this.Dispatcher.Invoke(new Action(() => {
                    if (Text_Bold.IsChecked == true)
                    {
                        ChapterContent.FontWeight = FontWeights.Bold;
                        Chapters.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        ChapterContent.FontWeight = FontWeights.Regular;
                        Chapters.FontWeight = FontWeights.Regular;
                    }
                }));
            }).Start(); 
        }

        private void EyeCare_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => {
                this.Dispatcher.Invoke(new Action(() => {
                    if (EyeCare.IsChecked == true)
                    {
                        ChapterContent.Background = new SolidColorBrush(Color.FromRgb(249, 241, 228));
                        ChapterContent.Foreground = new SolidColorBrush(Color.FromRgb(4, 0, 0));
                        Chapters.Background = new SolidColorBrush(Color.FromRgb(249, 241, 228));
                        Panel.Background = new SolidColorBrush(Color.FromRgb(249, 241, 228));
                    }
                    else
                    {
                        ChapterContent.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        ChapterContent.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                        Chapters.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        Panel.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    }
                }));
            }).Start();
            
        }

        private void FontFamily_Hei_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontFamily = new FontFamily("SimHei");
        }

        private void FontFamily_Song_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontFamily = new FontFamily("STZHONGSONG");
        }

        private void FontFamily_Kai_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontFamily = new FontFamily("KaiTi");
        }

        private void FontFamily_Yuan_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontFamily = new FontFamily("YouYuan");
        }

        private void FontFamily_Li_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontFamily = new FontFamily("LiSu");
        }

        private void FontFamily_YaHei_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontFamily = new FontFamily("Microsoft YaHei");
        }

        private void FontFamily_Deng_Click(object sender, RoutedEventArgs e)
        {
            ChapterContent.FontFamily = new FontFamily("DengXian");
        }

        private void TextSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ChapterContent.FontSize = Convert.ToInt16(TextSize.Text);
            }
        }

        private void ChapterContent_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Key.Up)
            {
                ScrollChapter.ScrollToVerticalOffset(-2);
            }
            if (e.Key == Key.Down)
            {
                ScrollChapter.ScrollToVerticalOffset(2);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AutoScroll_Click(object sender, RoutedEventArgs e)
        {
            
        }
        
    }
}
