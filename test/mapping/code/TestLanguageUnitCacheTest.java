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

package mapping.code;

import static org.junit.Assert.assertEquals;
import mapping.code.language.CSharpLanguageUnitCache;
import mapping.code.language.GenericLanguageUnitCache;
import mapping.code.language.NotInCacheException;

import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.StringType;
import org.junit.BeforeClass;
import org.junit.Test;

import util.test.Assert;

public class TestLanguageUnitCacheTest {

	private static GenericLanguageUnitCache	genericLanguageUnitCache;

	@BeforeClass
	public static void setup() throws Exception {
		genericLanguageUnitCache = new CSharpLanguageUnitCache();
	}

	@Test
	// JUnit FAQ: Uncaught exceptions will cause the test to fail with an error.
	public void testGetDatatypeFromString() throws NotInCacheException {
		Datatype type;
		//		type = languageUnitCache.getDatatypeFromString("object");
		//		ClassUnit objectType = Assert.assertInstanceof(ClassUnit.class, type);
		//		assertEquals("object", objectType.getName());

		type = genericLanguageUnitCache.getDatatypeFromString("string");
		StringType stringType = Assert.assertInstanceof(StringType.class, type);
		assertEquals("string", stringType.getName());

		type = genericLanguageUnitCache.getDatatypeFromString("String");
		StringType stringType2 = Assert.assertInstanceof(StringType.class, type);
		assertEquals("String", stringType2.getName());

		// TODO check the other predefined types (see CSharpLanguageUnitCache)
	}

}
