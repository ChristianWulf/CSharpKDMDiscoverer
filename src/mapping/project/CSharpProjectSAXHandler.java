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

package mapping.project;

import java.util.Collection;
import java.util.Deque;
import java.util.LinkedList;

import org.xml.sax.Attributes;
import org.xml.sax.SAXException;
import org.xml.sax.helpers.DefaultHandler;

class CSharpProjectSAXHandler extends DefaultHandler {

	private static final String	REFERENCE		= "Reference";
	private final Deque<String>	elementStack	= new LinkedList<String>();
	private final Collection<String>	externalLibraries;

	public CSharpProjectSAXHandler(final Collection<String> externalLibraries) {
		this.externalLibraries = externalLibraries;
	}

	@Override
	public void startElement(final String uri, final String localName, final String qName,
			final Attributes attributes) throws SAXException {
		// if ("Project".equals(qName)) {
		// String value = attributes.getValue("xmlns");
		// System.out.println("xmlns=" + value);
		// }

		elementStack.push(qName);

		if (!REFERENCE.equals(qName)) {
			return;
		}
		// System.out.println("URI: " + uri);
		// System.out.println("localName: " + localName);
		// System.out.println("qName: " + qName);
		// System.out.println("attributes.getLength(): " + attributes.getLength());
		//
		String includeValue = attributes.getValue("Include");
		// System.out.println("inputValue: " + includeValue);
		externalLibraries.add(includeValue);
	}

	@Override
	public void endElement(final String uri, final String localName, final String qName)
			throws SAXException {
		elementStack.pop();
	}
}
