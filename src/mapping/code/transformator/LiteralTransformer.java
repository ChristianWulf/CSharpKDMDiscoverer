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

package mapping.code.transformator;

import mapping.KDMElementFactory;
import mapping.code.language.GenericLanguageUnitCache;
import mapping.code.language.NotInCacheException;

import org.antlr.runtime.tree.CommonTree;
import org.eclipse.gmt.modisco.omg.kdm.code.AbstractCodeElement;
import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.Module;
import org.eclipse.gmt.modisco.omg.kdm.code.ValueElement;

public class LiteralTransformer {

	private final GenericLanguageUnitCache genericLanguageUnitCache;
	private final Module valueRepository;

	public LiteralTransformer(
			final GenericLanguageUnitCache genericLanguageUnitCache,
			final Module valueRepository) {
		this.genericLanguageUnitCache = genericLanguageUnitCache;
		this.valueRepository = valueRepository;
	}

	private ValueElement getValueFromValueRepository(final String valueName,
			final String ext, final Datatype valueType) {
		for (AbstractCodeElement element : valueRepository.getCodeElement()) {
			ValueElement valueElement = (ValueElement) element;
			if (valueType.equals(valueElement.getType())
					&& (valueName != null ? valueName.equals(valueElement
							.getName()) : true)
							&& (ext != null ? ext.equals(valueElement.getExt()) : true)) {
				return valueElement;
			}
		}
		return null;
	}

	private Datatype getPrimitiveType(final String typeName) {
		try {
			return genericLanguageUnitCache.getDatatypeFromString(typeName);
		} catch (NotInCacheException e) {
			throw new IllegalStateException(e.getMessage(), e);
		}
	}

	public ValueElement transformString(final CommonTree commonTreeNode,
			final AbstractCodeElement parent) {
		// remove first and last '"' TODO find way in grammar
		String stringContent = commonTreeNode.getText().substring(1,
				commonTreeNode.getText().length() - 1);

		Datatype type = getPrimitiveType("string");

		ValueElement stringValue = getValueFromValueRepository(stringContent,
				null, type);
		if (stringValue == null) {
			stringValue = KDMElementFactory.createValue();
			stringValue.setName(stringContent);
			stringValue.setSize(stringContent.length());
			stringValue.setType(type);
			valueRepository.getCodeElement().add(stringValue);
		}
		return stringValue;
	}

	public ValueElement transformInteger(final CommonTree commonTreeNode,
			final AbstractCodeElement parent) {
		String valueName = commonTreeNode.getText();

		Datatype type = getPrimitiveType("int");

		ValueElement intValue = getValueFromValueRepository(null, valueName,
				type);
		if (intValue == null) {
			intValue = KDMElementFactory.createValue();
			intValue.setName("number literal");
			intValue.setExt(valueName);
			// TODO decide size and type by suffix and/or type; as for now
			// assume int with size
			// 4 bytes
			// intValue.setSize(Integer.valueOf(4));
			intValue.setType(type);
			valueRepository.getCodeElement().add(intValue);
		}
		return intValue;
	}

	public ValueElement transformReal(final CommonTree commonTreeNode,
			final AbstractCodeElement parent) {
		String valueName = commonTreeNode.getText();

		Datatype type = getPrimitiveType("float");

		ValueElement realValue = getValueFromValueRepository(null, valueName,
				type);
		if (realValue == null) {
			realValue = KDMElementFactory.createValue();
			realValue.setName("number literal");
			realValue.setExt(valueName);
			// TODO decide size and type by suffix and/or type; as for now
			// assume float with
			// size 4 bytes
			// realValue.setSize(Integer.valueOf(4));
			realValue.setType(type);
			valueRepository.getCodeElement().add(realValue);
		}
		return realValue;
	}

	public ValueElement transformCharacter(final CommonTree commonTreeNode,
			final AbstractCodeElement parent) {
		// remove first and last '"' TODO find way in grammar
		String charContent = commonTreeNode.getText().substring(1,
				commonTreeNode.getText().length() - 1);

		Datatype type = getPrimitiveType("char");

		ValueElement charValue = getValueFromValueRepository(charContent, null,
				type);
		if (charValue == null) {
			charValue = KDMElementFactory.createValue();
			charValue.setName(charContent);
			charValue.setSize(1);
			charValue.setType(type);
			valueRepository.getCodeElement().add(charValue);
		}
		return charValue;
	}

	public ValueElement transformNull(final CommonTree commonTreeNode,
			final AbstractCodeElement parent) {
		String valueName = commonTreeNode.getText();

		// TODO decide size and type by walking back to the declaration
		Datatype type = null;
		// Datatype type = getPrimitiveType("bool");

		ValueElement nullValue = null;
		// nullValue = getValueFromValueRepository(null, valueName, type);
		if (nullValue == null) {
			nullValue = KDMElementFactory.createValue();
			nullValue.setName("null literal");
			nullValue.setExt(valueName);
			// realValue.setSize(Integer.valueOf(4));
			nullValue.setType(type);
			valueRepository.getCodeElement().add(nullValue);
		}
		return nullValue;
	}

	public ValueElement transformBoolean(final CommonTree commonTreeNode,
			final AbstractCodeElement parent) {
		String valueName = commonTreeNode.getText();

		Datatype type = getPrimitiveType("bool");

		ValueElement boolValue = getValueFromValueRepository(null, valueName,
				type);
		if (boolValue == null) {
			boolValue = KDMElementFactory.createValue();
			boolValue.setName("boolean literal");
			boolValue.setExt(valueName);
			// realValue.setSize(Integer.valueOf(4));
			boolValue.setType(type);
			valueRepository.getCodeElement().add(boolValue);
		}
		return boolValue;
	}

}
