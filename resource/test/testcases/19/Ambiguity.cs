namespace N1 {
    public class C { }
}

namespace N2 {
	public class C {}
}

namespace N
{
    using N1;
    using N2;

    class A
    {
        void Main()
        {
            new C();	// ambiguity -> compiler error
        }
    }
}
