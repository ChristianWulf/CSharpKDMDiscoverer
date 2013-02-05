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

package example;



public final class TestCS {

	private TestCS() {
		// utility class
	}

	//	public static void main(final String[] args) throws IOException {
	//		String filename;
	//		if (args.length > 0) {
	//			filename = args[0];
	//		} else {
	//			// filename = "Test2.cs";
	//			filename = "csharp-examples/HelloWorld.cs";
	//		}
	//		Lexer lex = new CsRewriteRulesLexer(new ANTLRFileStream(filename));
	//		CommonTokenStream tokens = new CommonTokenStream(lex);
	//		CsRewriteRulesParser parser = new CsRewriteRulesParser(tokens);
	//
	//		try {
	//			System.out.println(tokens.size());
	//			// parse
	//			compilation_unit_return cunit = parser.compilation_unit();
	//			System.out.println("size = " + tokens.size());
	//			System.out.println(cunit.getClass());
	//			System.out.println(cunit.getStart());
	//			System.out.println(cunit.getStop());
	//			System.out.println(cunit.getTemplate());
	//
	//			CommonTree tree = Parsing.buildAST(cunit, tokens);
	//
	//			CSharpLanguageUnitCache languageUnitCache = new CSharpLanguageUnitCache();
	//
	//			Namespace internalGlobalNamespace = KDMElementFactory.createGlobalNamespace();
	//			Namespace externalGlobalNamespace = KDMElementFactory.createGlobalNamespace();
	//			IDatatypeInfoLoader loader = new DatatypeInfoCacheLoader(new DatatypeInfoFileSystemLoader());
	//
	//			String rootDir = "C:/chw/decompiled";
	//			ExternalDatatypeInfoRepository classInfoRepository = new ExternalDatatypeInfoRepository(
	//					rootDir,loader);
	//
	//			FileMapper fileMapper = new FileMapper(languageUnitCache, internalGlobalNamespace,
	//					externalGlobalNamespace, classInfoRepository);
	//			fileMapper.transform(tree);
	//			CompilationUnit compilationUnit = fileMapper.getMappingResult().get(0);
	//
	//			CodeModel codeModel = CodeFactory.eINSTANCE.createCodeModel();
	//			codeModel.getCodeElement().add(languageUnitCache.getLanguageUnit());
	//			codeModel.getCodeElement().add(compilationUnit);
	//
	//			Segment segment = KDMElementFactory.createSegment();
	//			segment.getModel().add(codeModel);
	//
	//			FileAccess.saveEcoreToXMI(segment, "TestCS.xmi");
	//			System.out.println("Saved!");
	//		} catch (RecognitionException e) {
	//			e.printStackTrace();
	//		}
	//	}
	//
	//	private static void printChildren(final List children) {
	//		for (Object child : children) {
	//			CommonTree tree = (CommonTree) child;
	//			// System.out.println(tree.getText());
	//			// System.out.println(tree.getToken());
	//			// System.out.println(tree.getType());
	//			// System.out.println("Child: "+child.getClass());
	//			Token token = tree.getToken();
	//			switch (tree.getType()) {
	//				case CsRewriteRulesParser.USING:
	//					System.out.println("TYPE: using");
	//					System.out.println(tree.getChildren());
	//					break;
	//				case CsRewriteRulesParser.NAMESPACE:
	//					System.out.println("TYPE: namespace");
	//					System.out.println(tree.getChildren());
	//					break;
	//				case CsRewriteRulesParser.CLASS_DECL:
	//					System.out.println("TYPE: class");
	//					System.out.println(tree.getChildren());
	//					break;
	//				// case CSParser.CLASS_MEMBER:
	//				// System.out.println("TYPE: class member");
	//				// System.out.println(tree.getChildren());
	//				// break;
	//				// case CSParser.METHOD:
	//				// System.out.println("TYPE: method definition");
	//				// System.out.println(tree.getChildren());
	//				// break;
	//				case CsRewriteRulesParser.STRUCT_DECL:
	//					System.out.println("TYPE: struct");
	//					System.out.println(tree.getChildren());
	//					break;
	//				default:
	//					System.err.println("Unknown type '" + tree.getType() + "'");
	//			}
	//
	//			if (tree.getChildren() != null) {
	//				printChildren(tree.getChildren());
	//			}
	//		}
	//	}

}
