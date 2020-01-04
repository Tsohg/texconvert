using System.IO;
using System.Collections.Generic;
using CSharpImageLibrary;
using System.Windows.Media.Imaging;
using System;

/// <summary>
/// Takes a directory as input, converts MWW's dds textures to png format, outputs to the given directory.
/// Specifically hunts for .dds files output from DiLemming's unbundler in the input directory.
/// Able to convert .png files into .dds files. Mostly in DXT5 format. Any pngs that can not be compressed using DXT5 will be in ARGB_8 format.
/// </summary>
namespace texconvert
{
    class Program
    {
        /// <summary>
        /// Accepts up to 3 arguments.
        /// Argument 1: The full path of the directory containing the .dds files.
        /// Argument 2: The full path to the directory in which the .png files will be stored after conversion.
        /// [Argument 3]: rev
        /// rev = reverse. Used to indicate that you want to turn convert .png files to .dds format.
        /// Example usage:
        /// full\path\here\texconvert.exe full\path\here\mww_textures\ddsDirectory full\path\here\mww_textures\pngOutputDirectory
        /// 
        /// full\path\here\texconvert.exe full\path\here\mww_textures\pngDirectory full\path\here\mww_textures\ddsOutputDirectory rev
        /// </summary>
        /// <param name="args">Accepts 2 full directory paths for input and output respectively.</param>
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.Out.WriteLine("Incorrect Arguments. Usage of tool: texconvert inputDirectory outputDirectory");
                return;
            }
            Program p = new Program();
            string inPath = args[0];
            string outPath = args[1];
            string target;
            ConversionDel conversion;

            if (args.Length == 3 && args[2] == "rev") //reverse. going from png to dds
            {
                conversion = p.ConvertPngToDds;
                target = ".png";
            }
            else
            {
                conversion = p.ConvertDdsToPng;
                target = ".dds";
            }

            try
            {
                if (!Directory.Exists(outPath))
                    Directory.CreateDirectory(outPath);

                List<string> files = new List<string>();
                DirectoryInfo di = new DirectoryInfo(inPath);
                FileInfo[] fi = di.GetFiles();
                foreach (FileInfo f in fi)
                {
                    if (Path.GetExtension(Path.Combine(inPath, f.Name)) != target)
                        continue;
                    files.Add(Path.Combine(inPath, f.Name));
                }
                int index = 1;
                foreach (string file in files)
                {
                    conversion(file, inPath, outPath, ref index, files.Count);
                    index++;
                }
                Console.Out.WriteLine("Done.");
                //Console.Read(); //debug purposes
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }

        public delegate void ConversionDel(string file, string inPath, string outPath, ref int index, int total);

        /// <summary>
        /// Converts a single .dds file to .png format.
        /// </summary>
        /// <param name="file">Full path of the file.</param>
        /// <param name="inPath">Full input directory path.</param>
        /// <param name="outPath">Full output directory path.</param>
        /// <param name="index">Integer label. Start at i = 1.</param>
        /// <param name="total">Total number of files to be converted.</param>
        public void ConvertDdsToPng(string file, string inPath, string outPath, ref int index, int total)
        {
            Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
            ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, file));
            BitmapSource bmps = imi.GetWPFBitmap(0, true);
            PngBitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmps));
            FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".png")), FileMode.Create);
            enc.Save(fs);
            fs.Close();
        }

        /// <summary>
        /// Converts a single .png file to .dds format.
        /// </summary>
        /// <param name="file">Full path of the file.</param>
        /// <param name="inPath">Full input directory path.</param>
        /// <param name="outPath">Full output directory path.</param>
        /// <param name="index">Integer label. Start at i = 1.</param>
        /// <param name="total">Total number of files to be converted.</param>
        public void ConvertPngToDds(string file, string inPath, string outPath, ref int index, int total)
        {
            FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".dds")), FileMode.Create);
            ImageFormats.ImageEngineFormatDetails details = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_DXT5);
            ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, file));
            try
            {
                Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
                imi.Save(fs, details, MipHandling.Default, 0, 0, false);
            }
            catch (Exception e)
            {
                try
                {
                    Console.Out.WriteLine("Retrying Conversion: " + Path.GetFileName(file) + " " + index + "\\" + total);
                    details = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_ARGB_8);
                    imi.Save(fs, details, MipHandling.Default, 0, 0, false);
                    Console.Out.WriteLine("Retry successful.");
                }
                catch (Exception e2)
                {
                    Console.Out.WriteLine("Error converting file: " + Path.GetFileName(file) + " -> " + e2.Message);
                    return;
                }
                finally
                {
                    fs.Close();
                }
            }
            finally
            {
                fs.Close();
            }
        }

        //public void ConvertPngToDds(string file, string inPath, string outPath, ref int index, int total)
        //{
        //    FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".dds")), FileMode.Create);
        //    try
        //    {
        //        Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
        //        ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, file));
        //        ImageFormats.ImageEngineFormatDetails details;

        //        //Handling errors for when the width and height are not intervals of 4.
        //        if (imi.Width % 4 == 0 && imi.Height % 4 == 0)
        //            details = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_DXT5); //testing different dxts
        //        else
        //            details = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_CUSTOM);
        //        imi.Save(fs, details, MipHandling.Default, 0, 0, false);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.Out.WriteLine("Error converting file: " + Path.GetFileName(file) + " -> " + e.Message);
        //        return;
        //    }
        //    finally
        //    {
        //        fs.Close();
        //    }
        //}
    }
}
