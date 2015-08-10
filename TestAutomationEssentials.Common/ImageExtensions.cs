using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

namespace TestAutomationEssentials.Common
{
	/// <summary>
	/// Provides useful methods to work with Images
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class ImageExtensions
	{
		/// <summary>
		/// Creates an image from a byte array
		/// </summary>
		/// <param name="bytes">A byte array that comprise the image, in every format that may be stored in a valid image file</param>
		/// <returns>The corresponding image</returns>
		public static Image ToImage(this byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				return Image.FromStream(stream);
			}
		}

		/// <summary>
		/// Resizes the given image to fit in the specified size
		/// </summary>
		/// <param name="image">The image to resize</param>
		/// <param name="targetSize">The target size of the image</param>
		/// <returns>A new <see cref="Bitmap"/> object that contains the resized image</returns>
		/// <remarks>
		/// The method can either shrink or grow the original image, but always keeps the original ratio. If the ratio of the original
		/// image is not the same as of the target size, then the image is centered and the borders (either top & bottom or left & right)
		/// are left empty (black)
		/// </remarks>
		public static Bitmap FitToSize(this Image image, Size targetSize)
		{
			if (image == null)
				throw new ArgumentNullException("image");
			
			if (targetSize.Width <= 0 || targetSize.Height <= 0)
				throw new ArgumentOutOfRangeException("targetSize");

			var result = new Bitmap(targetSize.Width, targetSize.Height);

			var sourceRatio = image.Width/image.Height;
			var targetRatio = targetSize.Width/targetSize.Height;

			Rectangle rect;
			if (sourceRatio > targetRatio)
			{
				var targetHeight = image.Height*targetSize.Width/image.Width;
				rect = new Rectangle(0, (targetSize.Height - targetHeight) / 2, targetSize.Width, targetHeight);
			}
			else
			{
				var targetWidth = image.Width*targetSize.Height/image.Height;
				rect = new Rectangle((targetSize.Width-targetWidth)/2, 0, targetWidth, targetSize.Height);
			}

			using (var graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(image, rect);
			}
			return result;
		}
	}
}