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

package mapping.code.extern;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FilenameFilter;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import mapping.KDMElementFactory;
import mapping.code.extern.entity.DatatypeInfo;
import mapping.code.extern.entity.NamespaceInfo;
import mapping.code.extern.loader.DatatypeInfoLoadException;
import mapping.code.extern.loader.IDatatypeInfoLoader;
import mapping.code.extern.reference.ReferenceInfo;
import mapping.util.KeyStringHelper;

import org.eclipse.gmt.modisco.omg.kdm.code.CodeItem;

import util.ListUtil;
import util.Logger;

public class ExternalDatatypeInfoRepository {

	private static final Logger					LOGGER		= new Logger(ExternalDatatypeInfoRepository.class);

	private final String						rootDir;
	private final IDatatypeInfoLoader			datatypeInfoLoader;
	private final Map<String, ReferenceInfo>	repository	= new HashMap<String, ReferenceInfo>();

	private final FilenameFilter				csFilter	= new CSharpFilenameFilter();

	public ExternalDatatypeInfoRepository(final String rootDir, final IDatatypeInfoLoader datatypeInfoLoader) {
		// FIXME change exception to the checked FileNotFoundException
		if (!new File(rootDir).exists()) throw new IllegalArgumentException("File not found: " + rootDir);
		this.rootDir = rootDir;
		this.datatypeInfoLoader = datatypeInfoLoader;
	}

	private String getNamespaceFilename(final List<String> qualifiedNamespace) {
		// assume 15 characters for each namespace identifier in average
		int capacity = rootDir.length() + qualifiedNamespace.size() * 15;
		StringBuilder builder = new StringBuilder(capacity);
		builder.append(rootDir);
		builder.append(File.separator);

		if (qualifiedNamespace.size() > 0) {
			String ident = qualifiedNamespace.get(0);
			builder.append(ident);
			for (int i = 1; i < qualifiedNamespace.size(); i++) {
				builder.append(".");
				ident = qualifiedNamespace.get(i);
				builder.append(ident);
			}
		}

		return builder.toString();
	}

	public File findItem(final String identifier, List<String> qualifiedNamespace) throws FileNotFoundException {
		// System.out.println("qualifiedNamespace: " + qualifiedNamespace);
		String filename = getNamespaceFilename(qualifiedNamespace);
		// System.out.println("findItem: " + filename);
		File dir = new File(filename);
		File[] children = dir.listFiles(csFilter);
		if (children != null) {
			for (File c : children) {
				String name = c.getName();
				if (c.isFile()) { // remove file extension
					name = c.getName().substring(0, c.getName().lastIndexOf("."));
				}

				if (identifier.equals(name)) {
					// System.out.println("Found: " + c);
					return c;
				}
			}
		}

		// if identifier is a namespace part, not a type
		qualifiedNamespace = new LinkedList<String>(qualifiedNamespace);
		qualifiedNamespace.add(identifier);
		filename = getNamespaceFilename(qualifiedNamespace);
		dir = new File(filename);
		LOGGER.fine("\tloading namespace..." + dir);
		// System.out.println("\tloading namespace..." + dir);
		if (dir.exists() && dir.isDirectory()) {
			return dir;
		}

		throw new FileNotFoundException(dir.getAbsolutePath());
	}

	@Deprecated
	public DatatypeInfo loadDatatypeInfoFromDecompiledFile(final File f) throws DatatypeInfoLoadException {
		return datatypeInfoLoader.getDatatypeInfo(f.getAbsolutePath());
	}

	private String buildKey(final String qualifiedPath, final String identifier) {
		if (qualifiedPath.isEmpty()) return identifier;
		return qualifiedPath + "." + KeyStringHelper.normalize(identifier);
	}

	private ReferenceInfo getReferenceInfo(final String qualifiedPath, final String identifier)
			throws FileNotFoundException, DatatypeInfoLoadException, TypeNotFoundException {
		final String key = buildKey(qualifiedPath, identifier);
		// System.out.println("getReferenceInfo.qualifiedPath: " +
		// qualifiedPath);
		// System.out.println("getReferenceInfo.identifier: " + identifier);
		// System.out.println("getReferenceInfo.key: " + key);

		// search in repository
		ReferenceInfo referenceInfo = repository.get(key);
		if (referenceInfo != null) {
			// System.out.println("Get from repo: " + referenceInfo);
			return referenceInfo;
		}
		// if not found, load namespace/ datatype
		// System.out.println("loading..." + key);
		File f = findItem(identifier, ListUtil.splitAtDot(qualifiedPath));
		LOGGER.fine("LOAD " + f);
		// System.out.println("LOAD " + f);
		if (f.isDirectory()) {
			NamespaceInfo ni = new NamespaceInfo(identifier, qualifiedPath);
			repository.put(key, ni);
			return ni;
		}
		Map<String, ReferenceInfo> refs = datatypeInfoLoader.getReferences(f.getAbsolutePath());
		repository.putAll(refs);

		referenceInfo = repository.get(key);
		if (referenceInfo == null) {
			// e.g. the unsupported delegate (method)
			throw new TypeNotFoundException(key);
		}
		return referenceInfo;
	}

	public ReferenceInfo getReferenceInfo(final CodeItem prefix, final String identifier) throws FileNotFoundException,
	DatatypeInfoLoadException, TypeNotFoundException {
		String qualifiedPath = KDMElementFactory.getQualifiedNameAttribute(prefix).getValue();

		return getReferenceInfo(qualifiedPath, identifier);
	}

	public ReferenceInfo getReferenceInfo(final List<String> namespacePrefix, final String identifier)
			throws FileNotFoundException, DatatypeInfoLoadException, TypeNotFoundException {
		String qualifiedPath = ListUtil.combine(namespacePrefix, ".");

		return getReferenceInfo(qualifiedPath, identifier);
	}

}
