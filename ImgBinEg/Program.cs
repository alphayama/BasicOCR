using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Imaging.Filters;
using Accord.DataSets;
using System.Drawing;
using System.IO;
using Accord.Imaging;
using Tesseract;
using System.Diagnostics;
using Accord.IO;

namespace ImgBinEg
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Console.Title = "Basic OCR.exe";
            string filename,path,dir,language;
            //take full path from user
            Console.WriteLine("Enter path:");
            //Separating filename and directory
            path = Console.ReadLine();
            dir = Path.GetDirectoryName(path);
            filename =Path.GetFileNameWithoutExtension(path);
            //Enter language for Tesseract engine
            Console.WriteLine("Enter language(hin-Hindi,eng-English,ori-Oriya):");
            language=Console.ReadLine();
            //Create Bitmap object
            Bitmap img2 = Accord.Imaging.Image.FromFile(path);
            
            //Image is first converted to Grayscale as Thresholding doesn't support coloured images as input
            Grayscale gray = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap result = gray.Apply(img2);
            
            // This blackens the whole area in shades:  Threshold thres = new Threshold(100);

            /*
             Applying Bradley Thresholding which brightens up the area in shades hence a uniformly bright
             image is obtained.
            */
            Console.WriteLine("Applying Bradley Local Thresholding...");
            BradleyLocalThresholding thres = new BradleyLocalThresholding();
            thres.WindowSize = 80;
            thres.PixelBrightnessDifferenceLimit=0.1F;
            thres.ApplyInPlace(result);

            //Save the binarized image
            result.Save(dir+"\\"+filename+"_1thres.bmp");

            //Sharpening
            Console.WriteLine("Sharpening...");
            Sharpen sharp = new Sharpen();
            // apply the filter
            sharp.ApplyInPlace(result);
            //result.Save(dir + "\\" + filename + "_2sharp.bmp");
            
            //Bilateral Smoothing
            // create filter
            Console.WriteLine("Applying Bilateral smoothing...");
            BilateralSmoothing smooth = new BilateralSmoothing();
            smooth.KernelSize = 7;
            smooth.SpatialFactor = 10;
            smooth.ColorFactor = 60;
            smooth.ColorPower = 0.5;
            // apply the filter
            smooth.ApplyInPlace(result);

            //Save cleaned image
            //result.Save(dir + "\\" + filename + "_3smooth.bmp");

            //Document skew, line detection
            DocumentSkewChecker skew = new DocumentSkewChecker();
            double angle = skew.GetSkewAngle(result);
            Console.WriteLine("Skewing and rotating...");
            RotateBilinear rot = new RotateBilinear(-angle);
            rot.FillColor = Color.White;
            result=rot.Apply(result);

            result.Save(dir + "\\" + filename + "_4rot.bmp");

            try
            {
                using (var engine = new TesseractEngine(@".\tessdata", language, EngineMode.Default))
                {
                    using (var img = result)
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            //Console.WriteLine("Text(get text): \r\n{0}", text);

                            StreamWriter sw;
                            string Filename = dir + "\\" + filename + "_text.doc";
                            sw = File.CreateText(Filename);
                            Console.WriteLine("Generating file...");
                            string FileData = text;
                            sw.WriteLine(FileData);
                            sw.Close();

                            //We make a list of type Rectangle. It stores data of all the bounding boxes
                            List<Rectangle> boxes = page.GetSegmentedRegions(PageIteratorLevel.Symbol);


                            /*
                            Graphics gives the following error in case of result/img:
                            A Graphics object cannot be created from an image that has an indexed pixel format. ...
                            ...System.Exception: A Graphics object cannot be created from an image that has an indexed pixel format.
                            
                             I am using the following solution for now:
                             */
                            Bitmap rez = new Bitmap(result);


                            using (Graphics g = Graphics.FromImage(rez))
                            {

                                Pen p = new Pen(Brushes.Red, 1.0F);
                                foreach (Rectangle r in boxes)
                                {
                                    //Console.WriteLine(r);
                                    g.DrawRectangle(p, r);
                                }

                                g.DrawImage(rez, 0, 0);

                            }

                            //Saving the image with bounding boxes
                            Console.WriteLine("Generating image with bounding boxes...");
                            rez.Save(dir + "\\" + filename + "_5seg.bmp");


                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.WriteLine("\n\nPress ENTER/RETURN to exit");
                Console.ReadKey(true);
            }
        }
    }
}
