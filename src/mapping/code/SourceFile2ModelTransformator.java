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

import java.util.concurrent.BlockingQueue;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import mapping.KDMElementFactory;
import mapping.KDMElementFactory.GlobalKind;
import mapping.code.parser.ISourceFileParser;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFactory;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.Logger;

public class SourceFile2ModelTransformator extends Thread {
	// strategy: END_OF_QUEUE_TOKEN
	/*
	 * one token for all consumer threads:
	 * pros: *no logic necessary in consumer thread to pass token to the next waiting thread
	 * *producer thread does not need to know the number of consumers
	 * (important aspect for a variable number of consumer as well as producer threads)
	 * negs: *n-1 additional (END_OF_QUEUE_TOKEN) tokens in queue
	 * one token for each consumer thread:
	 * pros and negs vice versa
	 */
	public static final SourceFile							END_OF_QUEUE_TOKEN;

	private static final Logger								LOGGER	= new Logger(SourceFile2ModelTransformator.class);

	static {
		END_OF_QUEUE_TOKEN = SourceFactory.eINSTANCE.createSourceFile();
	}

	private final BlockingQueue<SourceFile>					queue;
	private final BlockingQueue<Thread>						finishedQueue;
	private final CodeModel									internalCodeModel;
	private final CodeModel									externalCodeModel;
	private final ConcurrentMap<String, ISourceFileParser>	sourceFileParsers;
	private final IProgressMonitor							monitor;
	private final Module									valueRepository;

	public SourceFile2ModelTransformator(final BlockingQueue<SourceFile> queue,
			final BlockingQueue<Thread> finishedQueue, final IProgressMonitor monitor) {
		this.queue = queue;
		this.finishedQueue = finishedQueue;
		this.monitor = monitor;
		this.sourceFileParsers = new ConcurrentHashMap<String, ISourceFileParser>();
		this.internalCodeModel = KDMElementFactory.createGenericCodeModel("Internal CodeModel", GlobalKind.INTERNAL);
		this.externalCodeModel = KDMElementFactory.createGenericCodeModel("External CodeModel", GlobalKind.EXTERNAL);
		this.valueRepository = KDMElementFactory.createValueRepository(internalCodeModel);
	}

	@Override
	public void run() {
		try {
			try {
				while (!Thread.interrupted()) {
					SourceFile sf = queue.take();
					// important: compare object reference here, i.e. use the == operator
					if (sf == END_OF_QUEUE_TOKEN) {
						break;
					}
					processSourceFile(sf);
				}
			} catch (InterruptedException e) {
				// terminate
			}
		} finally {
			// notify that this thread finished execution
			finishedQueue.add(this);
		}
	}

	public void addSourceFileParser(final ISourceFileParser sourceFileParser) {
		sourceFileParsers.put(sourceFileParser.getLanguageString(), sourceFileParser);
	}

	private void processSourceFile(final SourceFile sourceFile) {
		ISourceFileParser sourceFileParser = sourceFileParsers.get(sourceFile.getLanguage());
		if (sourceFileParser == null) {
			LOGGER.warning("No appropriate source file parser found for language '" + sourceFile.getPath() + "'.");
			return;
		}

		try {
			sourceFileParser.readInto(sourceFile, internalCodeModel, valueRepository, externalCodeModel, monitor);
		} catch (RuntimeException e) {
			LOGGER.warning("Exception while parsing " + sourceFile.getPath());
			// e.printStackTrace();
			throw e;
		}
	}

	public CodeModel getInternalCodeModel() {
		return this.internalCodeModel;
	}

	public CodeModel getExternalCodeModel() {
		return this.externalCodeModel;
	}

}
