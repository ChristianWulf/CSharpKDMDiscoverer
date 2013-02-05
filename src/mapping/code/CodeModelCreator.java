/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package mapping.code;

import java.util.Collection;
import java.util.Collections;
import java.util.Properties;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;

import mapping.KDMElementFactory;
import mapping.KDMElementFactory.GlobalKind;
import mapping.action.visitor.ActionSourceFileVisitor;
import mapping.code.extern.ExternalDatatypeInfoRepository;
import mapping.code.extern.loader.DatatypeInfoCacheLoader;
import mapping.code.extern.loader.DatatypeInfoFileSystemLoader;
import mapping.code.parser.CSharpMemberDeclarationParser;
import mapping.code.parser.CSharpSourceFileParser;
import mapping.code.parser.CSharpTypeParser;
import mapping.code.visitor.LanguageUnitDetectorVisitor;
import mapping.code.visitor.QueuingSourceFileVisitor;
import mapping.code.visitor.SequentialParseVisitor;
import mapping.source.InventoryModelWalker;
import mapping.source.visitor.SourceFileCounter;
import mapping.source.visitor.SourceFileVisitor;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.core.runtime.OperationCanceledException;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.LanguageUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

public class CodeModelCreator {

	private final String				rootDir;

	private CodeModel					internalCodeModel;
	private CodeModel					externalCodeModel;
	private LanguageUnitDetectorVisitor	languageUnitDetectorVisitor;

	private Module						valueRepository;

	private final int					toPhase;

	public CodeModelCreator(final Properties prop, final int toPhase) {
		this.toPhase = toPhase;
		rootDir = prop.getProperty("ExternalDatatypeInfoRepository.rootDir");
	}

	public void buildCodeModel(final InventoryModel inventoryModel, IProgressMonitor monitor) {
		// ensure there is a monitor of some sort
		if (monitor == null) monitor = new NullProgressMonitor();

		final InventoryModelWalker walker = new InventoryModelWalker(inventoryModel);

		SourceFileCounter sourceFileCounter = new SourceFileCounter();
		walker.walk(sourceFileCounter);
		final int numSourceFiles = sourceFileCounter.getAmount();

		// 3 passes + language detection
		monitor.beginTask("Extracting code model from inventory model...", 1 + toPhase * numSourceFiles);
		try {
			monitor.subTask("Searching for used programming languages...");
			languageUnitDetectorVisitor = new LanguageUnitDetectorVisitor();
			walker.walk(languageUnitDetectorVisitor);
			monitor.worked(1);

			if (monitor.isCanceled()) throw new OperationCanceledException();
			// process inventory model
			buildCodeModels(walker, monitor);

			// internalCodeModel = sourceFileTransformer.getInternalCodeModel();
			// internalCodeModel.getCodeElement().addAll(necessaryLanguageUnits);
			//
			// externalCodeModel = sourceFileTransformer.getExternalCodeModel();
		} finally {
			monitor.done();
		}
	}

	private void buildCodeModels(final InventoryModelWalker walker, final IProgressMonitor monitor) {
		// external datatypeinfo repository
		DatatypeInfoCacheLoader datatypeInfoLoader = new DatatypeInfoCacheLoader(new DatatypeInfoFileSystemLoader());
		ExternalDatatypeInfoRepository externalDatatypeInfoRepository = new ExternalDatatypeInfoRepository(rootDir,
				datatypeInfoLoader);

		// 1. transformation
		if (toPhase < 1) return;
		monitor.subTask("Transforming own types...");
		CSharpTypeParser sourceFileTypeParser = new CSharpTypeParser();
		CodeModel initialInternalCodeModel = buildTypes(sourceFileTypeParser, walker, monitor);

		if (monitor.isCanceled()) throw new OperationCanceledException();

		// 2. transformation
		if (toPhase < 2) return;
		monitor.subTask("Transforming own types' member declarations and method definitions...");
		CSharpMemberDeclarationParser cSharpParser1 = new CSharpMemberDeclarationParser(languageUnitDetectorVisitor,
				externalDatatypeInfoRepository);
		buildMemberDeclarations(initialInternalCodeModel, cSharpParser1, walker, monitor);

		if (monitor.isCanceled()) throw new OperationCanceledException();

		// 3. transformation
		if (toPhase < 3) return;
		monitor.subTask("Transforming expressions...");
		CSharpSourceFileParser cSharpParser2 = new CSharpSourceFileParser(languageUnitDetectorVisitor,
				externalDatatypeInfoRepository);
		buildSequentially(internalCodeModel, cSharpParser2, walker, monitor);

		// buildConcurrently(cSharpParser, walker, monitor);
	}

	private CodeModel buildTypes(final CSharpTypeParser sourceFileTypeParser, final InventoryModelWalker walker,
			final IProgressMonitor monitor) {
		ActionSourceFileVisitor visitor = new ActionSourceFileVisitor(monitor);
		visitor.addSourceFileParser(sourceFileTypeParser);
		walker.walk(visitor);

		// both only for test purposes
		this.internalCodeModel = visitor.getInternalCodeModel();
		this.externalCodeModel = KDMElementFactory.createGenericCodeModel("External CodeModel", GlobalKind.EXTERNAL);

		return visitor.getInternalCodeModel();
	}

	private void buildMemberDeclarations(final CodeModel initialInternalCodeModel,
			final CSharpMemberDeclarationParser cSharpParser, final InventoryModelWalker walker,
			final IProgressMonitor monitor) {
		this.valueRepository = KDMElementFactory.createValueRepository(initialInternalCodeModel);

		SequentialParseVisitor visitor = new SequentialParseVisitor(initialInternalCodeModel, externalCodeModel,
				monitor, valueRepository);
		visitor.addSourceFileParser(cSharpParser);
		walker.walk(visitor);

		this.internalCodeModel = visitor.getInternalCodeModel();
		this.externalCodeModel = visitor.getExternalCodeModel();
	}

	private void buildSequentially(final CodeModel passedInternalCodeModel, final CSharpSourceFileParser cSharpParser,
			final InventoryModelWalker walker, final IProgressMonitor monitor) {
		SequentialParseVisitor visitor = new SequentialParseVisitor(passedInternalCodeModel, externalCodeModel,
				monitor, valueRepository);
		visitor.addSourceFileParser(cSharpParser);
		walker.walk(visitor);

		this.internalCodeModel = visitor.getInternalCodeModel();
		this.externalCodeModel = visitor.getExternalCodeModel();
	}

	// TODO does not yet work
	private void buildConcurrently(final CSharpSourceFileParser cSharpParser, final InventoryModelWalker walker,
			final IProgressMonitor monitor) {
		final BlockingQueue<SourceFile> queue = new LinkedBlockingQueue<SourceFile>();
		BlockingQueue<Thread> finishedQueue = new LinkedBlockingQueue<Thread>();
		int numThreads = Runtime.getRuntime().availableProcessors();
		numThreads = 1;

		monitor.subTask("Building code models in concurrent mode with " + numThreads + " thread(s)...");

		for (int i = 0; i < numThreads; i++) {
			SourceFile2ModelTransformator transformator = new SourceFile2ModelTransformator(queue, finishedQueue,
					monitor);
			transformator.setName("Transformator " + i);
			transformator.addSourceFileParser(cSharpParser);
			transformator.start();
		}

		Thread producer = new Thread(new Runnable() {
			@Override
			public void run() {
				SourceFileVisitor visitor = new QueuingSourceFileVisitor(queue);
				walker.walk(visitor);
			}
		});
		producer.start();

		// 1. wait for the visitor to terminate
		try {
			producer.join();
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		// 2. add as much END_OF_QUEUE_TOKEN as consumer threads are running
		queue.addAll(Collections.nCopies(numThreads, SourceFile2ModelTransformator.END_OF_QUEUE_TOKEN));
		// TODO 1. and 2. can be done in another thread to not freeze the main
		// thread

		// wait for all consumer threads and process the consumer's models as
		// soon as it has
		// terminate
		mergeAllThreadsCodeModels(finishedQueue, numThreads, monitor);
	}

	private void mergeAllThreadsCodeModels(final BlockingQueue<Thread> finishedQueue, int activeThreadCount,
			final IProgressMonitor monitor) {
		monitor.subTask("Merging code models...");
		try {
			while (activeThreadCount > 0) {
				Thread t = finishedQueue.take();
				SourceFile2ModelTransformator transformator = (SourceFile2ModelTransformator) t;

				merge(transformator.getInternalCodeModel(), transformator.getExternalCodeModel());

				activeThreadCount--;
			}
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		monitor.worked(1);
	}

	private void merge(final CodeModel internalCodeModel2, final CodeModel externalCodeModel2) {
		// for the first thread
		// TODO remove if by instantiating empty code models before merge
		// operation
		if (internalCodeModel == null && externalCodeModel == null) {
			internalCodeModel = internalCodeModel2;
			externalCodeModel = externalCodeModel2;
		} else { // for all following threads
			// TODO
		}
	}

	public CodeModel getInternalCodeModel() {
		return this.internalCodeModel;
	}

	public CodeModel getExternalCodeModel() {
		return this.externalCodeModel;
	}

	public Collection<LanguageUnit> getNeccessaryLanguageUnits() {
		return languageUnitDetectorVisitor.getNecessaryLanguageUnits();
	}

}
