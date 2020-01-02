using System.IO;
using System.Collections.Generic;
using CSharpImageLibrary;
using System.Windows.Media.Imaging;
using System;

/// <summary>
/// Takes a directory as input, converts MWW's dds textures to png format, outputs to the given directory.
/// Specifically hunts for .dds files output from DiLemming's unbundler in the input directory.
/// </summary>
namespace texconvert
{
    class Program
    {
        /// <summary>
        /// Accepts 2 arguments only.
        /// Argument 1: The full path of the directory containing the .dds files.
        /// Argument 2: The full path to the directory in which the .png files will be stored after conversion.
        /// 
        /// Example usage:
        /// full\path\here\texconvert.exe full\path\here\mww_textures\ddsDirectory full\path\here\mww_textures\pngOutputDirectory
        /// </summary>
        /// <param name="args">Accepts 2 full directory paths for input and output respectively.</param>
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.Out.WriteLine("Incorrect Arguments. Usage of tool: texconvert inputDirectory outputDirectory");
                return;
            }

            string inPath = args[0];
            string outPath = args[1];

            try
            {
                if (!Directory.Exists(outPath))
                    Directory.CreateDirectory(outPath);

                List<string> files = new List<string>();
                DirectoryInfo di = new DirectoryInfo(inPath);
                FileInfo[] fi = di.GetFiles();
                foreach (FileInfo f in fi)
                {
                    if (Path.GetExtension(Path.Combine(inPath, f.Name)) != ".dds")
                        continue;
                    files.Add(Path.Combine(inPath, f.Name));
                }
                Program p = new Program();
                int index = 1;
                foreach (string file in files)
                {
                    p.ConvertFile(file, inPath, outPath, ref index, files.Count);
                    index++;
                }
                Console.Out.WriteLine("Done.");
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Converts a single .dds file to .png format.
        /// </summary>
        /// <param name="file">Full path of the file.</param>
        /// <param name="inPath">Full input directory path.</param>
        /// <param name="outPath">Full output directory path.</param>
        /// <param name="index">Integer label. Start at i = 1.</param>
        /// <param name="total">Total number of files to be converted.</param>
        public void ConvertFile(string file, string inPath, string outPath, ref int index, int total)
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
    }
}
