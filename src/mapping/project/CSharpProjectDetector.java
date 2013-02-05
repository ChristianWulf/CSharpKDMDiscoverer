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

package mapping.project;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.HashSet;
import java.util.LinkedList;
import java.util.List;
import java.util.Set;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.parsers.SAXParser;
import javax.xml.parsers.SAXParserFactory;

import mapping.code.Messages;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.xml.sax.SAXException;
import org.xml.sax.helpers.DefaultHandler;

import util.FileAccess;
import util.FileListener;
import util.Logger;

public class CSharpProjectDetector {

	private static final Logger LOGGER = new Logger(CSharpProjectDetector.class);
	private final Set<String>	externalLibraries	= new HashSet<String>();

	public CSharpProjectDetector(final String projectDirectory) throws SAXException, IOException {
		List<File> projectFiles = getProjectFiles(projectDirectory);
		for (File project : projectFiles) {
			readProjectFile(project);
		}
	}

	private List<File> getProjectFiles(final String projectDirectory) throws IOException {
		File projectDir = new File(projectDirectory);

		if (!projectDir.exists()) {
			throw new FileNotFoundException(projectDir.getAbsolutePath());
		}

		if (!projectDir.isDirectory()) {
			throw new RuntimeException("'" + projectDir + "' is not a directory. Please enter a valid directory.");
		}

		final List<File> csprojFiles = new LinkedList<File>();

		FileListener listener = new FileListener() {
			@Override
			public void updateFile(final File dir, final File file, final IProgressMonitor monitor) {
				if (file.getName().endsWith(Messages.CSharpProjectDetector_0)) {
					csprojFiles.add(file);
				}
			}

			@Override
			public void exitDir(final File parent, final File dir, final IProgressMonitor monitor) {/*
			 * no
			 * need
			 */
			}

			@Override
			public void enterDir(final File parent, final File dir, final IProgressMonitor monitor) {/*
			 * no
			 * need
			 */
			}
		};

		FileAccess.walkDirectoryRecursively(projectDir, listener, new NullProgressMonitor());

		LOGGER.fine("csprojFiles: "+csprojFiles);
		if (csprojFiles.size() == 0) {
			throw new FileNotFoundException("There is no project file in directory '" + projectDirectory + "'.");
		} /*
		 * else if (csprojFiles.length > 1) { throw new
		 * RuntimeException("There is more than one project file. " +
		 * "Can not determine which one to choose automatically. " +
		 * "Please delete one."); }
		 */
		return csprojFiles;
	}

	private void readProjectFile(final File projectFile) throws SAXException, IOException {
		SAXParserFactory saxParserFactory = SAXParserFactory.newInstance();
		SAXParser saxParser;

		try {
			saxParser = saxParserFactory.newSAXParser();
		} catch (ParserConfigurationException e) {
			throw new IllegalStateException(e.getMessage(), e);
		} catch (SAXException e) {
			throw new IllegalStateException(e.getMessage(), e);
		}

		DefaultHandler handler = new CSharpProjectSAXHandler(externalLibraries);
		saxParser.parse(projectFile, handler);
	}

	// non-SAX public methods

	/**
	 * Returns the internal list of external library names.
	 * 
	 * @return
	 */
	public Set<String> getUsedExternalLibraries() {
		return externalLibraries;
	}

}
