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

package mapping.code.parser;

import java.io.FileNotFoundException;
import java.io.IOException;

import lang.csharp.CSharp4AST;
import lang.csharp.CSharp4AST.compilation_unit_return;
import lang.csharp.CSharp4PreProcessorImpl;
import mapping.IKDMMapper;
import mapping.KDMElementFactory;
import mapping.code.extern.ExternalDatatypeInfoRepository;
import mapping.code.language.GenericLanguageUnitCache;
import mapping.code.transformator.Phase3Transformator;
import mapping.code.visitor.LanguageUnitDetectorVisitor;

import org.antlr.runtime.CharStream;
import org.antlr.runtime.CommonTokenStream;
import org.antlr.runtime.Lexer;
import org.antlr.runtime.RecognitionException;
import org.antlr.runtime.tree.CommonTree;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.code.CodeModel;
import org.eclipse.gmt.modisco.omg.kdm.code.CompilationUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.Logger;
import util.Parsing;
import util.unicode.ANTLRFileStreamWithBOM;

public class CSharpSourceFileParser implements ISourceFileParser {

	private static final Logger						LOGGER	= new Logger(CSharpSourceFileParser.class);
	private final GenericLanguageUnitCache			genericLanguageUnitCache;
	private final ExternalDatatypeInfoRepository	externalDatatypeInfoRepository;

	/**
	 * The constructor may only take thread-safe parameters.
	 * 
	 * @param languageUnitDetectorVisitor
	 * @param externalDatatypeInfoRepository
	 */
	public CSharpSourceFileParser(final LanguageUnitDetectorVisitor languageUnitDetectorVisitor,
			final ExternalDatatypeInfoRepository externalDatatypeInfoRepository) {
		this.genericLanguageUnitCache = languageUnitDetectorVisitor.getCache(getLanguageString());
		this.externalDatatypeInfoRepository = externalDatatypeInfoRepository;
	}

	@Override
	public void readInto(final SourceFile sourceFile, final CodeModel internalCodeModel,
			final Module valueRepository, final CodeModel externalCodeModel,
			final IProgressMonitor monitor) {
		try {
			CharStream input = new ANTLRFileStreamWithBOM(sourceFile.getPath(), "UTF-8");
			Lexer lex = new CSharp4PreProcessorImpl(input, "NET_4_0");
			CommonTokenStream tokens = new CommonTokenStream(lex);
			CSharp4AST parser = new CSharp4AST(tokens);

			LOGGER.info("Parsing file..." + sourceFile.getPath());
			compilation_unit_return cunit = parser.compilation_unit();

			if (cunit.getTree() == null) {
				// deprecated text: file is completely uncommented or empty, skip further processing
				//				LOGGER.info("File contains only comments or nothing. We skip it.");

				CompilationUnit cu = KDMElementFactory.createCompilationUnit(sourceFile);
				internalCodeModel.getCodeElement().add(cu);

				monitor.worked(1);
				return;
			}

			LOGGER.info("Creating AST...");
			CommonTree tree = Parsing.buildAST(cunit, tokens);

			LOGGER.info("Transforming AST...");
			IKDMMapper<CompilationUnit> fileMapper;
			// fileMapper = new FileMapper(languageUnitCache, internalCodeModel,
			// externalCodeModel, externalDatatypeInfoRepository);
			fileMapper = new Phase3Transformator(genericLanguageUnitCache, internalCodeModel,
					valueRepository, externalCodeModel, externalDatatypeInfoRepository);
			fileMapper.transform(tree, sourceFile);
			// get the new CompilationUnit (list contains only one CompilationUnit)
			// List<CompilationUnit> compilationUnits = fileMapper.getMappingResult();
			// // and add it to the internal CodeModel
			// internalCodeModel.getCodeElement().addAll(compilationUnits);
			// TODO

			monitor.worked(1);
		} catch (FileNotFoundException e) {
			LOGGER.info("Source file does not exist ('" + sourceFile.getPath() + "')");
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (RecognitionException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	@Override
	public String getLanguageString() {
		return "C#";
	}

}
