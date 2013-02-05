package testpackage;

public class TestClass {
    public static void main(String[] args) {
		int i = 9;
		if (i == 5) {
			i = 3;
			int x = 5;
			int y = x + i;
		} else if (i == 6) {
			i = 9;
		} else {
			i = 10;
		}
    }
}