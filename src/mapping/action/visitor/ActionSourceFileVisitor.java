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

package mapping.action.visitor;

import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import mapping.KDMElementFactory;
import mapping.KDMElementFactory.GlobalKind;
import mapping.code.parser.ISourceFileTypeParser;
import mapping.source.visitor.DefaultSourceFileVisitor;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.OperationCanceledException;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.Logger;

public class ActionSourceFileVisitor extends DefaultSourceFileVisitor {

	private static final Logger									LOGGER	= new Logger(ActionSourceFileVisitor.class);

	private final IProgressMonitor								monitor;
	private final CodeModel										internalCodeModel;
	private final ConcurrentMap<String, ISourceFileTypeParser>	sourceFileParsers;

	private final ExecutorService								executorService;

	private long												start;

	public ActionSourceFileVisitor(final IProgressMonitor monitor) {
		this.monitor = monitor;
		this.internalCodeModel = KDMElementFactory.createGenericCodeModel("Internal CodeModel", GlobalKind.INTERNAL);
		this.sourceFileParsers = new ConcurrentHashMap<String, ISourceFileTypeParser>();
		executorService = Executors.newFixedThreadPool(Runtime.getRuntime().availableProcessors());
	}

	@Override
	public void visitSourceFile(final SourceFile sourceFile) {
		if (monitor.isCanceled()) throw new OperationCanceledException();
		//		executorService.execute(new Runnable() {
		//			@Override
		//			public void run() {
		//				processSourceFile(sourceFile);
		//			}
		//		});
		processSourceFile(sourceFile);
	}

	@Override
	public void beforeWalk() {
		start = System.currentTimeMillis();
	}

	@Override
	public void afterWalk() {
		long end = System.currentTimeMillis();
		long duration = end - start;
		LOGGER.info("Took " + duration + " msec");
		LOGGER.fine("Waiting...");
		//		executorService.shutdown();
		//		try {
		//			executorService.awaitTermination(0, TimeUnit.SECONDS);
		//		} catch (InterruptedException e) {
		//			LOGGER.error(e);
		//		}
		LOGGER.fine("...Done.");
	}

	@Override
	public CodeModel getInternalCodeModel() {
		return this.internalCodeModel;
	}

	public void addSourceFileParser(final ISourceFileTypeParser sourceFileParser) {
		sourceFileParsers.put(sourceFileParser.getLanguageString(), sourceFileParser);
	}

	private void processSourceFile(final SourceFile sourceFile) {
		ISourceFileTypeParser sourceFileParser = sourceFileParsers.get(sourceFile.getLanguage());
		if (sourceFileParser == null) {
			LOGGER.warning("No appropriate source file parser found for language '" + sourceFile.getPath() + "'.");
			return;
		}

		try {
			sourceFileParser.readInto(sourceFile, internalCodeModel, monitor);
		} catch (RuntimeException e) {
			LOGGER.warning("Exception while parsing " + sourceFile.getPath());
			// e.printStackTrace();
			throw e;
		}
	}

}
