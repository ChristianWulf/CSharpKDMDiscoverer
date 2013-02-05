namespace MyNS {

	class Base {
		float f;
		public class InnerClass {
			public float temp;
		}
	}

	class SubClass : Base {
		void Show(InnerClass cl) {
			f = cl.temp;
		}
	}

}
