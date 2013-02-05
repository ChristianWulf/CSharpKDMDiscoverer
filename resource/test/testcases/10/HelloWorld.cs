using System;

namespace HelloWorld
{
    class Hello : object {
    	public string var1;
    	protected String var2a, var2b;
    	protected internal int var3;
    	internal float var4;
    	private bool var5;
    	readonly short var6;
    	const int* var7 = null;
    	public const bool** var8 = null;
    	internal protected string[] var9;
    	
    	// TODO more modifiers (combinations)
    	public static readonly Uri UriSchemeFile1;	// example from System.Uri
    	private const System.Uri UriSchemeFile2 = null;
    	
    	public static String prop1 {
    		get {return var1;}
    	}

    	// field declaration and assignment
    	//protected String varAssign = "Test";
    	//public int num1=1, num2=2;
    	
    	// Aliased type
    	//public Int32 varSystemInt32;
    	//public Int64 varSystemInt64;
    	// TODO
    }
}
