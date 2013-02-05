/***************************************************************************
 * Copyright 2012 by
 * + Christian-Albrechts-University of Kiel
 * + Department of Computer Science
 * + Software Engineering Group
 * and others.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***************************************************************************/

package mapping.code.visitor;

import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import mapping.code.parser.ISourceFileParser;
import mapping.source.visitor.DefaultSourceFileVisitor;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.OperationCanceledException;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.Logger;

public class SequentialParseVisitor extends DefaultSourceFileVisitor {

	private static final Logger LOGGER = new Logger(SequentialParseVisitor.class);

	private final CodeModel internalCodeModel;
	private final CodeModel externalCodeModel;
	private final IProgressMonitor monitor;
	private final Module valueRepository;
	private final ConcurrentMap<String, ISourceFileParser> sourceFileParsers;

	public SequentialParseVisitor(final CodeModel internalCodeModel,
			final CodeModel externalCodeModel, final IProgressMonitor monitor,
			final Module valueRepository) {
		this.internalCodeModel = internalCodeModel;
		this.externalCodeModel = externalCodeModel;
		this.monitor = monitor;
		this.valueRepository = valueRepository;
		this.sourceFileParsers = new ConcurrentHashMap<String, ISourceFileParser>();
	}

	@Override
	public void visitSourceFile(final SourceFile sourceFile) {
		if (monitor.isCanceled())
			throw new OperationCanceledException();
		processSourceFile(sourceFile);
	}

	@Override
	public CodeModel getInternalCodeModel() {
		return internalCodeModel;
	}

	@Override
	public CodeModel getExternalCodeModel() {
		return externalCodeModel;
	}

	public void addSourceFileParser(final ISourceFileParser sourceFileParser) {
		sourceFileParsers.put(sourceFileParser.getLanguageString(),
				sourceFileParser);
	}

	private void processSourceFile(final SourceFile sourceFile) {
		ISourceFileParser sourceFileParser = sourceFileParsers.get(sourceFile
				.getLanguage());
		if (sourceFileParser == null) {
			LOGGER.warning("No appropriate source file parser found for language '"
					+ sourceFile.getLanguage()
					+ "' in file "
					+ sourceFile.getPath() + ".");
			return;
		}

		try {
			sourceFileParser.readInto(sourceFile, internalCodeModel,
					valueRepository, externalCodeModel, monitor);
		} catch (RuntimeException e) {
			LOGGER.warning("Exception while parsing " + sourceFile.getPath());
			// e.printStackTrace();
			throw e;
		}
	}

	public Module getValueRepository() {
		return valueRepository;
	}

}
