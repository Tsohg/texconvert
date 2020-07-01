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
        /// full\path\here\texconvert.exe full\path\here\mww_textures\ddsDirectory full\path\here\mww_textures\pngOutputDirectory [m]
        /// 
        /// full\path\here\texconvert.exe full\path\here\mww_textures\pngDirectory full\path\here\mww_textures\ddsOutputDirectory rev [m]
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
                //target = ".texture"; //debug
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

            //debug
            //formats.Add(imi.Format);

            BitmapSource bmps = imi.GetWPFBitmap(0, true);
            PngBitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmps));
            //FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".png")), FileMode.Create);

            //String manipulation to remove the .target extension, insert the dds format delimited by '-', and append the new .png extension.
            string noExt = Path.ChangeExtension(file, "");
            noExt = noExt.Remove(noExt.Length - 1);
            string fNewName = Path.GetFileName(noExt) + "-" + imi.Format.ToString();
            string pathNew = Path.Combine(outPath, fNewName);
            string extNew = pathNew += ".png";

            //save file.
            FileStream fs = new FileStream(extNew, FileMode.Create);
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
            //get data about file..."D:\\msys2\\home\\Nathan\\dbg\\00004d824c7204b6f7-DDS_ARGB_8.png"
            string name = Path.GetFileName(file);
            string[] details = name.Split('-');
            if (details.Length > 2) throw new Exception("Additional details detected.");
            ImageFormats.ImageEngineFormatDetails imageDetails;
            switch(details[1].Split('.')[0]) //A bit of a complicated way to only look at the part without file extension...
            {
                case "DDS_ARGB_8":
                    imageDetails = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_ARGB_8);
                    break;
                case "DDS_G16_R16":
                    imageDetails = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_G16_R16);
                    break;
                case "DDS_DXT1":
                    imageDetails = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_DXT1);
                    break;
                case "DDS_DXT5":
                default: //we don't know what to do here...so we just assume DXT5.
                    imageDetails = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_DXT5);
                    break;
            }

            //we have already stripped metadata in split. Begin to return it to dds with the stripped metadata.
            FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(details[0], ".dds")), FileMode.Create);
            ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, name));
            Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
            imi.Save(fs, imageDetails, MipHandling.Default, 0, 0, false);
            fs.Close();
        }
    }
}
