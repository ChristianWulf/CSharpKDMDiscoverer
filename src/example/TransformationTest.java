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

import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;

import mapping.source.InventoryModelCreator;
import mapping.util.ModelCreationHelper;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Segment;
import org.eclipse.gmt.modisco.omg.kdm.source.InventoryModel;

import util.FileAccess;
import util.MyProperties;

public class TransformationTest {

	IProgressMonitor monitor = new ProgressMonitorExample();

	// On Windows, the default thread stack size is read from the binary
	// (java.exe). As of Java SE
	// 6, this value is 320k in the 32-bit VM and 1024k in the 64-bit VM.
	//
	// You can reduce your stack size by running with the -Xss option. For
	// example:
	//
	// java -server -Xss64k
	//
	// URL: http://www.oracle.com/technetwork/java/hotspotfaq-138619.html

	public static void main(final String[] args) throws IOException {
		TransformationTest test = new TransformationTest();

		InputStream fis = new BufferedInputStream( new FileInputStream("META-INF/logging.properties"));
		//		fis = new FileInputStream("META-INF/logging.properties");
		try {
			fis.read();
		} finally {
			fis.close();
		}

		long start = System.currentTimeMillis();
		// test.transform("resource/test/testcases/5", "HelloWorld5.xmi");
		// test.transform("resource/test/testcases/6", "HelloWorld6.xmi");
		// test.transform("resource/test/testcases/8", "HelloWorld8.xmi");
		// test.transform("resource/test/testcases/10", "HelloWorld10.xmi");
		// test.transform("resource/test/testcases/11", "HelloWorld11.xmi");
		//		test.transform("resource/test/testcases/12", "HelloWorld12.xmi");
		// test.transform("resource/test/testcases/13", "Test13.xmi");
		// test.transform("resource/test/testcases/15", "SimpleName.xmi");
		// test.transform("resource/test/testcases/16", "While.xmi");
		// test.transform("resource/test/testcases/17", "Loop.xmi");
		//		test.transform("resource/test/testcases/18", "CallTestClass.xmi");
		// test.transform("resource/test/testcases/14", "IfTestClass.xmi");
		// test.transform("resource/test/testcases/20", "Region.xmi");
		// test.transform("resource/test/testcases/21", "Test21.xmi");
		//		test.transform("resource/test/testcases/24", "Test24.xmi");
		//		test.transform("resource/example2/", "Example2.xmi");

		//		test.transform("D:/SW-development/NordicAnalytics", "NordicAnalytics.xmi");
		//		test.transform("E:/SUT/SharpDevelop_4.1.0.8000_Source/src", "SharpDevelop.xmi");
		//				test.transform("D:/SW-development/nant-0.91-src/src", "NAnt-p1.xmi");
		test.transform("E:/sw-dev/nant-0.92-beta1/src", "NAnt.xmi");
		//		test.transform("D:/SW-development/rail-20050119/RAIL", "RAIL-p1.xmi");

		//		test.transform("C:/NA_2_0_29/NordicAnalytics/NordicAnalytics", "NordicAnalytics.xmi");
		//		test.transform("C:/chw/workspace/ANTLR/resource/SharpDevelop_4.1.0.8000_Source/src", "SharpDevelop.xmi");

		long end = System.currentTimeMillis();
		System.out.println("Duration: " + (end - start) + " ms");
	}

	private void transform(final String dir, final String outputFilename)
			throws IOException {
		File directory = new File(dir);
		InventoryModel inventoryModel = new InventoryModelCreator().openDirectory(
				directory, new NullProgressMonitor());

		Segment segment = ModelCreationHelper.buildKDMInstance(inventoryModel, monitor, new MyProperties(), 3);

		FileAccess.saveEcoreToXMI(segment, outputFilename, monitor);
	}
}
