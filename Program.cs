using System.IO;
using System.Collections.Generic;
using CSharpImageLibrary;
using System.Windows.Media.Imaging;
using System;
using System.Drawing;
using System.Drawing.Imaging;

//Takes a directory as input, converts MWW's dds textures to png format, outputs to the given directory.
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
                if (!Directory.Exists(outPath))
                    Directory.CreateDirectory(outPath);

                List<string> files = new List<string>();
                DirectoryInfo di = new DirectoryInfo(inPath);
                FileInfo[] fi = di.GetFiles();
                foreach (FileInfo f in fi)
                {
                    if (Path.GetExtension(Path.Combine(inPath, f.Name)) != ".texture")
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
        //public void ConvertFile(string file, string inPath, string outPath, ref int index, int total)
        //{
        //    Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
        //    ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, file));
        //    FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".png")), FileMode.Create);
        //    ImageFormats.ImageEngineFormatDetails details = new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.PNG);
        //    imi.Save(fs, details, MipHandling.Default);
        //    fs.Close();
        //}

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

        //public void ConvertFile(string file, string inPath, string outPath, ref int index, int total)
        //{
        //    Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
        //    ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, file));
        //    BitmapSource bmps = imi.GetWPFBitmap();
        //    JpegBitmapEncoder enc = new JpegBitmapEncoder();
        //    enc.Frames.Add(BitmapFrame.Create(bmps));
        //    FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".jpg")), FileMode.Create);
        //    enc.Save(fs);
        //    fs.Close();
        //}

        //public void ConvertFile(string file, string inPath, string outPath, ref int index, int total)
        //{
        //    Console.Out.WriteLine("Converting: " + Path.GetFileName(file) + " " + index + "\\" + total);
        //    ImageEngineImage imi = new ImageEngineImage(Path.Combine(inPath, file));
        //    BitmapSource bmps = imi.GetWPFBitmap();
        //    Bitmap bmp = BitmapSourceToBitmap(bmps);
        //    //bmp.MakeTransparent(); //testing this method of transparency
        //    PngBitmapEncoder enc = new PngBitmapEncoder();
        //    FileStream fs = new FileStream(Path.Combine(outPath, Path.ChangeExtension(Path.GetFileName(file), ".png")), FileMode.Create);
        //    bmp.Save(fs, ImageFormat.Png);
        //    fs.Close();
        //}

        //private Bitmap BitmapSourceToBitmap(BitmapSource bmps)
        //{
        //    BitmapEncoder enc = new BmpBitmapEncoder();
        //    enc.Frames.Add(BitmapFrame.Create(bmps));
        //    MemoryStream stream = new MemoryStream();
        //    enc.Save(stream);
        //    Bitmap res = new Bitmap(stream);
        //    return res;
        //}
    }
}
