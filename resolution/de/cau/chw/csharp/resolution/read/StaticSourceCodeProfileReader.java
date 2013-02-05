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

package de.cau.chw.csharp.resolution.read;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import mapping.KDMElementFactory;

import org.eclipse.gmt.modisco.omg.kdm.code.ClassUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.EnumeratedType;
import org.eclipse.gmt.modisco.omg.kdm.code.InterfaceUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MemberUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.MethodUnit;
import org.eclipse.gmt.modisco.omg.kdm.code.Namespace;
import org.eclipse.gmt.modisco.omg.kdm.core.KDMEntity;
import org.eclipse.gmt.modisco.omg.kdm.core.impl.KDMEntityImpl;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.xml.sax.SAXException;

import util.ListUtil;
import util.Logger;

/**
 * @author chw
 */
public class StaticSourceCodeProfileReader {

	private static final Logger	LOG	= new Logger(StaticSourceCodeProfileReader.class);

	public Map<String, KDMEntity> readSourceCodeProfileFromFile(final String filename) {
		// read the elements of the file
		List<Node> elements = null;
		try {
			elements = readElementsFromFile(filename);
		} catch (ParserConfigurationException e) {
			LOG.error(e);
		} catch (SAXException e) {
			LOG.error(filename, e);
		} catch (IOException e) {
			LOG.error(e);
		}
		// transform each line to a corresponding KDM element
		Map<String, KDMEntity> entities = transformToKDMElements(elements);
		return entities;
	}

	/**
	 * @param elements
	 * @return
	 */
	private Map<String, KDMEntity> transformToKDMElements(final List<Node> elements) {
		Map<String, KDMEntity> entities = new HashMap<String, KDMEntity>();
		for (Node ele : elements) {
			String id = ele.getAttributes().getNamedItem("id").getNodeValue();
			entities.put(id, transformToKDMEntity(ele));
		}
		return entities;
	}

	/**
	 * @param ele
	 * @return
	 */
	private KDMEntity transformToKDMEntity(final Node ele) {
		KDMEntity entity;
		String tagName = ele.getNodeName();
		String id = ele.getAttributes().getNamedItem("id").getNodeValue();
		if (tagName.equals("class")) {
			ClassUnit cUnit = KDMElementFactory.createClassUnit();
			Boolean isAbstract = Boolean.parseBoolean(ele.getAttributes().getNamedItem("abstract").getNodeValue());
			cUnit.setIsAbstract(isAbstract);

			entity = cUnit;
		} else if (tagName.equals("interface")) {
			InterfaceUnit iUnit = KDMElementFactory.createInterfaceUnit();

			entity = iUnit;
		} else if (tagName.equals("delegate")) {
			Datatype dUnit = KDMElementFactory.createDelegate();

			entity = dUnit;
		} else if (tagName.equals("enum")) {
			EnumeratedType eUnit = KDMElementFactory.createEnum();

			entity = eUnit;
		} else if (tagName.equals("struct")) {
			ClassUnit sUnit = KDMElementFactory.createStruct();
			Boolean isAbstract = Boolean.parseBoolean(ele.getAttributes().getNamedItem("abstract").getNodeValue());
			sUnit.setIsAbstract(isAbstract);

			entity = sUnit;
		} else if (tagName.equals("method")) {
			MethodUnit mUnit = KDMElementFactory.createMethodUnit();
			// FIXME search in this repository before creating a new data type
			String name = ele.getAttributes().getNamedItem("returnType").getNodeValue();
			Datatype returnType = KDMElementFactory.createDatatype(name);
			mUnit.setType(returnType);

			entity = mUnit;
		} else if (tagName.equals("field")) {
			MemberUnit mUnit = KDMElementFactory.createMemberUnit();

			entity = mUnit;
		} else if (tagName.equals("namespace")) {
			Namespace nUnit = KDMElementFactory.createNamespaceUnit(KDMElementFactory
					.createFullyQualifiedNameAttribute(id));

			entity = nUnit;
		} else {
			LOG.unsupported("Unknown XML element '" + tagName + "'");
			entity = new KDMEntityImpl() {
			};
		}
		return entity;
	}

	/**
	 * @param filename
	 * @return
	 * @throws ParserConfigurationException
	 * @throws IOException
	 * @throws SAXException
	 */
	private List<Node> readElementsFromFile(final String filename) throws ParserConfigurationException, SAXException,
	IOException {
		DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
		//		dbf.setValidating(true);
		DocumentBuilder db = dbf.newDocumentBuilder();
		Document xmlDoc = db.parse(new File(filename));
		xmlDoc.getDocumentElement().normalize();

		return ListUtil.list(xmlDoc.getDocumentElement().getElementsByTagName("*"));
	}
}
