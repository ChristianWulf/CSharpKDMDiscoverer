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

package util.test;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

import java.util.Arrays;
import java.util.List;

import org.eclipse.emf.common.util.EList;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.DerivedType;
import org.eclipse.gmt.modisco.omg.kdm.code.MemberUnit;
import org.eclipse.gmt.modisco.omg.kdm.kdm.Attribute;

public final class Assert {

	private Assert() {
		// utility class
	}

	/**
	 * Asserts that <code>codeElement</code> is an instance of the given <code>clazz</code>. If it
	 * is so, the <code>codeElement</code> is casted to <code>clazz</code> and returned.
	 * 
	 * @param clazz
	 * @param codeElement
	 * @return
	 */
	public static <S, T extends S> T assertInstanceof(final Class<T> clazz, final S codeElement) {
		try {
			return clazz.cast(codeElement);
		} catch (ClassCastException e) {
			fail("ClassCastException: (" + clazz.getSimpleName() + ") "
					+ codeElement.getClass().getSimpleName());
			return null; // only for compiler logic
		}
	}

	public static void assertFieldDeclaration(final List<String> modifiers,
			final List<Class<? extends Datatype>> typeComponents, final String varName,
			final MemberUnit memberUnit) {
		EList<Attribute> attributes = memberUnit.getAttribute();

		assertEquals("Missing MoDisco attribute for modifiers", 1, attributes.size());
		// storableUnit.getExt(); // don't know what it means
		String[] moDiscoModifiers = attributes.get(0).getValue().split("\\s");
		assertEquals(varName + ": " + Arrays.asList(moDiscoModifiers).toString(), modifiers.size(),
				moDiscoModifiers.length);
		//		if (modifiers.contains("static")) {
		//			assertEquals(StorableKind.STATIC, memberUnit.getExport());
		//		}
		assertEquals(varName, memberUnit.getName());
		// storableUnit.getSize(); // don't know what it means

		Datatype type = memberUnit.getType();
		int index = 0;
		while (type != null) {
			try {
				assertInstanceof(typeComponents.get(index), type);

				if (type instanceof DerivedType) {
					type = ((DerivedType) type).getItemUnit().getType();
					index++;
				} else {
					type = null;
				}
			} catch (NullPointerException e) {
				fail("NullPointerException for " + typeComponents + ". type.getType() is not set.");
			}
		}

	}
}
