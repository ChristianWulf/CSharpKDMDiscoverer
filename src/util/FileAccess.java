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

package util;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.text.DecimalFormat;
import java.util.Collections;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.emf.common.util.EList;
import org.eclipse.emf.common.util.URI;
import org.eclipse.emf.ecore.EObject;
import org.eclipse.emf.ecore.EPackage;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.emf.ecore.resource.ResourceSet;
import org.eclipse.emf.ecore.resource.impl.ResourceSetImpl;
import org.eclipse.emf.ecore.xmi.XMIResource;
import org.eclipse.emf.ecore.xmi.impl.XMIResourceImpl;

public final class FileAccess {

	private final static Logger	LOGGER		= new Logger(FileAccess.class);
	private static final int	BUFFER_SIZE	= 1024;

	private FileAccess() {
		// utility class
	}

	public static void saveEcoreToXMI(final EObject modelInstance, final String filename, final IProgressMonitor monitor)
			throws IOException {
		try {
			FileAccess.saveEcoreToXMIUnsafe(modelInstance, filename, monitor);
		} catch (StackOverflowError e) {
			System.err.println("Could not save model due to StackOverflowError. "
					+ "Increase the default stack size limit for threads by the JVM argument '-Xss'.");
			e.printStackTrace();
		}
	}

	private static void saveEcoreToXMIUnsafe(final EObject modelInstance, final String filename,
			final IProgressMonitor monitor) throws IOException {
		monitor.subTask("Saving model to file..." + filename);
		// create resource set and resource
		// ResourceSet resourceSet = new ResourceSetImpl();
		//
		// // Register XML resource factory
		// resourceSet.getResourceFactoryRegistry().getExtensionToFactoryMap()
		// .put("xmi", new XMIResourceFactoryImpl());
		//
		// Resource resource =
		// resourceSet.createResource(URI.createFileURI(filename));
		//
		// // add the root object to the resource
		// resource.getContents().add(modelInstance);
		// // serialize resource – you can specify also serialization
		// // options which defined on org.eclipse.emf.ecore.xmi.XMIResource
		// resource.save(null);

		// the following adds the xml namespace for kdm automatically, i.e.
		// saving in this way works
		XMIResource resource;
		resource = new XMIResourceImpl();
		resource.setURI(URI.createFileURI(filename));
		resource.setEncoding("UTF-8");
		resource.getContents().add(modelInstance);
		resource.save(Collections.EMPTY_MAP);
	}

	@SuppressWarnings("unchecked")
	public static <T extends EPackage> T loadEcoreFromXMIFile(final T modelClass, final String filename)
			throws IOException {
		ResourceSet resourceSet = new ResourceSetImpl();
		// register package in local resource registry
		resourceSet.getPackageRegistry().put(modelClass.getNsURI(), modelClass);

		Resource resource = resourceSet.createResource(URI.createFileURI(filename));
		// load resource
		resource.load(null);

		EList<EObject> contents = resource.getContents();

		if (contents.size() == 0) {
			throw new IOException("File does not contain anything (" + filename + ")");
		}

		EObject eObject = contents.get(0);
		if (!(eObject instanceof EPackage)) {
			throw new ClassCastException("The file content is not of the type EPackage, but of "
					+ eObject.getClass().getName());
		}

		return (T) eObject;
	}

	public static String loadTextFile(final String filename) throws IOException {
		byte[] buffer = new byte[BUFFER_SIZE];
		int read;
		StringBuilder stringBuilder = new StringBuilder();
		FileInputStream fileInputStream = new FileInputStream(filename);
		try {
			while ((read = fileInputStream.read(buffer)) != -1) {
				stringBuilder.append(new String(buffer, 0, read));
			}
		} finally {
			fileInputStream.close();
		}
		return stringBuilder.toString();
	}

	// TODO return a stream/Reader
	public static BufferedReader loadTextFileAsStream(final String filename) throws FileNotFoundException {
		BufferedReader reader = new BufferedReader(new FileReader(filename));

		// InputStream source = null;
		// String charsetName = null;
		// Scanner scanner = new Scanner(source, charsetName);

		return reader;
	}

	public static void saveTextFile(final String filename, final String text) throws IOException {
		FileWriter fileWriter = new FileWriter(filename);
		try {
			fileWriter.write(text);
		} finally {
			fileWriter.close();
		}
	}

	/**
	 * @param dir
	 *            is not checked initially
	 * @param listener
	 * @param monitor
	 */
	public static void walkDirectoryRecursively(final File dir, final FileListener listener,
			final IProgressMonitor monitor) {
		LOGGER.finer("dir: " + dir);
		for (File file : dir.listFiles()) {
			LOGGER.finer("file: " + file);
			if (file.isDirectory()) {
				listener.enterDir(dir, file, monitor);
				walkDirectoryRecursively(file, listener, monitor);
				listener.exitDir(dir, file, monitor);
			} else {
				listener.updateFile(dir, file, monitor);
			}
		}
	}

	public static String getFileExtension(final File file) {
		String name = file.getName();
		int lastIndexOf = name.lastIndexOf('.');
		if (lastIndexOf == -1) {
			return "";
		}
		// +1 absorb the dot
		String fileExt = name.substring(lastIndexOf + 1).toLowerCase();
		return fileExt;
	}

	public static String getNiceFileSize(double size) {
		int run = 0;
		String[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
		while (size >= 1024) {
			size /= 1024;
			run++;
		}
		DecimalFormat df = new DecimalFormat("0.00");
		String zahl = df.format(size);
		return zahl + " " + sizes[run];
	}

	/**
	 * @param first
	 * @param second
	 * @return
	 */
	public static boolean isFirstYounger(final String first, final String second) {
		File firstFile = new File(first);
		// TODO max of createdTime and lastModified (unfortunately, java 6 does not supported reading creationTime)
		long firstTimestamp = Math.max(firstFile.lastModified(), firstFile.lastModified());
		return firstTimestamp > new File(second).lastModified();
	}
}
