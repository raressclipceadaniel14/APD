using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        string imagePath = "D:\\Facultate\\APD\\Proiect Curs\\ProiectAPD\\ProiectAPD\\input.jpg";

        Bitmap originalImage = new Bitmap(imagePath);

        Stopwatch sequentialStopwatch = Stopwatch.StartNew();
        string outputFilePath = "D:\\Facultate\\APD\\Proiect Curs\\ProiectAPD\\ProiectAPD\\output.jpg";
        //ProcessImageSequentially(originalImage, outputFilePath);
        ProcessImageInParallelAsync(originalImage, outputFilePath);
        sequentialStopwatch.Stop();
        Console.WriteLine("Sequential processing time: " + sequentialStopwatch.ElapsedMilliseconds + " ms");
    }

    static void ProcessImageSequentially(Bitmap original, string outputFilePath)
    {
        // Create a new bitmap for the result
        Bitmap result = new Bitmap(200, 200);

        // Loop through each pixel of the original image
        for (int x = 0; x < original.Width; x++)
        {
            for (int y = 0; y < original.Height; y++)
            {
                // Get the color of the pixel in the original image
                Color pixelColor = original.GetPixel(x, y);

                // Filter to black and white
                int avgColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                Color bwColor = Color.FromArgb(avgColor, avgColor, avgColor);

                // Resize the image to 200x200 pixels
                int newX = (int)((double)x / original.Width * 200);
                int newY = (int)((double)y / original.Height * 200);

                // Convert all blue pixels to red
                if (pixelColor.B > 0)
                    pixelColor = Color.FromArgb(pixelColor.R, pixelColor.G, 255); // Red color for blue pixels

                // Set the color of the corresponding pixel in the result image
                result.SetPixel(newX, newY, pixelColor);
            }
        }

        // Save the processed image
        result.Save(outputFilePath);
    }

    static async Task ProcessImageInParallelAsync(Bitmap original, string outputFilePath)
    {
        // Create a new bitmap for the result
        Bitmap result = new Bitmap(200, 200);

        // Convert to black and white asynchronously
        Bitmap bwImage = await ConvertToBlackAndWhiteAsync(original);

        // Resize asynchronously
        Bitmap resizedImage = await ResizeAsync(bwImage);

        // Convert blue to red asynchronously
        await ConvertBlueToRedAsync(resizedImage);

        // Save the processed image
        resizedImage.Save(outputFilePath);
        Console.WriteLine("Processed image saved to: " + outputFilePath);
    }

    static Task<Bitmap> ConvertToBlackAndWhiteAsync(Bitmap original)
    {
        return Task.Run(() =>
        {
            Bitmap bwImage = new Bitmap(original.Width, original.Height);
            object lockObj = new object(); // Lock object

            Parallel.For(0, original.Width, x =>
            {
                Parallel.For(0, original.Height, y =>
                {
                    Color pixelColor;
                    lock (lockObj)
                    {
                        pixelColor = original.GetPixel(x, y);
                    }

                    int avgColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color bwColor = Color.FromArgb(avgColor, avgColor, avgColor);

                    lock (lockObj)
                    {
                        bwImage.SetPixel(x, y, bwColor);
                    }
                });
            });

            return bwImage;
        });
    }

    static Task<Bitmap> ResizeAsync(Bitmap original)
    {
        return Task.Run(() =>
        {
            Bitmap resizedImage = new Bitmap(200, 200);
            object lockObj = new object(); // Lock object

            Parallel.For(0, resizedImage.Width, x =>
            {
                Parallel.For(0, resizedImage.Height, y =>
                {
                    int origX, origY;
                    Color pixelColor;
                    lock (lockObj)
                    {
                        origX = (int)((double)x / resizedImage.Width * original.Width);
                        origY = (int)((double)y / resizedImage.Height * original.Height);
                        pixelColor = original.GetPixel(origX, origY);
                    }
                    lock (lockObj)
                    {
                        resizedImage.SetPixel(x, y, pixelColor);
                    }
                });
            });

            return resizedImage;
        });
    }

    static Task ConvertBlueToRedAsync(Bitmap image)
    {
        return Task.Run(() =>
        {
            object lockObj = new object(); // Lock object

            Parallel.For(0, image.Width, x =>
            {
                Parallel.For(0, image.Height, y =>
                {
                    Color pixelColor;
                    lock (lockObj)
                    {
                        pixelColor = image.GetPixel(x, y);
                    }
                    if (pixelColor.B > 0)
                    {
                        lock (lockObj)
                        {
                            image.SetPixel(x, y, Color.FromArgb(pixelColor.R, pixelColor.G, 255));
                        }
                    }
                });
            });
        });
    }

}
