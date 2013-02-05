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

package de.cau.chw.csharp.resolution.build;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import mapping.code.extern.DatatypeInfoMapper;
import mapping.code.extern.entity.DatatypeInfo;
import mapping.code.extern.entity.FieldInfo;
import mapping.code.extern.entity.MethodInfo;
import mapping.code.extern.entity.NamespaceInfo;
import mapping.code.extern.reference.ReferenceInfo;

import org.antlr.runtime.RecognitionException;
import org.antlr.runtime.tree.CommonTree;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.NullProgressMonitor;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import util.FileAccess;
import util.FileListener;
import util.ListUtil;
import util.Logger;
import util.Parsing;

/**
 * @author chw
 */
public class StaticSourceCodeProfileBuilder {

	private static final Logger	LOG	= new Logger(StaticSourceCodeProfileBuilder.class);

	public void updateStaticProfileFromSourceFile(final String filename) throws IOException, RecognitionException {
		if (FileAccess.isFirstYounger(filename, filename + ".xml")) {
			buildStaticProfileFromSourceFile(filename);
		}
	}

	public void buildStaticProfileFromSourceFile(final String filename) throws IOException, RecognitionException {
		// build the AST from file
		CommonTree ast = getASTFromFile(filename);
		// extract defined namespaces, types, and members as strings (a.k.a. the profile of the
		// source file)
		Map<String, ReferenceInfo> profile = extractProfileFromAST(ast);
		// save the profile to a file equally named as the source file with extension 'txt'/'xml'
		// saveProfileToTextFile(profile.keySet(), filename + ".txt");
		saveProfileToXmlFile(profile, filename + ".xml");
	}

	/**
	 * @param profile
	 * @param string
	 */
	private void saveProfileToXmlFile(final Map<String, ReferenceInfo> profile, final String filename) {
		DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
		DocumentBuilder docBuilder;
		try {
			docBuilder = docFactory.newDocumentBuilder();
		} catch (ParserConfigurationException e) {
			LOG.error(e);
			return;
		}

		// root element
		Document doc = docBuilder.newDocument();
		Element rootElement = doc.createElement("elements");
		doc.appendChild(rootElement);

		for (Entry<String, ReferenceInfo> entry : profile.entrySet()) {
			// extract info from ReferenceInfo
			String tagName = getTagNameFromReferenceInfo(entry.getValue());
			Map<String, String> attributes = getAttributesFromReferenceInfo(entry.getValue());

			// build XML element with corresponding attributes
			Element ele = doc.createElement(tagName);
			ele.setAttribute("id", entry.getKey());
			for (Entry<String, String> a : attributes.entrySet()) {
				ele.setAttribute(a.getKey(), a.getValue());
			}

			rootElement.appendChild(ele);
		}

		// write the content into xml file
		TransformerFactory transformerFactory = TransformerFactory.newInstance();
		transformerFactory.setAttribute("indent-number", 4); // TODO does not work
		Transformer transformer;
		try {
			transformer = transformerFactory.newTransformer();
			transformer.setOutputProperty(OutputKeys.INDENT, "yes");
			transformer.transform(new DOMSource(doc), new StreamResult(new File(filename)));
		} catch (TransformerException e) {
			LOG.error(e);
			return;
		}
	}

	/**
	 * @param value
	 * @return
	 */
	private Map<String, String> getAttributesFromReferenceInfo(final ReferenceInfo info) {
		Map<String, String> attributes = new HashMap<String, String>();
		if (info instanceof DatatypeInfo) {
			attributes.put("abstract", ((DatatypeInfo) info).isAbstract().toString());
		} else if (info instanceof FieldInfo) {
			attributes.put("type", ((FieldInfo) info).getType().getType().toString());
		} else if (info instanceof MethodInfo) {
			MethodInfo methodInfo = (MethodInfo) info;
			attributes.put("returnType", methodInfo.getReturnType());
		} else if (info instanceof NamespaceInfo) {
			// no special attributes
		} else {
			LOG.unsupported("ReferenceInfo subclass '" + info + "'");
		}
		return attributes;
	}

	/**
	 * @param value
	 * @return
	 */
	private String getTagNameFromReferenceInfo(final ReferenceInfo info) {
		if (info instanceof DatatypeInfo) {
			return ((DatatypeInfo) info).getType();
		} else if (info instanceof FieldInfo) {
			return "field";
		} else if (info instanceof MethodInfo) {
			return "method";
		} else if (info instanceof NamespaceInfo) {
			return "namespace";
		} else {
			LOG.unsupported("ReferenceInfo subclass '" + info + "'");
			return "unsupported";
		}
	}

	/**
	 * @param string
	 * @param profile
	 * @param profile
	 * @param string
	 * @throws IOException
	 */
	private void saveProfileToTextFile(final Set<String> profile, final String filename) throws IOException {
		String text = ListUtil.combine(profile, "\n");
		FileAccess.saveTextFile(filename, text);
	}

	/**
	 * @param ast
	 * @return
	 */
	private Map<String, ReferenceInfo> extractProfileFromAST(final CommonTree ast) {
		DatatypeInfoMapper datatypeInfoMapper = new DatatypeInfoMapper();
		datatypeInfoMapper.transform(ast);
		// TODO performance: for reading the keys only, we do not build ReferenceInfo instances
		Map<String, ReferenceInfo> references = datatypeInfoMapper.getReferences();
		return references;
	}

	/**
	 * @param filename
	 * @return
	 * @throws RecognitionException
	 * @throws IOException
	 */
	private CommonTree getASTFromFile(final String filename) throws IOException, RecognitionException {
		return Parsing.loadASTFromFile(filename);
	}

	public static void main(final String[] args) {
		// runForFile();
		runForFolder();
	}

	private static void runForFile() {
		File file = new File("D:/DecompilationRepository/System/Buffer.cs");
		if (!file.isFile()) throw new IllegalArgumentException();

		StaticSourceCodeProfileBuilder builder = new StaticSourceCodeProfileBuilder();
		try {
			builder.updateStaticProfileFromSourceFile(file.getAbsolutePath());
		} catch (Exception e) {
			LOG.error(e);
		}
	}

	private static void runForFolder() {
		File decompilationDir = new File("D:/DecompilationRepository/System");
		if (!decompilationDir.isDirectory()) throw new IllegalArgumentException();

		FileListener listener = new FileListener() {
			private final Logger			LOG		= new Logger(FileListener.class);
			StaticSourceCodeProfileBuilder	builder	= new StaticSourceCodeProfileBuilder();

			@Override
			public void updateFile(final File dir, final File file, final IProgressMonitor monitor) {
				if (file.getName().endsWith(".cs")) {
					try {
						builder.updateStaticProfileFromSourceFile(file.getAbsolutePath());
					} catch (Exception e) {
						LOG.error(e);
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
		FileAccess.walkDirectoryRecursively(decompilationDir, listener, new NullProgressMonitor());
	}
}
