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
        //public static string dirToSave;
        public static string dirTarget;
        static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();
            
            string dirToSave = args[0];
            dirTarget= args[1];
            logerr = args[2];
                        
            Console.WriteLine("Let's go");
            ParseDirAnCopy(new DirectoryInfo(dirToSave));
            Console.WriteLine("The end.");


            //Console.WriteLine();
            //Console.WriteLine($"New files: count={newFilesCount}, size={newFilesSize}");
            //Console.WriteLine($"Existing files: count={existingFilesCount}, size={existingFilesSize}");
            //Console.WriteLine($"Total: count={newFilesCount + existingFilesCount}, size={newFilesSize + existingFilesSize}");
            //Console.WriteLine($"Errors : {errors} - see '{logerr}'");

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
                        nimages++;
                        return new DateInfos() { Date = dateTime.Value, Mode = 0 };                        
                    }                    
                }
                throw new Exception("perso");
            } catch
            {
                nimageslastwrite++;
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
        public static int nimages = 0;
        public static int nimageslastwrite = 0;
        
        public static void ParseDirAnCopy(DirectoryInfo dir)
        {
            try
            {
                foreach (var f in dir.GetFiles())
                {
                    try
                    {
                        string fullname = f.FullName;
                        if ( imagesFormats.Any(p=> fullname.ToLower().EndsWith(p)))
                        {                   
                            var t = GetDateTakenFromImage2(fullname);

                            string p =     Path.Combine(dirTarget,  "mode_"+ t.Mode,   t.Date.Year + t.Date.Month.ToString("00"));
                            new DirectoryInfo(p).Create();
                            File.Copy(fullname, Path.Combine(p, f.Name), true);

                           
                            if ((nimages + nimageslastwrite) %100==0)
                                Console.WriteLine($"{nimages} + {nimageslastwrite } = {nimages+nimageslastwrite}   images traitées");
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
                    ParseDirAnCopy(d);
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
