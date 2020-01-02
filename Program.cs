using System.IO;
using System.Collections.Generic;
using CSharpImageLibrary;
using System.Windows.Media.Imaging;
using System;

//Takes a directory as input, converts MWW's dds textures to jpeg format, outputs to the given directory.
//Specifically hunts for .texture from DiLemming's unbundler in the input directory.
namespace texconvert
{
    class Program
    {


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
                List<string> files = new List<string>();
                DirectoryInfo di = new DirectoryInfo(inPath);
                FileInfo[] fi = di.GetFiles();
                foreach (FileInfo f in fi)
                    files.Add(Path.Combine(inPath, f.Name));
                Program p = new Program();
                int index = 1;
                foreach (string file in files)
                {
                    p.ConvertFile(file, inPath, outPath, index, files.Count);
                    index++;
                }
                Console.Out.WriteLine("Done.");
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }

        public void ConvertFile(string file, string inPath, string outPath, int index, int total)
        {
            if (Path.GetExtension(file) != ".texture")
                return;
            Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
            ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, file));
            BitmapSource bmps = imi.GetWPFBitmap();
            JpegBitmapEncoder enc = new JpegBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmps));
            FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".jpg")), FileMode.Create);
            enc.Save(fs);
            fs.Close();
        }
    }
}
