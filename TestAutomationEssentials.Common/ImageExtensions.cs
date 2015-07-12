using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

namespace TestAutomationEssentials.Common
{
	[ExcludeFromCodeCoverage]
	public static class ImageExtensions
	{
		public static Image ToImage(this byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				return Image.FromStream(stream);
			}
		}

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