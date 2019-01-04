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

namespace ImgBinEg
{
    class Program
    {
       

        static void Main(string[] args)
        {
            string filename,path,dir;
            //take full path from user
            Console.WriteLine("Enter path:");
            //Separating filename and directory
            path = Console.ReadLine();
            dir = Path.GetDirectoryName(path);
            filename =Path.GetFileNameWithoutExtension(path);

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
            result.Save(dir + "\\" + filename + "_2sharp.bmp");
            
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
            result.Save(dir + "\\" + filename + "_3smooth.bmp");

            //Document skew, line detection
            DocumentSkewChecker skew = new DocumentSkewChecker();
            double angle = skew.GetSkewAngle(result);
            Console.WriteLine("Skewing and rotating...");
            RotateBilinear rot = new RotateBilinear(-angle);
            rot.FillColor = Color.White;
            result=rot.Apply(result);

            result.Save(dir + "\\" + filename + "_4rot.bmp");


        }
    }
}
