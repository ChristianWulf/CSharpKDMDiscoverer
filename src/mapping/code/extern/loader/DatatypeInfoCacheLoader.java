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

package mapping.code.extern.loader;

import java.util.HashMap;
import java.util.Map;

import mapping.code.extern.entity.DatatypeInfo;
import mapping.code.extern.reference.ReferenceInfo;

/**
 * Decorator: Concrete decorator
 * 
 * @author chw
 */
@Deprecated
public class DatatypeInfoCacheLoader extends DatatypeInfoLoaderDecorator {

	private final Map<String, DatatypeInfo>	cacheMap	= new HashMap<String, DatatypeInfo>();

	public DatatypeInfoCacheLoader(final IDatatypeInfoLoader loader) {
		super(loader);
	}

	@Override
	public DatatypeInfo getDatatypeInfo(final String classFilename)
			throws DatatypeInfoLoadException {
		DatatypeInfo dataTypeInfo = cacheMap.get(classFilename);
		if (dataTypeInfo != null) {
			return dataTypeInfo;
		}
		dataTypeInfo = super.getDatatypeInfo(classFilename);
		cacheMap.put(classFilename, dataTypeInfo);
		return dataTypeInfo;
	}

	@Override
	public Map<String, ReferenceInfo> getReferences(final String datatypeFilename)
			throws DatatypeInfoLoadException {
		return super.getReferences(datatypeFilename);
	}

}
