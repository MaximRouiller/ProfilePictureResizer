using Microsoft.Azure.WebJobs;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;

namespace PortraitImageResizer.Serverless.Model
{
    public class PortraitGenerator
    {
        private ExecutionContext context;

        public PortraitGenerator(ExecutionContext context)
        {
            this.context = context;
        }

        public Stream GeneratePortrait(Stream fileStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                byte[] fileContent = memoryStream.ToArray();

                Rect[] faces = DetectFace(fileContent);
                if (faces.Length == 0)
                {
                    Console.WriteLine($"No faces detected.");
                    return null;
                }
                Rect face = faces.Where(face => face.Height == faces.Max(x => x.Height)).First();

                using (Image image = Image.Load(fileContent))
                {
                    double zoomRatio = 2.5;
                    Rectangle cropRectangle = CalculateCropRectangleForFace(face, zoomRatio, image.Width, image.Height);

                    image.Mutate(x => x.Crop(cropRectangle));

                    MemoryStream outputStream = new MemoryStream();                    
                    var encoder = new JpegEncoder()
                    {
                        Quality = 75 
                    };

                    image.Save(outputStream, encoder);
                    outputStream.Position = 0;
                    return outputStream;
                }
            }
        }

        private Rectangle CalculateCropRectangleForFace(Rect face, double zoomRatio, int originalImageWidth, int originalImageHeight)
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

        private Rect[] DetectFace(byte[] fileContent)
        {
            var cascade = new CascadeClassifier(Path.Combine(context.FunctionAppDirectory, "data/haarcascade_frontalface_default.xml"));
            using (var src = Mat.FromImageData(fileContent, ImreadModes.Color))
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
