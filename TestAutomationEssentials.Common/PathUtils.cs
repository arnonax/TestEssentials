using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TestAutomationEssentials.Common
{
	/// <summary>
	/// Provide useful methods to work with file-system paths, beyond those in <see cref="Path"/>
	/// </summary>
	public class PathUtils
	{
		/// <summary>
		/// Determines whether the a file or folder is contained in another folder
		/// </summary>
		/// <param name="path">The path of the file or folder. This can be either a relative or full path</param>
		/// <param name="containingFolder">The path of the folder to check if it contains <paramref name="path"/>. This can be eitehr a relative or full path</param>
		/// <returns><b>true</b> if <paramref name="path"/> is inside <paramref name="containingFolder"/>, otherwise <b>false</b></returns>
		public static bool IsInFolder(string path, string containingFolder)
		{
			return Path.GetFullPath(path).StartsWith(Path.GetFullPath(containingFolder + '\\'));
		}

		/// <summary>
		/// Returns the absolute path of the desktop folder
		/// </summary>
		/// <returns>The absolute path of the desktop folder</returns>
		[ExcludeFromCodeCoverage] // This method is a syntactic sugar of its content and has no value writing a unit test for it.
		public static string GetDesktopFolder()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		}


		/// <summary>
		/// Returns the full path of the an ancestor folder of another path
		/// </summary>
		/// <param name="containingPath">A path to a file or folder inside <paramref name="ancestorFolderName"/>. If this path is relative, it is assumed to be relative to the current directory</param>
		/// <param name="ancestorFolderName">The name of a folder that should be up the hierarchy of <paramref name="containingPath"/>. It's also possible to specify multiple nested folders that should be together in the hierarchy of <paramref name="containingPath"/></param>
		/// <returns>The full path of <paramref name="ancestorFolderName"/></returns>
		/// <exception cref="ArgumentNullException"><paramref name="containingPath"/> or <paramref name="ancestorFolderName"/> is <b>null</b></exception>
		/// <exception cref="InvalidOperationException"><paramref name="ancestorFolderName"/> cannot be found up the hierarchy of <paramref name="containingPath"/></exception>
		/// <example>
		/// var ancestor = PathUtils.GetAncestorPath(@"C:\folder1\folder2\folder3\folder4", "folder2");
		/// // ancestor == @"c:\folder1\folder2
		/// </example>
		/// <example>
		/// var ancestor = PathUtils.GetAncestorPath(@"C:\folder1\folder2\folder3\folder4\folder3", "@"folder2\folder3"");
		/// // ancestor == @"C:\folder1\folder2\folder3
		/// </example>
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
