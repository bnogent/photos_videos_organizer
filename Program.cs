using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace photos_videos_organizer
{

    class DateInfos
    {
        public DateTime Date { get; set; }
        public int Mode { get; set; }
    }

    


    class Program
    {
        private static int lst_extenssions = 0;
        private static int process_photos = 1;
        private static int process_videos = 2;
        //public static string dirToSave;
        public static string dirTarget;
        static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();
            
            string dirToSave = args[0];
            dirTarget= args[1];
            logerr = args[2];
                        
            Console.WriteLine("Let's go");
            ParseDirAnCopy(new DirectoryInfo(dirToSave), process_videos);
            Console.WriteLine("The end.");
            
            watch.Stop();
            Console.WriteLine($"Elapsed hours={watch.Elapsed.TotalHours}");
        }

        public static int errors = 0;
        public static string logerr;
        private static Regex r = new Regex(":");


        public static DateInfos GetDateTakenFromImage2(string path)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(path);
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDirectory != null)
                {
                    var dateTime = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
                    if (dateTime.HasValue)
                    {
                        n++;
                        return new DateInfos() { Date = dateTime.Value, Mode = 0 };                        
                    }                    
                }
                throw new Exception("perso");
            } catch
            {
                nlastwrite++;
                return new DateInfos() { Date = new FileInfo(path).LastWriteTime, Mode = 1 };                
            }                                                                                  
        }

        //public static DateTime GetDateTakenFromImage(string path)
        //{
        //    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        //    using (Image myImage = Image.FromStream(fs/*, false, false*/))
        //    {
        //        PropertyItem propItem = null;
        //        try
        //        {
        //            propItem = myImage.GetPropertyItem(36867);
        //        }
        //        catch { }
        //        if (propItem != null)
        //        {
        //            string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
        //            return DateTime.Parse(dateTaken);
        //        }
        //        else
        //            return new FileInfo(path).LastWriteTime;
        //    }
        //}

        
        private static string[] imagesFormats = new string[] { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".bmp",".gif",".eps", ".raw", ".cr2", ".nef", ".orf", ".sr2" };
        private static string[] videosFormats = new string[] { ".mts", ".mpg", ".mp2", ".mpeg", ".mpe", ".mpv", ".ogg", ".mp4", ".m4p", ".m4v", ".avi", ".wmv", ".webm", ".mov", ".qt", ".flv", ".swf", };
        public static int n = 0;
        public static int nlastwrite = 0;

        private static HashSet<string> exts = new HashSet<string>();

        public static void ParseDirAnCopy(DirectoryInfo dir, int method)
        {
            try
            {
                foreach (var f in dir.GetFiles())
                {
                    try
                    {

                        if (method == process_photos)
                        {
                            string fullname = f.FullName;
                            if (imagesFormats.Any(p => fullname.ToLower().EndsWith(p)))
                            {
                                var t = GetDateTakenFromImage2(fullname);

                                string p = Path.Combine(dirTarget, "mode_photos_" + t.Mode, t.Date.Year + t.Date.Month.ToString("00"));
                                new DirectoryInfo(p).Create();
                                File.Copy(fullname, Path.Combine(p, f.Name), true);


                                if ((n + nlastwrite) % 100 == 0)
                                    Console.WriteLine($"{n} + {nlastwrite } = {n + nlastwrite}   images traitées");
                            }
                        }

                        else if (method == process_videos)
                        {
                            string fullname = f.FullName;
                            if (videosFormats.Any(p => fullname.ToLower().EndsWith(p)))
                            {
                                var t = GetDateTakenFromImage2(fullname);

                                string p = Path.Combine(dirTarget, "mode_videos_" + t.Mode, t.Date.Year + t.Date.Month.ToString("00"));
                                new DirectoryInfo(p).Create();
                                File.Copy(fullname, Path.Combine(p, f.Name), true);


                                if ((n + nlastwrite) % 100 == 0)
                                    Console.WriteLine($"{n} + {nlastwrite } = {n + nlastwrite}   videos traitées");
                            }
                        }

                        else if (method == lst_extenssions)
                        {
                            if (!exts.Contains(f.Extension))
                            {
                                exts.Add(f.Extension);
                                File.AppendAllLines("exts.txt", new List<string>() { f.Extension });
                            }
                                
                        }

                        
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                        OnError($"Can't read file :{f.FullName}  {e.Message}");
                    }
                }
            }
            catch
            {
                OnError($"Can't list files of :{dir}");
            }

            try
            {
                foreach (var d in dir.GetDirectories())
                    ParseDirAnCopy(d, method);
            }
            catch
            {
                OnError($"Can't list directories of :{dir}");
            }
        }

        private static void OnError(string err)
        {
            LogErrorMessage(err);
            Console.WriteLine(err);
            errors++;
        }

       
        public static void LogErrorMessage(string err)
        {
            string[] enum2 = new string[] { err };
            File.AppendAllLines(logerr, enum2);
        }
    }
}
