package org.eclipse.modisco.csharp.discoverer.internal;

import java.io.File;
import java.util.Queue;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.emf.ecore.resource.Resource;

public class Transformator extends Thread {

	private final Queue<File>		tasks;
	private final IProgressMonitor	monitor;
	private Resource				resource;

	public Transformator(final Queue<File> queues, final IProgressMonitor monitor) {
		this.tasks = queues;
		this.monitor = monitor;
	}

	@Override
	public void run() {
		// add as constructor parameter (read access need not be synched)
		//		final CSharpLanguageUnit languageUnitCache = new CSharpLanguageUnit();

		while (!this.tasks.isEmpty()) {
			if (this.monitor.isCanceled()) {
				break;
			}
			File file = this.tasks.poll();
			String filename = file.getAbsolutePath();


			// TODO
			//			try {
			//				CSLexer lex = new CSLexer(new ANTLRFileStream(filename));
			//				CommonTokenStream tokens = new CommonTokenStream(lex);
			//				CSParser parser = new CSParser(tokens);
			//				// parser.setTreeAdaptor(new CSharpTreeAdaptor());
			//
			//				this.monitor.setTaskName("Parsing " + filename);
			//				compilation_unit_return cunit = parser.compilation_unit();
			//
			//				this.monitor.setTaskName("Building AST for " + filename);
			//				CommonTree ast = Parsing.buildAST(cunit, tokens);
			//
			//				this.monitor.setTaskName("Transforming file to KDM package");
			//				CompilationUnit transform = new FileMapper(languageUnitCache).transform(ast);
			//
			//				this.resource.getContents().add(transform);
			//			} catch (IOException e) {
			//				// TODO Auto-generated catch block
			//				e.printStackTrace();
			//			} catch (RecognitionException e) {
			//				// TODO Auto-generated catch block
			//				e.printStackTrace();
			//			}
		}
	}

	public Resource getResource() {
		return this.resource;
	}
}
