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

import java.util.HashMap;
import java.util.Map;

import org.eclipse.gmt.modisco.omg.kdm.code.Datatype;
import org.eclipse.gmt.modisco.omg.kdm.code.LanguageUnit;

public class GenericLanguageUnitCache {

	private final LanguageUnit			languageUnit;
	private final Map<String, Datatype>	cache	= new HashMap<String, Datatype>();

	public GenericLanguageUnitCache(final LanguageUnit languageUnit) {
		this.languageUnit = languageUnit;
	}

	public Datatype getDatatypeFromString(final String datatypeName) throws NotInCacheException {
		Datatype datatype = cache.get(datatypeName);
		if (datatype == null) {
			throw new NotInCacheException();
		}
		return datatype;
	}

	public LanguageUnit getLanguageUnit() {
		return languageUnit;
	}

	public void addDatatype(final String datatypeName, final Datatype datatype) {
		datatype.setName(datatypeName);

		languageUnit.getCodeElement().add(datatype);
		cache.put(datatype.getName(), datatype);
	}

	public void setName(final String name) {
		languageUnit.setName(name);
	}

}
