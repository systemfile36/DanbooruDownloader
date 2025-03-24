using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing; //for using Mutate

namespace DanbooruDownloader.Utilities
{
    public static class ImageUtility
    {
        /// <summary>
        /// File extension(or format) of image that resized by this Utility.
        /// </summary>
        public const string RESIZE_FILE_EXTENSION = "png";

        /// <summary>
        /// Return resized Image as Stream 
        /// </summary>
        /// <param name="imageStream">Image to resize. as Stream</param>
        /// <param name="targetWidth">target width</param>
        /// <param name="targetHeight">target height</param>
        /// <returns>Resized Image. image format is PNG.</returns>
        public static async Task<Stream> ResizeImage(Stream imageStream, int targetWidth, int targetHeight)
        {
            MemoryStream output = new MemoryStream();

            await ResizeImage(imageStream, output, targetWidth, targetHeight);

            return output;
        }

        /// <summary>
        /// Resize image and write it to output stream. format of image is PNG
        /// </summary>
        /// <param name="imageStream">Image to resize. as Stream</param>
        /// <param name="outputStream">Output Stream. result will be written to this.</param>
        /// <param name="targetWidth">target width</param>
        /// <param name="targetHeight">target height</param>
        public static async Task ResizeImage(Stream imageStream, Stream outputStream, int targetWidth, int targetHeight)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                //Copy to MemoryStream to prevent NotSupportedException by ReadAsync 
                await imageStream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;

                //Read Image from stream
                using (Image<Rgba32> image = await Image.LoadAsync<Rgba32>(memoryStream))
                {
                    //Resize image
                    image.Mutate(ctx => ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(targetWidth, targetHeight),
                        Mode = ResizeMode.Pad,
                        Sampler = KnownResamplers.Box
                    }));

                    //Write image to stream as PNG
                    await image.SaveAsPngAsync(outputStream);
                    await outputStream.FlushAsync();
                }
            }
        }
    }
}
