namespace testpackage {
	public class TestClass {
	    public static int main(String[] args) {
	    	bool b = true && false && true;
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
			return i==10 ? 77 : 42;
	    }
	}
}
