using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        string imagePath = "D:\\Facultate\\Proiect APD\\ProiectAPD\\ProiectAPD\\input.jpg";

        Bitmap originalImage = new Bitmap(imagePath);

        Stopwatch sequentialStopwatch = Stopwatch.StartNew();
        string outputFilePath = "D:\\Facultate\\Proiect APD\\ProiectAPD\\ProiectAPD\\output.jpg";
        ProcessImageSequentially(originalImage, outputFilePath);
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
}
