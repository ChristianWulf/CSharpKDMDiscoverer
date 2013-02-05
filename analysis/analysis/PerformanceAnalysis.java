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

package analysis;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Collections;
import java.util.LinkedList;
import java.util.List;

import lang.csharp.CSharp4AST;
import lang.csharp.CSharp4PreProcessorImpl;
import mapping.source.InventoryModelCreator;
import mapping.source.InventoryModelWalker;
import mapping.source.visitor.DefaultSourceFileVisitor;
import mapping.util.ModelCreationHelper;

import org.antlr.runtime.CharStream;
import org.antlr.runtime.CommonTokenStream;
import org.antlr.runtime.Lexer;
import org.antlr.runtime.RecognitionException;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.MyProperties;
import util.unicode.ANTLRFileStreamWithBOM;

public class PerformanceAnalysis {

	public static class ParseVisitor extends DefaultSourceFileVisitor {
		public final List<SourceFile> files = new LinkedList<SourceFile>();

		@Override
		public void visitSourceFile(final SourceFile sourceFile) {
			if (!sourceFile.getLanguage().equals("C#"))
				return;
			try {
				//				parse(sourceFile.getPath());
				parseWithAST(sourceFile.getPath());
				files.add(sourceFile);
			} catch (RecognitionException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}

		// depends on project ModifiedGrammar
		//		private void parse(final String filename) throws IOException,
		//		RecognitionException {
		//			CharStream input = new ANTLRFileStreamWithBOM(filename, "UTF-8");
		//
		//			Lexer lex = new CSharp4PreProcessorImpl(input);
		//			CommonTokenStream tokens = new CommonTokenStream(lex);
		//			CSharp4 parser = new CSharp4(tokens);
		//
		//			parser.compilation_unit();
		//		}

		private void parseWithAST(final String filename) throws IOException,
		RecognitionException {
			CharStream input = new ANTLRFileStreamWithBOM(filename, "UTF-8");

			Lexer lex = new CSharp4PreProcessorImpl(input);
			CommonTokenStream tokens = new CommonTokenStream(lex);
			CSharp4AST parser = new CSharp4AST(tokens);

			parser.compilation_unit();
		}
	}

	/**
	 * @param args
	 * @throws InterruptedException
	 * @throws IOException
	 */
	public static void main(final String[] args) throws InterruptedException, IOException {
		String outputFilename = "resource/analysis-results.txt";
		String inputFilename = "resource/analysis-paths-less.txt";

		List<String> paths = PathLoader.loadPaths(inputFilename);
		BufferedWriter writer = new BufferedWriter(new FileWriter(outputFilename));
		try {
			for (String dirname : paths) {
				IProgressMonitor monitor = new NullProgressMonitor();

				InventoryModelCreator inventoryModelCreator = new InventoryModelCreator();
				InventoryModel inventoryModel = inventoryModelCreator.openDirectory(
						dirname, monitor);

				InventoryModelWalker walker = new InventoryModelWalker(inventoryModel);
				ParseVisitor visitor = new ParseVisitor();

				List<Long> times = new LinkedList<Long>();

				String analyzing = "Start analyzing..." + dirname;
				System.out.println(analyzing);
				writer.append(analyzing + "\n");

				Thread.sleep(500);

				final int COUNT = 6;
				int count = COUNT;
				while (count > 0) {
					long start = System.currentTimeMillis();
					//					walker.walk(visitor);
					ModelCreationHelper.buildKDMInstance(inventoryModel, monitor, new MyProperties(), 3);
					long end = System.currentTimeMillis();
					count--;

					Long duration = Long.valueOf(end - start);
					times.add(duration);
					System.out.print(duration + ", ");
				}
				System.out.println();

				times.remove(0); // remove first due to initialization overhead
				String allTimes = times.toString();
				String avgDuration = "Average Duration: " + avg(times) + " ms";
				String medianDuration = "Median Duration: " + median(times) + " ms";
				String fileCount = "Files: " + visitor.files.size() / COUNT;

				System.out.println(allTimes);
				writer.append(allTimes + "\n");

				System.out.println(avgDuration);
				writer.append(avgDuration + "\n");

				System.out.println(medianDuration);
				writer.append(medianDuration + "\n");

				System.out.println(fileCount);
				writer.append(fileCount + "\n");

				// CodeModelCreator codeModelCreator = new CodeModelCreator();
				// codeModelCreator.buildCodeModel(inventoryModel, monitor);
			}
		} finally {
			writer.close();
		}
	}

	private static long avg(final List<Long> times) {
		long sum = 0;
		for (Long t : times) {
			sum += t;
		}
		return sum / times.size();
	}

	private static Long median(final List<Long> times) {
		Collections.sort(times);
		int midIndex = times.size() / 2;
		if (times.size() % 2 == 0) {
			long sumMids = times.get(midIndex) + times.get(midIndex - 1);
			return sumMids / 2;
		}
		return times.get(midIndex);
	}
}
