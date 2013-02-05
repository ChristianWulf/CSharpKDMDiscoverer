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

package de.cau.chw.csharp.resolution;

import java.io.File;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;

import util.FileAccess;
import util.FileListener;
import util.Logger;
import de.cau.chw.csharp.resolution.read.StaticSourceCodeProfileReader;

/**
 * @author chw
 */
public class MapperFiller {

	private static final Logger LOG = new Logger(MapperFiller.class);

	private Map<String, String> readUsingDirectivesFromFile(final String filename) {
		Map<String, String> usingDirectives = new HashMap<String, String>();
		// TODO
		return usingDirectives;
	}

	private Map<String, KDMEntity> resolveUsingDirectives(final Map<String, String> usingDirectives) {
		Map<String, KDMEntity> resolvedUsingDirectives = new HashMap<String, KDMEntity>();
		for (Entry<String, String> e : usingDirectives.entrySet()) {

		}
		// TODO
		return resolvedUsingDirectives;
	}

	public void fillUsingDirectives(final Mapper mapper) {
		String filename = null;
		Map<String, String> usingDirectivesFromFile = readUsingDirectivesFromFile(filename);
		Map<String, KDMEntity> usingDirectives = resolveUsingDirectives(usingDirectivesFromFile);
		mapper.usingDirectives.putAll(usingDirectives);
	}

	public void fillExternLibraryElements(final Mapper mapper, final String dirName) {
		final StaticSourceCodeProfileReader reader = new StaticSourceCodeProfileReader();

		FileListener fileListener = new FileListener() {
			@Override
			public void updateFile(final File dir, final File file, final IProgressMonitor monitor) {
				if (file.getName().endsWith(".xml")) {
					String filename = file.getAbsolutePath();
					try {
						Map<String, KDMEntity> elements = reader.readSourceCodeProfileFromFile(filename);
						mapper.externLibraryElement.putAll(elements);
					} catch (RuntimeException e) {
						LOG.error("Exception in file '"+filename+"': ");
						throw e;
					}
				}
			}

			@Override
			public void exitDir(final File parent, final File dir, final IProgressMonitor monitor) {
				// not necessary
			}

			@Override
			public void enterDir(final File parent, final File dir, final IProgressMonitor monitor) {
				// not necessary
			}
		};

		FileAccess.walkDirectoryRecursively(new File(dirName), fileListener, new NullProgressMonitor());
	}

	public static void main(final String[] args) {
		Mapper mapper = new Mapper();

		MapperFiller mapperFiller = new MapperFiller();
		mapperFiller.fillExternLibraryElements(mapper, "D:/DecompilationRepository/System");

		System.out.println("externLibraryElement: "+mapper.externLibraryElement.size());

		mapper.initNamespaceFilter();
		System.out.println("namespaces: "+mapper.namespaces.size());
		mapper.initClassFilter();
		System.out.println("classes: "+mapper.classes.size());
		mapper.initEnumFilter();
		System.out.println("enums: "+mapper.enums.size());
		mapper.initInterfaceFilter();
		System.out.println("interfaces: "+mapper.interfaces.size());
		mapper.initDelegateFilter();
		System.out.println("delegates: "+mapper.delegates.size());
	}
}
