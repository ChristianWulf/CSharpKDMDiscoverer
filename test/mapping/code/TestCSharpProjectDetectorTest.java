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

package mapping.code;

import static org.junit.Assert.assertEquals;

import java.io.IOException;
import java.util.HashSet;

import mapping.project.CSharpProjectDetector;

import org.junit.Test;
import org.xml.sax.SAXException;

import com.google.common.collect.Sets;

public class TestCSharpProjectDetectorTest {

	private static final String	RESOURCE_TEST	= "resource/test/";

	@Test
	public void testGetExternalLibraries() throws SAXException, IOException {
		String projectDirectory = RESOURCE_TEST + "C#Project";
		CSharpProjectDetector cSharpProjectDetector = new CSharpProjectDetector(projectDirectory);

		HashSet<String> expected = Sets.newHashSet("System", "System.Data", "System.Drawing", "System.Management",
				"System.Windows.Forms", "System.Xml");
		assertEquals(expected, cSharpProjectDetector.getUsedExternalLibraries());
	}

	@Test(expected = RuntimeException.class)
	public void testGetExternalLibrariesFromFile() throws SAXException, IOException {
		String projectDirectory = RESOURCE_TEST + "C#Project/NordicAnalytics.csproj";
		new CSharpProjectDetector(projectDirectory);
	}

	@Test(expected = IOException.class)
	public void testGetExternalLibrariesFromNotExistentDir() throws SAXException, IOException {
		String projectDirectory = RESOURCE_TEST + "not_existent";
		new CSharpProjectDetector(projectDirectory);
	}

	@Test(expected = IOException.class)
	public void testGetExternalLibrariesFromNoCSharpProjectDir() throws SAXException, IOException {
		String projectDirectory = RESOURCE_TEST + "testcases";
		new CSharpProjectDetector(projectDirectory);
	}

}
