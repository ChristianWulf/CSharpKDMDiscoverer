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

package mapping.code.language;

import org.eclipse.gmt.modisco.omg.kdm.code.CodeFactory;


public class CSharpLanguageUnitCache extends GenericLanguageUnitCache {

	private static final CodeFactory	CODE_FACTORY	= CodeFactory.eINSTANCE;

	public CSharpLanguageUnitCache() {
		super(CODE_FACTORY.createLanguageUnit());
		init();
	}

	private void init() {
		// see C# specification 4th Edition / June 2006, ECMA 344, page 18
		setName("Common C# predefined types");

		addDatatype("object", CODE_FACTORY.createDatatype());
		addDatatype("dynamic", CODE_FACTORY.createDatatype());
		addDatatype("String", CODE_FACTORY.createStringType());	// TODO remove
		addDatatype("string", CODE_FACTORY.createStringType());
		addDatatype("sbyte", CODE_FACTORY.createOctetType());
		addDatatype("short", CODE_FACTORY.createIntegerType());
		addDatatype("int", CODE_FACTORY.createIntegerType());
		addDatatype("long", CODE_FACTORY.createIntegerType());
		addDatatype("byte", CODE_FACTORY.createOctetType());
		addDatatype("ushort", CODE_FACTORY.createIntegerType());
		addDatatype("uint", CODE_FACTORY.createIntegerType());
		addDatatype("ulong", CODE_FACTORY.createIntegerType());
		addDatatype("float", CODE_FACTORY.createFloatType());
		addDatatype("double", CODE_FACTORY.createFloatType());
		addDatatype("bool", CODE_FACTORY.createBooleanType());
		addDatatype("char", CODE_FACTORY.createCharType());
		addDatatype("decimal", CODE_FACTORY.createDecimalType());
		addDatatype("void", CODE_FACTORY.createVoidType());
	}

}
