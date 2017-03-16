.class public abstract auto ansi sealed beforefieldinit ArgusLib.EnumExtensions
       extends [System.Runtime]System.Object
{
	.custom instance void [System.Runtime]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 )

	.method public hidebysig static bool  ContainsFlag<valuetype .ctor ([System.Runtime]System.Enum) T>(!!T bitSet, !!T flag) cil managed
	{
		.custom instance void [System.Runtime]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 )
		// Code size       8 (0x8)
		.maxstack  2
		IL_0000:  ldarg.0
		IL_0001:  ldarg.1
		IL_0002:  call       bool class ArgusLib.BitSet`1<!!T>::HasFlag(!0, !0)
		IL_0007:  ret
	} // end of method BitSet::ContainsFlag

	.method public hidebysig static !!T  SetFlag<valuetype .ctor ([System.Runtime]System.Enum) T>(!!T bitSet, !!T flag) cil managed
	{
		.custom instance void [System.Runtime]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 )
		// Code size       8 (0x8)
		.maxstack  2
		IL_0000:  ldarg.0
		IL_0001:  ldarg.1
		IL_0002:  call       !0 class ArgusLib.BitSet`1<!!T>::SetFlag(!0, !0)
		IL_0007:  ret
	} // end of method BitSet::SetFlag

	.method public hidebysig static !!T  ClearFlag<valuetype .ctor ([System.Runtime]System.Enum) T>(!!T bitSet, !!T flag) cil managed
	{
		.custom instance void [System.Runtime]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 )
		// Code size       8 (0x8)
		.maxstack  2
		IL_0000:  ldarg.0
		IL_0001:  ldarg.1
		IL_0002:  call       !0 class ArgusLib.BitSet`1<!!T>::ClearFlag(!0, !0)
		IL_0007:  ret
	} // end of method BitSet::ClearFlag
}