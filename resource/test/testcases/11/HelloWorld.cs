using System;

namespace HelloWorld
{
	class CastMe {
		CastMe var;
		
		void TestMethod() {}

		public object ToObject() {
			return "simple text";
		}
	}

    class Hello {
    	CastMe var2;

    	void main() {
    		CastMe c = new CastMe();
    		/*Hello h = new Hello();

    		((CastMe)c).var = null;
    		((Hello)h).var2.TestMethod();
    		c.var = null;
			h.var2 = null;
			var2.TestMethod();*/
    	}    	
    }
}
