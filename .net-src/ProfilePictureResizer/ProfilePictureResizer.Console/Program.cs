using System;
using System.IO;
using System.Linq;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ProfilePictureResizer.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Smart Portait Picture Resizer 9000");

            string[] imagesFilePaths = Directory.GetFiles(@"./images");

            foreach (var imageFilePath in imagesFilePaths)
            {
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(imageFilePath);
                Rect[] faces = DetectFace(imageFilePath);
                if (faces.Length == 0)
                {
                    Console.WriteLine($"No faces detected for {filenameWithoutExtension}");
                    continue;
                }
                var prefixDirectory = "./out/";
                Directory.CreateDirectory(prefixDirectory);

                Rect face = faces.Where(face => face.Height == faces.Max(x => x.Height)).First();

                //Console.WriteLine($"For '{filenameWithoutExtension}': X= {face.X} , Y= {face.Y}, Width={face.Width}, Height={face.Height}");
                using (Image image = Image.Load(imageFilePath))
                {
                    double zoomRatio = 2.5;
                    Rectangle cropRectangle = CalculateCropRectangleForFace(face, zoomRatio, image.Width, image.Height);

                    image.Mutate(x => x.Crop(cropRectangle));
                    image.Save(Path.Combine(prefixDirectory, $"{filenameWithoutExtension}.jpg"));
                }
            }
        }

        private static Rectangle CalculateCropRectangleForFace(Rect face, double zoomRatio, int originalImageWidth, int originalImageHeight)
        {
            int x = face.X - (int)Math.Ceiling(((face.Width * zoomRatio) - face.Width) / 2);
            int y = face.Y - (int)Math.Ceiling(((face.Height * zoomRatio) - face.Height) / 2);
            x = x <= 0 ? 0 : x;
            y = y <= 0 ? 0 : y;

            int width = (int)Math.Ceiling(face.Width * zoomRatio);
            int height = (int)Math.Ceiling(face.Height * zoomRatio);
            width = (width + x) > originalImageWidth ? originalImageWidth - x : width;
            height = (height + y) > originalImageHeight ? originalImageHeight - y : height;

            // fixing non-square pictures
            if (width != height)
            {
                // I do not understand how this works. It just does.
                if (width > height)
                {
                    int offset = (width - height) / 2;
                    x += offset;
                    width = height;
                }
                else
                {
                    int offset = (height - width) / 2;
                    y += offset;
                    height = width;
                }
            }
            return new Rectangle(x, y, width, height);
        }

        private static Rect[] DetectFace(string fileName)
        {
            var cascade = new CascadeClassifier("./data/haarcascade_frontalface_default.xml");
            using (var src = new Mat(fileName, ImreadModes.Color))
            using (var gray = new Mat())
            {
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                // Detect faces
                Rect[] faces = cascade.DetectMultiScale(gray, 1.08, 2, HaarDetectionType.ScaleImage, new OpenCvSharp.Size(30, 30));

                return faces;
            }
        }
    }
}
