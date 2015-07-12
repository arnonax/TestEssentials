using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAutomationEssentials.Common;
using TestAutomationEssentials.MSTest;

namespace TestAutomationEssentials.UnitTests
{
	[TestClass]
	public class PathUtilsTests
	{
		[TestMethod]
		public void IsInFolderReturnsTrueIfThe1StPathIsContainedInThe2Nd()
		{
			const string parentFolder = @"C:\Root";
			const string childFolder = @"C:\Root\Child\GrandChild";

			Assert.IsTrue(PathUtils.IsInFolder(childFolder, parentFolder));
		}

		[TestMethod]
		public void IsInFolderReturnsFalseEventIfThe1StPathBeginsWithThe2NdPath()
		{
			const string parentFolder = @"C:\Ro";
			const string childFolder = @"C:\Root\Child";

			Assert.IsFalse(PathUtils.IsInFolder(childFolder, parentFolder));
		}

		[TestMethod]
		public void IsInFolderReturnsTrueIfThe1StPathIsRelativeAndThe2NdIsTheCurrentDirectoryAsAbsolute()
		{
			var parentFolder = Directory.GetCurrentDirectory();
			
			Assert.IsTrue(PathUtils.IsInFolder("ChilDir", parentFolder));
		}

		[TestMethod]
		public void IsInFolderReturnsFalseIfThe1StPathIsRelativeButIsNotInsideThe2NdAbsolutePath()
		{
			const string parentFolder = @"C:\Root\Child";

			Assert.IsFalse(PathUtils.IsInFolder("GrandChild", parentFolder));
		}

		[TestMethod]
		public void IsInFolderReturnsFalseIfThe1StPathIsNotADescendantOfThe2NdPath()
		{
			const string parentFolder = @"C:\Root\Child1";
			const string childFolder = @"C:\Root\Child2\GrandChild";

			Assert.IsFalse(PathUtils.IsInFolder(childFolder, parentFolder));
		}

		[TestMethod]
		public void IsInFolderReturnsTrueIf1StPathIsAbsoluteAnd2NdIsRelative()
		{
			const string parentFolder = "RootFolder";
			var childFolder = Path.GetFullPath(@"RootFolder\Child");
			
			Assert.IsTrue(PathUtils.IsInFolder(childFolder, parentFolder));
		}

		[TestMethod]
		public void GetAncestorPathThrowsArgumentNullExceptionIfContainingPathIsNull()
		{
			var ex = TestUtils.ExpectException<ArgumentNullException>(() => PathUtils.GetAncestorPath(containingPath: null, ancestorFolderName:"dummy"));
			Assert.AreEqual("containingPath", ex.ParamName);
		}

		[TestMethod]
		public void GetAncestorPathThrowsArgumentNullExceptionIfAncestorFolderNameIsNull()
		{
			var ex = TestUtils.ExpectException<ArgumentNullException>(() => PathUtils.GetAncestorPath("dummy", ancestorFolderName: null));
			Assert.AreEqual("ancestorFolderName", ex.ParamName);
		}

		[TestMethod]
		public void GetAncestorPathReturnsContainingFolderIfItIsTheAncestorFolder()
		{
			const string containingFolder = @"C:\folder1\folder2";
			const string ancestorFolderName = "folder2";

			Assert.AreEqual(containingFolder, PathUtils.GetAncestorPath(containingFolder, ancestorFolderName));
		}

		[TestMethod]
		public void GetAncestorPathThrowsInvalidOperationExceptionIfTheContainingFolderDoesNotContainTheFolderName()
		{
			const string supposedlyContainingPath = @"C:\folder1\folder2";
			const string supposedlyAncestorFolderName = "folder3";

			TestUtils.ExpectException<InvalidOperationException>(
				() => PathUtils.GetAncestorPath(supposedlyContainingPath, supposedlyAncestorFolderName));
		}

		[TestMethod]
		public void GetAncestorPathShouldReturnTheAncestorPathWhoseNameIsSpecified()
		{
			const string containingPath = @"C:\folder1\folder2\folder3\folder4";
			const string folderName = "folder2";

			var ancestor = PathUtils.GetAncestorPath(containingPath, folderName);
			Assert.AreEqual(@"C:\folder1\folder2", ancestor);
		}

		[TestMethod]
		public void GetAncestorPathShouldReturnFullPathEvenIfContainingFolderIsRelative()
		{
			var currentDir = Directory.GetCurrentDirectory();
			const string containingPath = "folder1\\folder2";
			const string ancestorFolderName = "folder1";
			var result = PathUtils.GetAncestorPath(containingPath, ancestorFolderName);
			Assert.AreEqual(Path.Combine(currentDir, ancestorFolderName), result);
		}

		[TestMethod]
		public void GetAncestorPathShouldReturnCorrectFolderNameEvenIfItsUpperThanTheCurrentDirectory()
		{
			var currentDir = Directory.GetCurrentDirectory();
			const string containingPath = "folder1\\folder2";
			var ancestorFolderName = Path.GetFileName(Path.GetFullPath(Path.Combine(currentDir, "..\\.."))); // name of 2 folders up the current dir.
			var result = PathUtils.GetAncestorPath(containingPath, ancestorFolderName);
			Assert.IsTrue(PathUtils.IsInFolder(currentDir, result));
		}

		[TestMethod]
		public void PartialFolderNameIsNotConsideredAsAncestor()
		{
			const string containingFolder = @"C:\folder1\folder2\folder3";
			TestUtils.ExpectException<InvalidOperationException>(() => PathUtils.GetAncestorPath(containingFolder, "folder"));
			TestUtils.ExpectException<InvalidOperationException>(() => PathUtils.GetAncestorPath(containingFolder, "older2"));
		}

		[TestMethod]
		public void GetAncestorPathCanAcceptAPathPortionAndNotOnlySingleFolderName()
		{
			const string containingFolder = @"C:\folder1\folder2\folder3\folder4\folder3";
			const string ancestorFolderName = @"folder2\folder3";
			const string expectedResult = @"C:\folder1\folder2\folder3";
			Assert.AreEqual(expectedResult, PathUtils.GetAncestorPath(containingFolder, ancestorFolderName));
		}
	}
}
