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

package util;

import java.io.IOException;

import lang.csharp.CSharp4AST;
import lang.csharp.CSharp4AST.compilation_unit_return;
import lang.csharp.CSharp4PreProcessorImpl;

import org.antlr.runtime.CharStream;
import org.antlr.runtime.CommonTokenStream;
import org.antlr.runtime.Lexer;
import org.antlr.runtime.RecognitionException;
import org.antlr.runtime.tree.CommonTree;

import util.unicode.ANTLRFileStreamWithBOM;

public final class Parsing {

	private Parsing() {
		// utility class
	}

	public static CommonTree buildAST(final compilation_unit_return cunit,
			final CommonTokenStream tokens) {
		CommonTree tree = (CommonTree) cunit.getTree();
		// Check if we didn't get an AST
		// This often happens if your grammar and tree grammar don't match
		if (tree == null) {
			if (tokens.size() > 0) {
				System.err.println("No Tree returned from parsing!"
						+ " (Your rule did not parse correctly)");
			} else {
				// the file was empty, this is not an error.
				// Clear archive attribute
				System.out.println("Source file is empty.");
			}
			System.out.println(tokens.getTokens());
		} else {
			// System.out.println("Parsed. OK!");
			// System.out.println("Tree: " + tree.toStringTree());
		}
		return tree;
	}

	public static CommonTree loadASTFromFile(final String filename) throws IOException,
	RecognitionException {
		CharStream input = new ANTLRFileStreamWithBOM(filename, "UTF-8");
		try {
			// "NET_4_0" "NET_2_1"
			Lexer lex = new CSharp4PreProcessorImpl(input, "NET_4_0");
			CommonTokenStream tokens = new CommonTokenStream(lex);
			CSharp4AST parser = new CSharp4AST(tokens);

			compilation_unit_return cunit = parser.compilation_unit();
			CommonTree ast = Parsing.buildAST(cunit, tokens);

			return ast;
		} catch (RuntimeException e) {
			// add filename to the exception
			throw new RuntimeException("Failt to load file '" + filename + "'.", e);
		}
	}
}
