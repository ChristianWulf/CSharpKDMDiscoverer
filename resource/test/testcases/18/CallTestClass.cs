public class TestClass {
    public static void main(String[] args) {
		method1();
    }
	
	public static void method1() {
		int i = 3;
		int x = method2(i, "example", method3());
	}

	public static int method2(int i, String x, long y) {
		System.Console.WriteLine(x);
		return i-1;
	}

	public static long method3() {
		return 3;
	}
}
