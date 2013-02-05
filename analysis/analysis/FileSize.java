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
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.LinkedList;
import java.util.List;

import mapping.source.InventoryModelCreator;
import mapping.source.InventoryModelWalker;
import mapping.source.visitor.DefaultSourceFileVisitor;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;
import org.eclipse.gmt.modisco.omg.kdm.source.SourceFile;

import util.FileAccess;

public class FileSize {

	public static class FileSizeWalker extends DefaultSourceFileVisitor {

		public final List<Long>	sizes	= new LinkedList<Long>();

		@Override
		public void visitSourceFile(final SourceFile sourceFile) {
			if (!sourceFile.getLanguage().equals("C#")) return;
			String filename = sourceFile.getPath();
			File file = new File(filename);
			sizes.add(file.length());
		}
	}

	public static void main(final String[] args) throws IOException {
		String outputFilename = "resource/filesize-results.txt";
		String inputFilename = "resource/analysis-paths-less.txt";

		BufferedWriter writer = new BufferedWriter(new FileWriter(outputFilename));
		try {
			List<String> paths = PathLoader.loadPaths(inputFilename);
			for (String dirname : paths) {
				IProgressMonitor monitor = new NullProgressMonitor();

				InventoryModelCreator inventoryModelCreator = new InventoryModelCreator();
				InventoryModel inventoryModel = inventoryModelCreator.openDirectory(dirname,
						monitor);

				FileSizeWalker visitor = new FileSizeWalker();
				InventoryModelWalker walker = new InventoryModelWalker(inventoryModel);

				String analyzing = "Start analyzing..." + dirname;
				System.out.println(analyzing);
				writer.append(analyzing + "\n");

				walker.walk(visitor);

				long size = 0;
				for (Long s : visitor.sizes) {
					size += s;
				}

				String overallSize = "Overall file size: " + FileAccess.getNiceFileSize(size);
				System.out.println(overallSize);
				writer.append(overallSize + "\n");
			}
		} finally {
			writer.close();
		}
	}

	// D:/SW-development/NordicAnalytics/Engine
	// 10,51 MB
	//
}
