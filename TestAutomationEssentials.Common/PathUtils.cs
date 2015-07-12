using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TestAutomationEssentials.Common
{
	public class PathUtils
	{
		public static bool IsInFolder(string folderPath, string containingFolder)
		{
			return Path.GetFullPath(folderPath).StartsWith(Path.GetFullPath(containingFolder + '\\'));
		}

		[ExcludeFromCodeCoverage] // This method is a syntactic sugar of its content and has no value writing a unit test for it.
		public static string GetDesktopFolder()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		}

		public static string GetAncestorPath(string containingPath, string ancestorFolderName)
		{
			if (containingPath == null)
				throw new ArgumentNullException("containingPath");

			if (ancestorFolderName == null)
				throw new ArgumentNullException("ancestorFolderName");

			ancestorFolderName = '\\' + ancestorFolderName;
			containingPath = Path.GetFullPath(containingPath);

			if (containingPath.EndsWith(ancestorFolderName))
				return containingPath;

			ancestorFolderName += '\\';
			var lastIndex = containingPath.LastIndexOf(ancestorFolderName, StringComparison.InvariantCulture);
			if (lastIndex ==-1)
				throw new InvalidOperationException(string.Format("Folder name '{0}' is not an ancestor of '{1}'", ancestorFolderName, containingPath));
			
			lastIndex = lastIndex + ancestorFolderName.Length;
			return containingPath.Remove(lastIndex - 1);
		}
	}
}
