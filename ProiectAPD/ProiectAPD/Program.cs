using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        //ProcessImageInParallelAsync(originalImage, outputFilePath).Wait();
        ProcessImageWithPLINQ(originalImage, outputFilePath);
        sequentialStopwatch.Stop();
        Console.WriteLine("Processing time: " + sequentialStopwatch.ElapsedMilliseconds + " ms");
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

    static void ProcessImageWithPLINQ(Bitmap original, string outputFilePath)
    {
        int width = 200;
        int height = 200;

        Bitmap result = new Bitmap(width, height);

        // Convert to black and white using PLINQ
        var bwImage = ConvertToBlackAndWhitePLINQ(original);

        // Resize using PLINQ
        var resizedImage = ResizePLINQ(bwImage, width, height);

        // Convert blue to red using PLINQ
        ConvertBlueToRedPLINQ(resizedImage);

        // Save the processed image
        resizedImage.Save(outputFilePath);
        Console.WriteLine("Processed image saved to: " + outputFilePath);
    }

    static Bitmap ConvertToBlackAndWhitePLINQ(Bitmap original)
    {
        int width = original.Width;
        int height = original.Height;

        // Extract pixel data into a buffer
        Color[,] pixels = new Color[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = original.GetPixel(x, y);
            }
        }

        Color[,] bwPixels = new Color[width, height];

        // Process the pixels using PLINQ
        var bwPixelData = Enumerable.Range(0, width)
            .SelectMany(x => Enumerable.Range(0, height), (x, y) => new { x, y })
            .AsParallel()
            .Select(p =>
            {
                Color pixelColor = pixels[p.x, p.y];
                int avgColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                return new { p.x, p.y, bwColor = Color.FromArgb(avgColor, avgColor, avgColor) };
            })
            .ToList();

        foreach (var p in bwPixelData)
        {
            bwPixels[p.x, p.y] = p.bwColor;
        }

        Bitmap bwImage = new Bitmap(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bwImage.SetPixel(x, y, bwPixels[x, y]);
            }
        }

        return bwImage;
    }

    static Bitmap ResizePLINQ(Bitmap original, int newWidth, int newHeight)
    {
        int width = original.Width;
        int height = original.Height;

        // Extract pixel data into a buffer
        Color[,] pixels = new Color[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = original.GetPixel(x, y);
            }
        }

        Color[,] resizedPixels = new Color[newWidth, newHeight];

        // Process the pixels using PLINQ
        var resizedPixelData = Enumerable.Range(0, newWidth)
            .SelectMany(x => Enumerable.Range(0, newHeight), (x, y) => new { x, y })
            .AsParallel()
            .Select(p =>
            {
                int origX = (int)((double)p.x / newWidth * width);
                int origY = (int)((double)p.y / newHeight * height);
                Color pixelColor = pixels[origX, origY];
                return new { p.x, p.y, pixelColor };
            })
            .ToList();

        foreach (var p in resizedPixelData)
        {
            resizedPixels[p.x, p.y] = p.pixelColor;
        }

        Bitmap resizedImage = new Bitmap(newWidth, newHeight);
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                resizedImage.SetPixel(x, y, resizedPixels[x, y]);
            }
        }

        return resizedImage;
    }

    static void ConvertBlueToRedPLINQ(Bitmap image)
    {
        int width = image.Width;
        int height = image.Height;

        // Extract pixel data into a buffer
        Color[,] pixels = new Color[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = image.GetPixel(x, y);
            }
        }

        // Process the pixels using PLINQ
        var pixelData = Enumerable.Range(0, width)
            .SelectMany(x => Enumerable.Range(0, height), (x, y) => new { x, y })
            .AsParallel()
            .Select(p =>
            {
                Color pixelColor = pixels[p.x, p.y];
                if (pixelColor.B > 0)
                {
                    pixelColor = Color.FromArgb(pixelColor.R, pixelColor.G, 255);
                }
                return new { p.x, p.y, pixelColor };
            })
            .ToList();

        foreach (var p in pixelData)
        {
            image.SetPixel(p.x, p.y, p.pixelColor);
        }
    }
}
