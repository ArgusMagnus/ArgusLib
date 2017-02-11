#region Copyright and License
/*
This file is part of ArgusLib.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion

namespace ArgusLib.Reflection
{
	public enum BindingAttributes
	{
		Public = 1 << 0,
		NonPublic = 1 << 1,
		Instance = 1 << 2,
		Static = 1 << 3,
	}
}
