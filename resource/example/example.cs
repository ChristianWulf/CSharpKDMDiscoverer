using System.IO;

namespace NS {
	class A {
		Zoom z;		// contained in namespace NS
		File f;		// contained in namespace System.IO
		
		void execute() {
			z = doZoom();
		}
		
		Zoom doZoom() {
			//[...]
		}
	}
}
