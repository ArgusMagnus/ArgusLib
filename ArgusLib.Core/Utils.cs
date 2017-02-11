#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion

namespace ArgusLib
{
	static class Utils
    {
		static readonly string _solutionDirectory = Resources.DevelopmentEnvironment.SolutionDirectoryFile;

		public static string SolutionDirectory => _solutionDirectory;
    }
}
